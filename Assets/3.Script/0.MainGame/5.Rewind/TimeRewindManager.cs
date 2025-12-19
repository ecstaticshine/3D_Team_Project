using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeRewindManager : MonoBehaviour
{
    public static TimeRewindManager Instance;

    private List<RewindableObject> rewindables = new List<RewindableObject>();
    private bool isRewinding = false;

    [Header("효과 연결")]
    [SerializeField] private ScreenEffectManager effectManager;

    private void Awake()
    {
        // [유니] 싱글톤 보장
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // 필요하다면 씬 전환 시 유지
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RegisterObject(RewindableObject obj)
    {
        if (!rewindables.Contains(obj))
        {
            rewindables.Add(obj);
        }
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
        Debug.Log($"[유니 매니저] 되감기 시작! (대상: {rewindables.Count}명)"); // 로그 추가
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
            for (int i = 0; i < rewindables.Count; i++)
            {
                if (rewindables[i] != null && rewindables[i].RewindStep()) hasData = true;
            }
            yield return null;
        }

        foreach (var obj in rewindables) { if (obj != null) obj.StopRewind(); }

        if (ScreenEffectManager.instance != null) ScreenEffectManager.instance.SetRewindActive(false);

        isRewinding = false;
        Debug.Log("되감기 완료!");
    }
}