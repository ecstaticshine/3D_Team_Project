using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;

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
    }

    public void ResumeGame()
    {
        isPlaying = true;
    }
}
