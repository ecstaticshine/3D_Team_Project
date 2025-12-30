using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TimeRewindManager : MonoBehaviour
{
    public static TimeRewindManager Instance;

    [Header("설정")]
    [SerializeField] private int baseRewindSpeed = 3;
    [SerializeField] private float maxRewindDuration = 5.0f;

    [Header("효과 연결")]
    [SerializeField] private ScreenEffectManager effectManager;

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

        int maxRecordCount = 0;
        foreach (var obj in rewindables)
        {
            if (obj != null)
            {
                obj.StartRewind();
                if (obj.RecordCount > maxRecordCount) maxRecordCount = obj.RecordCount;
            }
        }

        float targetFrames = maxRewindDuration * 60f;
        int calculatedSpeed = Mathf.CeilToInt(maxRecordCount / targetFrames);

        int finalSpeed = Mathf.Max(baseRewindSpeed, calculatedSpeed);

        Debug.Log($"데이터: {maxRecordCount}개 / 목표시간: {maxRewindDuration}초 / 결정된 배속: {finalSpeed}배");

        bool hasData = true;

        while (hasData)
        {
            hasData = false;

            for (int speed = 0; speed < finalSpeed; speed++)
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
    }
}