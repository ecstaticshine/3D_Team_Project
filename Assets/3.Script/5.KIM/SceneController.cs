using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

// 씬 이름 정의를 enum으로 활용(이름 추가하고 싶으면 여기다가 추가)
public enum SceneName
{
    Title,
    Prologue,
    Training,
    Score,
    Stage1TimeLine,
    Stage1,
    Stage2TimeLine,
    Stage2,
    Loading,
    GameClear
}


public static class SceneNameMap
{
    public static string Get(SceneName scene)
    {
        return scene switch
        {
            SceneName.Title => "Title",
            SceneName.Prologue => "Prologue",
            SceneName.Training => "TrainingScene 1",
            SceneName.Score => "Score",
            SceneName.Stage1 => "Stage1",
            SceneName.Stage1TimeLine => "Stage1TimeLine",
            SceneName.Stage2 => "Stage2",
            SceneName.Stage2TimeLine => "Stage2TimeLine",
            SceneName.Loading => "LoadingScene",
            SceneName.GameClear => "GameClear",
            //씬을 새로 추가하고 싶으시면 여기에 넣으시면 됩니다

            _ => throw new ArgumentOutOfRangeException(nameof(scene), scene, null)
        };
    }
}


public class SceneController : MonoBehaviour
{
    public static SceneController Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject loadingCanvas;
    [SerializeField] private Slider progressBar;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private GameObject skipGuideText;

    [Header("Settings")]
    [SerializeField] private float minLoadingTime = 2f;

    [Header("수동 전환 씬 설정")]
    [SerializeField]
    private List<SceneName> enterToSceneList = new List<SceneName>
    {
        SceneName.Training,
        SceneName.Stage1TimeLine,
        SceneName.Stage2TimeLine,
    };

    //Skip 가능
    private bool canActivateScene = false;

    private AsyncOperation currentOperation;

    //로딩 100퍼 확인 변수
    public bool isLoadingVisualDone { get; private set; } = false;

    //실행 중인 코루틴, 중복 불가 
    private Coroutine currentLoadingCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            canActivateScene = true;
        }
    }

    public void LoadScene(SceneName targetScene, bool showUI = true, bool forceActivation = true)
    {
        //씬 2번 불리지 않도록 제어
        if (currentLoadingCoroutine != null)
        {
            return;
        }
        canActivateScene = false;

        isLoadingVisualDone = false;
        currentLoadingCoroutine = StartCoroutine(LoadSceneProcess_co(targetScene, showUI, forceActivation));
    }

    public void LoadNextScene()
    {
        SceneName currentScene = (SceneName)SceneManager.GetActiveScene().buildIndex;
        SceneName nextScene = (SceneName)((int)currentScene + 1);

        if ((int)nextScene < Enum.GetNames(typeof(SceneName)).Length)
        {
            LoadScene(nextScene);
        }
    }

    private IEnumerator LoadSceneProcess_co(SceneName targetSceneName, bool showUI, bool forceActivation)
    {
        //UI 빼먹었는지 확인
        bool hasUI = (loadingCanvas != null && progressBar != null);

        //  UI 보이기에 체크되있고 UI가 있어야 로딩 캔버스 표시 
        if (showUI && hasUI)
        {
            loadingCanvas.SetActive(true);
        }

        //리스트에 없으면 자동, 있으면 수동
        bool isManualTransition = !forceActivation && enterToSceneList.Contains(targetSceneName);

        if (hasUI)
        {
            if (isManualTransition && showUI)
            {
                loadingCanvas.SetActive(true);
                skipGuideText.SetActive(true);
            }
            else
            {
                loadingCanvas.SetActive(false);
                skipGuideText.SetActive(false);
            }
        }
        progressBar.value = 0;

        // 1. 매핑 클래스에서 실제 씬 파일 이름 가져옵니다
        string scenePath = SceneNameMap.Get(targetSceneName);

        // 비동기 로드가 시작됩니다.
        currentOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(scenePath);
        currentOperation.allowSceneActivation = false; // 90%에서 대기
        float timer = 0f;

        while (currentOperation.progress < 0.9f || (hasUI && progressBar.value < 0.99f))
        {
            yield return null;
            timer += Time.unscaledDeltaTime;
            float targetProgress = Mathf.Clamp01(currentOperation.progress / 0.9f);

            if (hasUI)
            {
                progressBar.value = Mathf.MoveTowards(progressBar.value, targetProgress, Time.unscaledDeltaTime * 2f);
                if (progressText != null)
                    progressText.text = $"Loading...{Mathf.FloorToInt(progressBar.value * 100)}%";
            }
        }

        isLoadingVisualDone = true;

        // [2단계] 수동 전환 대기 (수동 리스트에 있고 자동 전환이 아닐 때)
        if (isManualTransition)
        {
            // 수동 씬: 가이드를 띄우고 Enter(canActivateScene)를 기다림
            if (hasUI && skipGuideText != null) skipGuideText.SetActive(true);

            // 엔터를 누르거나 + 최소 로딩시간이 지날 때까지 대기
            while (!canActivateScene || timer < minLoadingTime)
            {
                yield return null;
                timer += Time.unscaledDeltaTime;
            }
        }
        else
        {
            // 자동 씬: 엔터 상관없이 최소 로딩시간만 채우면 즉시 통과
            while (timer < minLoadingTime)
            {
                yield return null;
                timer += Time.unscaledDeltaTime;
            }
            canActivateScene = true;
        }

        currentOperation.allowSceneActivation = true;

        // 씬이 완전히 바뀔 때까지 대기
        while (!currentOperation.isDone)
        {
            yield return null;
        }

        // 후처리
        if (AudioManager.instance != null) AudioManager.instance.PlayBGMByScene(targetSceneName);
        if (hasUI) loadingCanvas.SetActive(false);
        currentLoadingCoroutine = null;
    }

}