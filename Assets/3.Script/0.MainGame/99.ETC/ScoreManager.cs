using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance = null;

    [Header("씬 이동 정보")]
    public string nextSceneName;

    [Header("점수 기록")]
    public int killCount = 0;
    public int headshotCount = 0;
    public float stageTime = 0f;
    public float abilityUsageDuration = 0f;
    public int shotsFired = 0;
    public int shotsHit = 0;

    public int finalScore = 0;
    public float finalAccuracy = 0f;

    private bool isStageClear = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (!isStageClear) stageTime += Time.unscaledDeltaTime;
    }

    public void AddKill(bool isHeadshot) { killCount++; if (isHeadshot) headshotCount++; }
    public void AddShotFired() => shotsFired++;
    public void AddShotHit() => shotsHit++;
    public void AddAbilityUsage(float duration) { abilityUsageDuration += duration; }

    public void CalculateFinalScore()
    {
        if (isStageClear) return;
        isStageClear = true;

        int score = 0;

        score += killCount * 100;
        score += headshotCount * 50;

        finalAccuracy = (shotsFired > 0) ? ((float)shotsHit / shotsFired) : 0f;
        score += Mathf.RoundToInt(finalAccuracy * 1000);

        score -= Mathf.FloorToInt(stageTime * 10);
        score -= Mathf.FloorToInt(abilityUsageDuration * 5);

        if (score < 0) score = 0;

        finalScore = score;
    }
}