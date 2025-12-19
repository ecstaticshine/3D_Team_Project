using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ScreenEffectManager : MonoBehaviour
{
    public static ScreenEffectManager instance;

    [Header("1. 기본 화면 효과")]
    [SerializeField] private Volume globalVolume;
    [SerializeField] private GameObject deadPanel;
    [SerializeField] private float maxIntensity = 0.5f;
    [SerializeField] private Color damageColor = Color.red;

    [Header("2. 되감기 효과")]
    [SerializeField] private GameObject rewindUI;
    [SerializeField] private Volume rewindVolume;

    private Vignette vignette;
    private bool isLowHealth = false;
    private bool isDead = false;
    private float blinkTimer = 0f;
    private float hitDurationTimer = 0f;

    private float targetIntensity = 0f;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        if (globalVolume != null && globalVolume.profile.TryGet(out Vignette v))
        {
            vignette = v;
        }
    }

    private void Update()
    {
        HandleDamageEffect();
    }

    private void HandleDamageEffect()
    {
        if (vignette == null) return;

        if (isDead)
        {
            vignette.color.value = damageColor;
            vignette.intensity.value = Mathf.Lerp(vignette.intensity.value, 0.5f, Time.unscaledDeltaTime * 2f);
        }
        else if (hitDurationTimer > 0)
        {
            hitDurationTimer -= Time.unscaledDeltaTime;
            vignette.color.value = damageColor;
            vignette.intensity.value = Mathf.Lerp(vignette.intensity.value, 0.6f, Time.unscaledDeltaTime * 20f);
        }
        else if (isLowHealth)
        {
            vignette.color.value = damageColor;
            blinkTimer += Time.unscaledDeltaTime * 10f;

            float blink = targetIntensity + (Mathf.Sin(blinkTimer) * 0.1f);

            blink = Mathf.Clamp(blink, 0.2f, 0.8f);

            vignette.intensity.value = Mathf.Lerp(vignette.intensity.value, blink, Time.unscaledDeltaTime * 10f);
        }
        else
        {
            vignette.intensity.value = Mathf.Lerp(vignette.intensity.value, 0f, Time.unscaledDeltaTime * 5f);
        }
    }

    public void CheckHealthStatus(float currentHp, float maxHp)
    {
        if (isDead) return;
        isLowHealth = (currentHp / maxHp <= 0.3f && currentHp > 0);
    }

    public void UpdateEffect(float currentGauge, float maxGauge)
    {
        float ratio = currentGauge / maxGauge;

        if (ratio <= 0.3f)
        {
            float dangerFactor = 1.0f - (ratio / 0.3f);
            targetIntensity = dangerFactor * maxIntensity;
        }
        else
        {
            targetIntensity = 0f;
        }
    }

    public void PlayHitEffect()
    {
        hitDurationTimer = 0.3f;
    }

    public void SetDeathEffect()
    {
        isDead = true;
        isLowHealth = false;
        if (deadPanel != null) deadPanel.SetActive(true);
    }

    public void ResetEffect()
    {
        isDead = false;
        isLowHealth = false;
        targetIntensity = 0f;
        hitDurationTimer = 0f;
        if (deadPanel != null) deadPanel.SetActive(false);
        if (vignette != null) vignette.intensity.value = 0f;
        SetRewindActive(false);
    }

    public void SetRewindActive(bool isActive)
    {
        if (rewindUI != null) rewindUI.SetActive(isActive);
        if (rewindVolume != null) rewindVolume.weight = isActive ? 1.0f : 0.0f;
    }
}