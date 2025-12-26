using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoaderPad : MonoBehaviour
{
    [Header("이동할 씬 이름")]
    [SerializeField] private string targetSceneName = "Stage1";
    [SerializeField] private bool useScoreScene = true; // 스코어 씬 이동할지

    [Header("저장 여부")]
    [SerializeField] private bool shouldSave = true;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            MoveToScene();
        }
    }

    private void MoveToScene()
    {

        if (shouldSave && SaveManager.instance != null)
        {
            SaveManager.instance.saveData.sceneToLoad = targetSceneName;

            SaveManager.instance.SaveGame();

            Debug.Log($"[유니] {targetSceneName} 진입 전 저장 완료!");
        }

        if (useScoreScene && ScoreManager.instance != null)
        {
            ScoreManager.instance.CalculateFinalScore(targetSceneName);
            SceneManager.LoadScene("ScoreScene");
        }
        else
        {
            SceneManager.LoadScene(targetSceneName);
        }
    }
}