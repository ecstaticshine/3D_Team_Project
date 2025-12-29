using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class ScoreSceneUI : MonoBehaviour
{
    [Header("Clock Settings")]
    [SerializeField] private Transform clockHand;
    [SerializeField] private float startAngle = 45f;
    [SerializeField] private float stepAngle = 15f;
    [SerializeField] private float finalAngle = 120f;
    [SerializeField] private float rotateSpeed = 2f;

    [Header("Score Items (순서 중요!)")]
    [SerializeField] private List<GameObject> scoreTexts;

    [Header("Final Result")]
    [SerializeField] private GameObject totalScoreObject;
    [SerializeField] private Button continueButton;

    public string nextSceneName;
    private WaitForSeconds _waitTick = new WaitForSeconds(0.3f);

    private void Start()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.ResumeGame();
            GameManager.instance.canPause = false;
        }

        InitializeUI();

        StartCoroutine(SequenceScoreAnimation());
    }

    private void InitializeUI()
    {
        clockHand.localRotation = Quaternion.Euler(0, startAngle - stepAngle, 0);

        ApplyScoreData();

        foreach (var textObj in scoreTexts)
        {
            if (textObj != null) textObj.SetActive(false);
        }

        if (totalScoreObject != null) totalScoreObject.SetActive(false);
        if (continueButton != null) continueButton.gameObject.SetActive(false);
    }

    private void ApplyScoreData()
    {
        if (SaveManager.instance == null || SaveManager.instance.lastStageResult == null)
        {
            Debug.LogError("SaveManager가 없거나 전달된 점수 데이터가 없어요!");
            return;
        }

        var data = SaveManager.instance.lastStageResult;

        int kills = data.kills;
        int headshots = data.headshots;
        float playTime = data.playTime;
        float abilityTime = data.abilityTime;
        float accuracy = data.accuracy * 100f;
        int totalScore = data.totalScore;


        SetText(0, $"Kills\n{kills}");
        SetText(1, $"Headshots\n{headshots}");

        TimeSpan ts = TimeSpan.FromSeconds(playTime);
        string timeString = string.Format("{0:00}:{1:00}", ts.Minutes, ts.Seconds);
        SetText(2, $"Accuracy\n{accuracy:F1}%");

        SetText(3, $"Time\n{timeString}");
        SetText(4, $"Ability\n{abilityTime:F1}s");

        if (totalScoreObject != null)
        {
            TextMeshProUGUI totalTmp = totalScoreObject.GetComponent<TextMeshProUGUI>();
            if (totalTmp != null)
            {
                totalTmp.text = $"TOTAL SCORE\n{totalScore}";
            }
        }
    }

    private void SetText(int index, string content)
    {
        if (index >= scoreTexts.Count || scoreTexts[index] == null) return;

        TextMeshProUGUI tmp = scoreTexts[index].GetComponent<TextMeshProUGUI>();

        tmp.text = content;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            OnClickContinue();
        }
    }

    private IEnumerator SequenceScoreAnimation()
    {
        yield return new WaitForSeconds(0.5f);

        for (int i = 0; i < scoreTexts.Count; i++)
        {
            float targetY = startAngle + (i * stepAngle);

            yield return StartCoroutine(MoveNeedleToAngle(targetY));

            if (scoreTexts[i] != null)
            {
                scoreTexts[i].SetActive(true);
                // AudioSource.PlayOneShot(tickSound);
            }

            yield return _waitTick;
        }

        yield return StartCoroutine(MoveNeedleToAngle(finalAngle));

        if (totalScoreObject != null) totalScoreObject.SetActive(true);

        if (continueButton != null) continueButton.gameObject.SetActive(true);
    }

    private IEnumerator MoveNeedleToAngle(float targetY)
    {
        Quaternion targetRotation = Quaternion.Euler(0, targetY, 0);

        while (Quaternion.Angle(clockHand.localRotation, targetRotation) > 0.1f)
        {
            clockHand.localRotation = Quaternion.Slerp(clockHand.localRotation, targetRotation, Time.deltaTime * rotateSpeed);
            yield return null;
        }

        clockHand.localRotation = targetRotation;
    }

    public void OnClickContinue()
    {
        if (SaveManager.instance != null && SaveManager.instance.lastStageResult != null)
        {
            SceneName nextScene = SaveManager.instance.lastStageResult.nextScene;

            if (ScoreManager.instance != null) Destroy(ScoreManager.instance.gameObject);

            SceneController.Instance.LoadScene(nextScene, false);
        }
        else
        {
            SceneController.Instance.LoadScene(SceneName.Title);
        }
    }
}