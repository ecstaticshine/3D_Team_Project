using UnityEngine;
using UnityEngine.SceneManagement;

public class FinishPad : MonoBehaviour
{
    [Header("다음으로 이동할 씬 이름")]
    [SerializeField] private SceneName sceneToLoad = SceneName.Stage1;

    private bool isTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isTriggered || !other.CompareTag("Player")) return;

        isTriggered = true;

        if (GameManager.instance != null)
        {
            GameManager.instance.canPause = false;

            if (GameManager.instance.CurrentState == GameManager.GameState.Paused)
            {
                GameManager.instance.ResumeGame();
            }
        }

        if (ScoreManager.instance != null)
        {
            ScoreManager.instance.CalculateFinalScore(sceneToLoad);
            SceneController.Instance.LoadScene(SceneName.Score);
        }
    }
}