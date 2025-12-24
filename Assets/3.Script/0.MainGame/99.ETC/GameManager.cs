using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;
    // [추가] 일시정지 상태가 변할 때마다 외칠 전광판 (true: 정지, false: 재개)
    public event Action<bool> OnPauseChanged;
    private bool isPlaying = true;

    public long totalPlayTimeMs;
    public int deathCount;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator Start()
    {
        //세이브매니져가 불려오고 시작
        yield return new WaitUntil(() => SaveManager.instance.IsLoaded);

        GetCurrentData(SaveManager.instance.saveData);
    }

    private void GetCurrentData(SaveData savedata)
    {
        totalPlayTimeMs = savedata.totalPlayTimeMs;
        deathCount = savedata.deathCount;
    }

    private void Update()
    {
        if (!isPlaying) return;

        // 슬로우 타임 영향을 받지 않게
        totalPlayTimeMs += (long)(Time.unscaledDeltaTime * 1000);
    }

    public void PauseGame()
    {
        isPlaying = false;
        Time.timeScale = 0f; // [추가]시간 정지
        OnPauseChanged?.Invoke(true); // [추가]"모두 멈춰!"라고 신호 보냄
    }

    public void ResumeGame()
    {
        isPlaying = true; 
        Time.timeScale = 1f; // [추가]시간 재개
        OnPauseChanged?.Invoke(false); // [추가]"다시 움직여!"라고 신호 보냄
    }
}
