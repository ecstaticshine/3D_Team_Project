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

        [SerializeField] private Slider masterSlider;
        [SerializeField] private Slider bgmSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private Slider mouseSlider;

        private void Start()
        {

            float master = PlayerPrefs.GetFloat("MasterVol", 0.75f);
            float bgm = PlayerPrefs.GetFloat("BGMVol", 0.75f);
            float sfx = PlayerPrefs.GetFloat("SFXVol", 0.75f);

            //UI 슬라이더 바꾸기
            masterSlider.value = master;
            bgmSlider.value = bgm;
            sfxSlider.value = sfx;


            ApplyAll();

            masterSlider.onValueChanged.AddListener(OnMasterChanged);
            bgmSlider.onValueChanged.AddListener(OnBGMChanged);
            sfxSlider.onValueChanged.AddListener(OnSFXChanged);

            float savedValue = PlayerPrefs.GetFloat("MouseSensitivity", 50.0f);

            if (mouseSlider != null)
            {
                mouseSlider.value = savedValue;
            }
        }

        private void ApplyAll()
        {
            AudioManager.instance.SetVolume(masterSlider.value, "MasterVol");
            AudioManager.instance.SetVolume(bgmSlider.value, "BGMVol");
            AudioManager.instance.SetVolume(sfxSlider.value, "SFXVol");
        }

        private void OnMasterChanged(float value)
        {
            AudioManager.instance.SetVolume(value, "MasterVol");
            PlayerPrefs.SetFloat("MasterVol",value);
        }

        private void OnBGMChanged(float value)
        {
            AudioManager.instance.SetVolume(value, "BGMVol");
            PlayerPrefs.SetFloat("BGMVol", value);
        }

        private void OnSFXChanged(float value)
        {
            AudioManager.instance.SetVolume(value, "SFXVol");
            PlayerPrefs.SetFloat("SFXVol", value);
        }
    }
}