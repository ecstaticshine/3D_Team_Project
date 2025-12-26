using UnityEngine;
using UnityEngine.SceneManagement;

public class FinishPad : MonoBehaviour
{
    [Header("다음으로 이동할 씬 이름")]
    [SerializeField] private string sceneToLoad;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ScoreManager.instance.CalculateFinalScore();

            ScoreManager.instance.nextSceneName = sceneToLoad;

            SceneManager.LoadScene("ScoreScene");
        }
    }
}