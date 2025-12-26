using System.Collections;
using System.Text;
using UnityEngine;
using TMPro;
public class ScoreCounter : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI scoreText;

    [Header("Settings")]
    [SerializeField] private float duration = 1.5f;
    [SerializeField] private AnimationCurve animationCurve;

    private StringBuilder _sb = new StringBuilder(16);

    public void ShowScore(int targetScore)
    {
        StopAllCoroutines();
        StartCoroutine(AnimateScore(targetScore));
    }

    private IEnumerator AnimateScore(int targetScore)
    {
        float timer = 0f;
        int currentScore = 0;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;

            float curveValue = animationCurve.Evaluate(progress);

            currentScore = Mathf.RoundToInt(Mathf.Lerp(0, targetScore, curveValue));

            UpdateText(currentScore);

            yield return null;
        }

        UpdateText(targetScore);
    }

    private void UpdateText(int value)
    {
        _sb.Clear();
        _sb.Append("TOTAL SCORE: ");
        _sb.Append(value);
        scoreText.text = _sb.ToString();
    }
}