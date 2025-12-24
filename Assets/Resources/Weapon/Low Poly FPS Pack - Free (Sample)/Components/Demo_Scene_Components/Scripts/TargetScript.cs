using UnityEngine;
using System.Collections;

public class TargetScript : MonoBehaviour
{

    [Header("Customizable Options")]
    public float minTime;
    public float maxTime;

    [Header("Audio")]
    public AudioClip upSound;
    public AudioClip downSound;
    public AudioSource audioSource;

    public bool isHit = false;

    private Animation targetAnim;

    private void Start()
    {
        if (!TryGetComponent<Animation>(out targetAnim))
        {
        }

        if (audioSource == null && !TryGetComponent<AudioSource>(out audioSource))
        {
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bullet") && isHit == false)
        {
            GetHit();
        }
    }

    public void GetHit()
    {
        if (isHit) return;

        isHit = true;

        if (audioSource != null && downSound != null)
        {
            audioSource.clip = downSound;
            audioSource.Play();
        }

        if (targetAnim != null)
        {
            targetAnim.Stop();
            targetAnim.Play("target_down");
        }

        StartCoroutine(DelayTimer());
    }

    private IEnumerator DelayTimer()
    {
        float randomWaitTime = Random.Range(minTime, maxTime);
        yield return new WaitForSeconds(randomWaitTime);

        if (targetAnim != null)
        {
            targetAnim.Stop();
            targetAnim.Play("target_up");
        }

        if (audioSource != null && upSound != null)
        {
            audioSource.clip = upSound;
            audioSource.Play();
        }

        isHit = false;
    }
}