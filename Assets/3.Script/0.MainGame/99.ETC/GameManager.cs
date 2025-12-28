using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public enum GameState
    {
        Playing,
        Paused
    }

    public static GameManager instance = null;
    // [추가] 일시정지 상태가 변할 때마다 외칠 전광판 (true: 정지, false: 재개)
    public event Action<bool> OnPauseChanged;
    //private bool isPlaying = true;
    
    [Header("씬 상태")]
    public GameState CurrentState { get; private set; }

    [Header("패널 연결")]
    [SerializeField] private GameObject settingsPanel; // 방금 만든 Panel_Settings 연결

    public long totalPlayTimeMs;
    public int deathCount;

    [Header("입력 연결 (New Input System)")]
    // 인스펙터에서 만들어둔 'Pause'나 'Menu' 액션을 여기에 드래그해서 넣으세요
    [SerializeField] private InputActionReference menuAction;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            CurrentState = GameState.Playing;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private bool isSettingsOpen = false;

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

        if (menuAction != null && menuAction.action.WasPressedThisFrame())
        {
            if (CurrentState == GameState.Playing)
            {
                PauseGame();
            }
            else if (CurrentState == GameState.Paused)
            {
                ResumeGame();
            }
        }
        
        if (CurrentState == GameState.Playing)
        {
            // 슬로우 타임 영향을 받지 않게
            totalPlayTimeMs += (long)(Time.deltaTime * 1000);
        }
    }

    public void PauseGame()
    {
        CurrentState = GameState.Paused;
        Time.timeScale = 0f;

        settingsPanel.SetActive(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        OnPauseChanged?.Invoke(true); // [추가]"모두 멈춰!"라고 신호 보냄
    }

    public void ResumeGame()
    {
        CurrentState = GameState.Playing;
        Time.timeScale = 1f;

        settingsPanel.SetActive(false);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        OnPauseChanged?.Invoke(false); // [추가]"다시 움직여!"라고 신호 보냄
    }

}
