using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneBGM : MonoBehaviour
{
    [SerializeField] private string bgmName;

    private void Start()
    {
        if (AudioManager.instance.BGMPlayer.isPlaying)
        {
            AudioManager.instance.StopBGM();
        }
        AudioManager.instance.PlayBGM(bgmName);
    }
}
