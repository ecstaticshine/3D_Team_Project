using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TimeRewindManager : MonoBehaviour
{
    public static TimeRewindManager Instance;

    [Header("효과 연결")]
    [SerializeField] private ScreenEffectManager effectManager;
    [SerializeField] private int rewindSpeed = 3;

    private List<RewindableObject> rewindables = new List<RewindableObject>();

    private bool isRewinding = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public void RegisterObject(RewindableObject obj)
    {
        if (!rewindables.Contains(obj)) rewindables.Add(obj);
    }
    public void UnregisterObject(RewindableObject obj) => rewindables.Remove(obj);

    private void FixedUpdate()
    {
        if (!isRewinding)
        {
            for (int i = 0; i < rewindables.Count; i++)
            {
                if (rewindables[i] != null) rewindables[i].Record();
            }
        }
    }

    public void StartFullRewind()
    {
        if (isRewinding) return;

        Time.timeScale = 1.0f;

        StartCoroutine(RewindAllCoroutine());
    }

    private IEnumerator RewindAllCoroutine()
    {
        isRewinding = true;

        if (ScreenEffectManager.instance != null) ScreenEffectManager.instance.SetRewindActive(true);

        foreach (var obj in rewindables) { if (obj != null) obj.StartRewind(); }

        bool hasData = true;

        while (hasData)
        {
            hasData = false;

            for (int speed = 0; speed < rewindSpeed; speed++)
            {
                for (int i = 0; i < rewindables.Count; i++)
                {
                    if (rewindables[i] != null && rewindables[i].RewindStep()) hasData = true;
                }
            }
            yield return null;
        }

        if (ScreenEffectManager.instance != null)
        {
            yield return StartCoroutine(ScreenEffectManager.instance.Fade(1f));

            ScreenEffectManager.instance.SetRewindActive(false);
        }

        string currentSceneNameStr = SceneManager.GetActiveScene().name;

        SceneName targetScene = SceneName.Title;

        foreach (SceneName scene in System.Enum.GetValues(typeof(SceneName)))
        {
            if (SceneNameMap.Get(scene) == currentSceneNameStr)
            {
                targetScene = scene;
                break;
            }
        }

        SceneController.Instance.LoadScene(targetScene, false);

        isRewinding = false;
    }
}