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
        SceneName.Prologue,
        SceneName.Training,
        SceneName.Stage1TimeLine,
        SceneName.Stage2TimeLine
    };

    //Skip 가능
    public bool canActivateScene = false;

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

    public void LoadScene(SceneName targetScene)
    {
        //씬 2번 불리지 않도록 제어
        if (currentLoadingCoroutine != null)
        {
            return;
        }
        

        isLoadingVisualDone = false;
        currentLoadingCoroutine = StartCoroutine(LoadSceneProcess_co(targetScene));
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

    private IEnumerator LoadSceneProcess_co(SceneName targetSceneName)
    {

        canActivateScene = false;

        if (!enterToSceneList.Contains(targetSceneName))
        {
            canActivateScene = true; // 수동 리스트에 없으면 자동으로 넘어가짐
        }
       

        // UI 요소가 연결되어 있는지 꼭 확인하는 마법의 방어 코드예요
        if (loadingCanvas == null || progressBar == null)
        {
            currentLoadingCoroutine = null;
            yield break;
        }
        loadingCanvas.SetActive(true);
        skipGuideText.SetActive(false);
        progressBar.value = 0;

        // 1. 매핑 클래스에서 실제 씬 파일 이름 가져옵니다
        string scenePath = SceneNameMap.Get(targetSceneName);

        // 비동기 로드가 시작됩니다.
        AsyncOperation operation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(scenePath);
        operation.allowSceneActivation = false; // 90%에서 대기
        float timer = 0f;

        while (!operation.isDone)
        {
            yield return null;
            timer += Time.unscaledDeltaTime;

            float targetProgress = Mathf.Clamp01(operation.progress / 0.9f);

            progressBar.value = Mathf.MoveTowards(progressBar.value, targetProgress, Time.unscaledDeltaTime * 2f);

            if (progressText != null)
            {
                progressText.text = $"Loading~~~{Mathf.FloorToInt(progressBar.value * 100)}%";
            }

            if (progressBar.value >= 0.98f)
            {
                isLoadingVisualDone = true;

                if (!canActivateScene)
                {
                    if (skipGuideText != null)
                    {
                        skipGuideText.SetActive(true);
                    }
                    if (Input.GetKeyDown(KeyCode.Return))
                    {
                        canActivateScene = true;
                    }
                }

                if (canActivateScene && timer >= minLoadingTime)
                {
                    operation.allowSceneActivation = true;
                }

            }
            if (operation.isDone)
            {
                if (AudioManager.instance != null)
                {
                    AudioManager.instance.PlayBGMByScene(targetSceneName);
                }

                loadingCanvas.SetActive(false);
                currentLoadingCoroutine = null;
                yield break;

            }
        }
        currentLoadingCoroutine = null;
    }

}
