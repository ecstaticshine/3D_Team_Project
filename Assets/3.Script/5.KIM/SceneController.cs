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
    Lobby,
    Game,
    Loading
}


public static class SceneNameMap
{
    public static string Get(SceneName scene)
    {
        return scene switch
        {
            SceneName.Title =>"TitleScene",
            SceneName.Lobby => "LobbyScene",
            SceneName.Game => "GameScene",
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
    [SerializeField] private GameObject lodingCanvas;
    [SerializeField] private Image progressBar;
    [SerializeField] private TextMeshProUGUI progressText;

    [Header("Settings")]
    [SerializeField] private float minLoadingTime = 2f;

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
        StartCoroutine(LoadSceneProcess_co(targetSceneName));
    }

    private IEnumerator LoadSceneProcess_co(SceneName targetSceneName)
    {
        lodingCanvas.SetActive(true);
        progressBar.fillAmount = 0;

        // 1. 매핑 클래스에서 실제 씬 파일 이름 가져옵니다
        string scenePath = SceneNameMap.Get(targetSceneName);

        // 비동기 로드가 시작됩니다.
        AsyncOperation operation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(scenePath);
        operation.allowSceneActivation = false; // 90%에서 대기

        float timer = 0f;
        while (operation.isDone)
        {
            yield return null;
            timer += Time.unscaledDeltaTime;

            float targetProgress = Mathf.Clamp01(operation.progress / 0.9f);

            progressBar.fillAmount = Mathf.MoveTowards(progressBar.fillAmount, targetProgress, timer);
            if (progressText != null)
                progressText.text = $"Loading~~~{Mathf.FloorToInt(progressBar.fillAmount * 100)}%";

            if(operation.progress>=0.9f && progressBar.fillAmount >= 0.99f)
            {
                if (timer >= minLoadingTime)
                {
                    operation.allowSceneActivation = true;
                }
            }
        }
    }


}
