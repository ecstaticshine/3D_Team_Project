using UnityEngine;
using UnityEngine.SceneManagement;

public class FinishPad : MonoBehaviour
{
    [Header("다음으로 이동할 씬 이름")]
    [SerializeField] private SceneName sceneToLoad = SceneName.Stage1;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ScoreManager.instance.CalculateFinalScore(sceneToLoad);

            SceneController.Instance.LoadScene(SceneName.Score);
        }
    }
}