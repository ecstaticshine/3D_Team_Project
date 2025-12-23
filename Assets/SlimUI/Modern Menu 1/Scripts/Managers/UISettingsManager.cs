using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;

namespace SlimUI.ModernMenu
{
    public class UISettingsManager : MonoBehaviour
    {
        [Header("연결")]
        [SerializeField] private AudioMixer mainMixer;
        [SerializeField] private Slider masterSlider;
        [SerializeField] private Slider bgmSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private Slider mouseSlider;

        private void Start()
        {
            // 게임 시작 시, 저장된 볼륨이 있으면 불러오고 없으면 기본값(0.75) 설정
            // (PlayerPrefs 기능은 나중에 추가해도 되니 일단 기본 세팅만 합니다)

            float savedValue = PlayerPrefs.GetFloat("MouseSensitivity", 50.0f);

            if (mouseSlider != null)
            {
                mouseSlider.value = savedValue;
            }

            masterSlider.value = 1f;
            bgmSlider.value = 1f;
            sfxSlider.value = 1f;

            // 믹서에 적용
            SetMasterVolume(masterSlider.value);
            SetBGMVolume(bgmSlider.value);
            SetSFXVolume(sfxSlider.value);
        }

        // 슬라이더 값이 바뀔 때 호출될 함수들
        public void SetMasterVolume(float level)
        {
            // 슬라이더가 0에 가까우면 그냥 음소거(-80dB) 처리
            if (level <= 0.001f)
            {
                mainMixer.SetFloat("MasterVol", -80f);
            }
            else
            {
                // 그 외에는 로그 곡선 적용
                mainMixer.SetFloat("MasterVol", Mathf.Log10(level) * 20);
            }
        }

        public void SetBGMVolume(float level)
        {
            if (level <= 0.001f)
            {
                mainMixer.SetFloat("BGMVol", -80f);
            }
            else
            {
                mainMixer.SetFloat("BGMVol", Mathf.Log10(level) * 20);
            }
        }

        public void SetSFXVolume(float level)
        {
            if (level <= 0.001f)
            {
                mainMixer.SetFloat("SFXVol", -80f);
            }
            else
            {
                mainMixer.SetFloat("SFXVol", Mathf.Log10(level) * 20);
            }
        }

        public void SetMouseSensitivity(float amount)
        {
            Debug.Log($"[저장됨] 마우스 감도: {amount}");

            PlayerPrefs.SetFloat("MouseSensitivity", amount);
            PlayerPrefs.Save();
        }
    }
}