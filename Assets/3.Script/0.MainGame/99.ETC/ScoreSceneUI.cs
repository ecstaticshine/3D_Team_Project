using UnityEngine;
using UnityEngine.UI; // [유니] Legacy Text를 쓰니까 이걸로!

public class ScoreSceneUI : MonoBehaviour
{
    [Header("텍스트 연결 (Legacy Text)")]
    [SerializeField] private Text killText;
    [SerializeField] private Text timeText;
    [SerializeField] private Text abilityText;
    [SerializeField] private Text accuracyText;
    [SerializeField] private Text totalScoreText;

    private void Start()
    {
        ScoreManager sm = ScoreManager.instance;

        if (sm == null)
        {
            return;
        }

        killText.text = $"Kills: {sm.killCount} ({sm.headshotCount} Headshots)";

        int minutes = Mathf.FloorToInt(sm.stageTime / 60F);
        int seconds = Mathf.FloorToInt(sm.stageTime % 60F);
        timeText.text = $"Time: {minutes:00}:{seconds:00}";

        abilityText.text = $"Ability: {sm.abilityUsageDuration:F1}s";

        accuracyText.text = $"Accuracy: {sm.finalAccuracy * 100f:F1}%";

        totalScoreText.text = $"TOTAL SCORE: {sm.finalScore:N0}";
    }

    public void OnClickContinue()
    {
        if (ScoreManager.instance != null) Destroy(ScoreManager.instance.gameObject);
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainTitle");
    }
}