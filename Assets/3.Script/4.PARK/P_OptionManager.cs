using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.InputSystem;

public class P_OptionManager : MonoBehaviour
{
    
    [Header("연결")]
    [SerializeField] private AudioMixer mainMixer;
    [SerializeField] private GameObject settingsPanel; // 껐다 켤 설정창 패널
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider mouseSlider; // [추가] 감도 슬라이더 연결

    private bool isSettingsOpen = false;

    private void Start()
    {
        // 초기 설정: 패널은 닫아두고 볼륨 기본값 세팅
        if (settingsPanel != null) settingsPanel.SetActive(false);

        masterSlider.value = PlayerPrefs.GetFloat("MasterVolValue", 0.75f);
        bgmSlider.value = PlayerPrefs.GetFloat("BGMVolValue", 0.75f);
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVolValue", 0.75f);
        // 마우스 감도 로드 (기존 UISettingsManager 로직 활용)
        if (mouseSlider != null)
        {
            mouseSlider.value = PlayerPrefs.GetFloat("MouseSensitivity", 25.0f);
        }
        ApplyAllVolume();
        ApplyMouseSensitivity(mouseSlider.value); // 시작 시 감도 적용
    }

    private void Update()
    {
        // ESC 키를 누르면 설정창 토글
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleSettings();
        }
    }

    public void ToggleSettings()
    {
        isSettingsOpen = !isSettingsOpen;
        settingsPanel.SetActive(isSettingsOpen);

        if (isSettingsOpen)
        {
            GameManager.instance.PauseGame(); // 매니저에게 정지 요청
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            GameManager.instance.ResumeGame(); // 매니저에게 재개 요청
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
    // 마우스 감도 조절 함수 (Dynamic float으로 슬라이더에 연결)
    public void SetMouseSensitivity(float amount)
    {
        PlayerPrefs.SetFloat("MouseSensitivity", amount);
        PlayerPrefs.Save(); //
        ApplyMouseSensitivity(amount);
    }

    private void ApplyMouseSensitivity(float amount)
    {
        // 씬에 있는 Player를 찾아 실시간으로 감도를 바꿔줍니다.
        var player = GameObject.FindWithTag("Player")?.GetComponent<Player>();
        if (player != null)
        {
            // Player.cs의 mouseSensitivity 변수에 접근 (public이어야 함)
            // player.mouseSensitivity = amount; 
        }
    }
    // 슬라이더 호출용 함수들 (Dynamic float으로 연결)
    public void SetMasterVolume(float level)
    {
        SetMixer("MasterVol", level);
        PlayerPrefs.SetFloat("MasterVolValue", level); // 값 저장
    }

    public void SetBGMVolume(float level)
    {
        SetMixer("BGMVol", level);
        PlayerPrefs.SetFloat("BGMVolValue", level); // 값 저장
    }

    public void SetSFXVolume(float level)
    {
        SetMixer("SFXVol", level);
        PlayerPrefs.SetFloat("SFXVolValue", level); // 값 저장
    }

    private void SetMixer(string parameterName, float level)
    {
        if (level <= 0.001f) mainMixer.SetFloat(parameterName, -80f);
        else mainMixer.SetFloat(parameterName, Mathf.Log10(level) * 20);
    }

    private void ApplyAllVolume()
    {
        SetMasterVolume(masterSlider.value);
        SetBGMVolume(bgmSlider.value);
        SetSFXVolume(sfxSlider.value);
    }


}

