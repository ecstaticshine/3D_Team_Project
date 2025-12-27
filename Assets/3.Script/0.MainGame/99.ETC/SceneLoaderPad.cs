using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoaderPad : MonoBehaviour
{
    [Header("이동할 씬 이름")]
    [SerializeField] private SceneName targetScene = SceneName.Stage1;
    [SerializeField] private bool useScoreScene = true; // 스코어 씬 이동할지

    [Header("저장 여부")]
    [SerializeField] private bool shouldSave = true;

    private bool isActivated = false; // 중복 실행 방지 변수

    private void OnTriggerEnter(Collider other)
    {
        if (isActivated || !other.CompareTag("Player")) return;

        isActivated = true;
         MoveToScene();
    }

    private void MoveToScene()
    {

        if (shouldSave && SaveManager.instance != null)
        {
            SaveManager.instance.saveData.sceneToLoad = targetScene.ToString();

            SaveManager.instance.SaveGame();

            Debug.Log($"[유니] {targetScene.ToString()} 진입 전 저장 완료!");
        }

        if (useScoreScene && ScoreManager.instance != null)
        {
            ScoreManager.instance.CalculateFinalScore(targetScene);
            SceneController.Instance.LoadScene(SceneName.Score);
        }
        else
        {
            SceneController.Instance.LoadScene(targetScene);
        }
    }
}