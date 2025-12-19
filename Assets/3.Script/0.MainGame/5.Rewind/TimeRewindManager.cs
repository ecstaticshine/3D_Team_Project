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
            Debug.Log($"[유니 매니저] {obj.name} 등록 완료! (현재 관리 중: {rewindables.Count}개)");
        }
    }

    public void UnregisterObject(RewindableObject obj) => rewindables.Remove(obj);

    private void FixedUpdate()
    {
        if (!isRewinding)
        {
            // [유니 디버그] 등록된 애들이 없으면 기록도 안 합니다.
            if (rewindables.Count == 0 && Time.frameCount % 120 == 0)
            {
                Debug.LogWarning("[유니 매니저] 현재 관리하는 오브젝트가 0개입니다! 등록이 안 되고 있어요.");
            }

            for (int i = 0; i < rewindables.Count; i++)
            {
                if (rewindables[i] != null) rewindables[i].Record();
            }
        }
    }

    // ... (StartFullRewind 및 나머지 코드는 기존과 동일) ...
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

        // [유니] 오빠가 수정한 부분: ScreenEffectManager 직접 호출 방식이라면 여기를 주석 처리하거나 맞춰주세요
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