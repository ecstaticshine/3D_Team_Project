using UnityEngine;
using System.Collections;
public class RainController : MonoBehaviour
{
    [SerializeField] private ParticleSystem rainParticle;
    [SerializeField] private AudioSource rainAudio;

    void OnEnable()
    {
        StartRain();
    }

    void OnDisable()
    {
        StopRain();
    }

    public void StartRain()
    {
        // 파티클 시스템 재생
        if (rainParticle != null && !rainParticle.isPlaying)
            rainParticle.Play();

        // 오디오 소스 재생 및 볼륨 설정
        if (rainAudio != null && !rainAudio.isPlaying)
        {
            rainAudio.volume = 0.5f;
            rainAudio.Play();
        }
    }

    public void StopRain()
    {
        // 파티클 즉시 정지
        if (rainParticle != null)
            rainParticle.Stop();

        // 오디오 즉시 정지 
        if (rainAudio != null)
            rainAudio.Stop();
    }
}
