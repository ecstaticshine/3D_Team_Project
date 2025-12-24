using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class Sound
{
    public string audioClipName;
    public AudioClip audioClip;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance = null;

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
        AutoSetting();
    }

    [Space(10f)]
    [Header("Audio Clip")]
    [SerializeField] private Sound[] BGM;
    [SerializeField] private Sound[] SFX;

    [Header("Audio Clip")]
    public AudioMixer mainMixer;

    [Space(50f)]
    [Header("Audio Soucre")]
    [Space(10f)]
    [SerializeField] private AudioSource BGMPlayer;
    [SerializeField] private AudioSource[] SFXPlayer;

    private float bgmPausedTime = 0f;
    [Header("음향 속도")]
    public float globalPitch = 1f;


    private void AutoSetting()
    {
        BGMPlayer = transform.GetChild(0).GetComponent<AudioSource>();
        SFXPlayer = transform.GetChild(1).GetComponents<AudioSource>();
    }

    public void PlaySlow(float slowFactor)
    {
        globalPitch = slowFactor;
        BGMPlayer.pitch = slowFactor;
            

        for (int i = 0; i < SFXPlayer.Length; i++)
        {
                SFXPlayer[i].pitch = slowFactor;
        }

        mainMixer.SetFloat("SFX", 0.5f);
    }
    public void PlayOriginal()
    {
        globalPitch = 1f;


        BGMPlayer.pitch = 1f;

        for (int i = 0; i < SFXPlayer.Length; i++)
        {
                SFXPlayer[i].pitch = 1f;
        }
        mainMixer.SetFloat("SFX", 1f);
    }

    public void PlaySFX(string bgmName)
    {
        foreach (Sound sound in SFX)
        {
            if (sound.audioClipName.Equals(bgmName))
            {
                for (int i = 0; i < SFXPlayer.Length; i++)
                {
                    if (!SFXPlayer[i].isPlaying)
                    {
                        SFXPlayer[i].pitch = globalPitch;
                        SFXPlayer[i].clip = sound.audioClip;
                        SFXPlayer[i].Play();
                        return;
                    }
                }
                // 모든 오디오소스가 플레이 중이다.
                Debug.Log("모든 AudioSource가 플레이 중입니다...");
                return;
            }
        }
        // 해당 이름을 가진 SFX가 없습니다.
        Debug.Log($"해당 SFX를 가진 친국는 없습니다..[{bgmName}]");
    }

    public void PlayBGM(string bgmName)
    {
        foreach (Sound sound in BGM)
        {
            if (sound.audioClipName.Equals(bgmName))
            {
                BGMPlayer.clip = sound.audioClip;
                BGMPlayer.Play();
                break;
            }
        }
    }

    public void PauseBGM()
    {
        if (BGMPlayer.isPlaying)
        {
            BGMPlayer.Pause();
            bgmPausedTime = BGMPlayer.time;
        }
    }

    public void StopBGM()
    {
        BGMPlayer.Stop();
    }


    public void ResumeBGM()
    {
        if (!BGMPlayer.isPlaying)
        {
            BGMPlayer.Play();
            BGMPlayer.time = bgmPausedTime;
        }
    }


    public void SetVolume(float level, string volumeName)
    {
        // 슬라이더가 0에 가까우면 그냥 음소거(-80dB) 처리
        if (level <= 0.001f)
        {
            mainMixer.SetFloat(volumeName, -80f);
        }
        else
        {
            mainMixer.SetFloat(volumeName, Mathf.Log10(level) * 20f);
        }
        PlayerPrefs.SetFloat(volumeName, level);
    }

}
