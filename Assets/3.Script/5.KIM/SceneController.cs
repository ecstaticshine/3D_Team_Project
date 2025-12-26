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
    ScoreScene,
    Stage1,
    Stage2,
    Loading
}


public static class SceneNameMap
{
    public static string Get(SceneName scene)
    {
        return scene switch
        {
            SceneName.Title =>"Title",
            SceneName.Prologue => "Prologue",
            SceneName.Training => "TrainingScene 1",
            SceneName.ScoreScene => "ScoreScene",
            SceneName.Stage1 => "Stage1",
            SceneName.Stage2 => "Stage2",
            SceneName.Loading => "LoadingScene",
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

    [Header("Settings")]
    [SerializeField] private float minLoadingTime = 2f;

    //Skip 가능
    public bool canActivateScene = false;

    //로딩 100퍼 확인 변수
    public bool isLoadingVisualDone { get; private set; } = false;

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

    public void LoadScene(SceneName targetSceneName)
    {
        canActivateScene = false;
        isLoadingVisualDone = false;
        StartCoroutine(LoadSceneProcess_co(targetSceneName));
    }

    private IEnumerator LoadSceneProcess_co(SceneName targetSceneName)
    {

        // UI 요소가 연결되어 있는지 꼭 확인하는 마법의 방어 코드예요
        if (loadingCanvas == null || progressBar == null)
        {
            yield break;
        }
        loadingCanvas.SetActive(true);
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

            progressBar.value = Mathf.MoveTowards(progressBar.value, targetProgress, timer);

            if (progressText != null)
            {
                progressText.text = $"Loading~~~{Mathf.FloorToInt(progressBar.value * 100)}%";
            }

            if (progressBar.value >= 0.99f) isLoadingVisualDone = true;

            if (operation.progress>=0.9f && isLoadingVisualDone)
            {
                if (timer >= minLoadingTime && canActivateScene)
                {
                    operation.allowSceneActivation = true;
                    loadingCanvas.SetActive(false);
                }
            }
        }
    }


}
