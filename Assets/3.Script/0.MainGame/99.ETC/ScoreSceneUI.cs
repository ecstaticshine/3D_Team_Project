using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ScoreSceneUI : MonoBehaviour
{
    [Header("텍스트 연결")]
    [SerializeField] private Text killText;
    [SerializeField] private Text timeText;
    [SerializeField] private Text abilityText;
    [SerializeField] private Text accuracyText;
    [SerializeField] private Text totalScoreText;

    private void Start()
    {
        if (ScoreManager.instance == null)
        {
            return;
        }

        killText.text = $"Kills: {ScoreManager.instance.killCount} ({ScoreManager.instance.headshotCount} Headshots)";

        int minutes = Mathf.FloorToInt(ScoreManager.instance.stageTime / 60F);
        int seconds = Mathf.FloorToInt(ScoreManager.instance.stageTime % 60F);
        timeText.text = $"Time: {minutes:00}:{seconds:00}";

        abilityText.text = $"Ability: {ScoreManager.instance.abilityUsageDuration:F1}s";

        accuracyText.text = $"Accuracy: {ScoreManager.instance.finalAccuracy * 100f:F1}%";

        totalScoreText.text = $"TOTAL SCORE: {ScoreManager.instance.finalScore:N0}";
    }

    public void OnClickContinue()
    {
        if (ScoreManager.instance != null) Destroy(ScoreManager.instance.gameObject);
        //SceneManager.LoadScene("MainTitle");
    }
}