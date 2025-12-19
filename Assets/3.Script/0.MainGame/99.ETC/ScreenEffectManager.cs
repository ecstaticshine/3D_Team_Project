using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ScreenEffectManager : MonoBehaviour
{
    public static ScreenEffectManager instance;

    [Header("연결")]
    [SerializeField] private Volume globalVolume;
    [SerializeField] private GameObject deadPanel;

    [Header("설정")]
    [SerializeField] private float effectSpeed = 5f;
    [SerializeField] private float maxIntensity = 0.5f;
    [SerializeField] private Color damageColor = Color.red;

    // 내부 변수
    private Vignette vignette;
    private float targetIntensity = 0f;
    private bool isLowHealth = false;
    private bool isDead = false;
    private float blinkTimer = 0f;
    private float hitDurationTimer = 0f;

    private void Awake()
    {
        if (instance == null) instance = this;

        if (globalVolume.profile.TryGet(out Vignette v))
        {
            vignette = v;
        }
    }

    private void Update()
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
            float blink = 0.4f + Mathf.Sin(blinkTimer) * 0.1f;
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

        if (currentHp / maxHp <= 0.3f && currentHp > 0)
        {
            isLowHealth = true;
        }
        else
        {
            isLowHealth = false;
        }
    }

    public void SetDeathEffect()
    {
        isDead = true;
        isLowHealth = false;

        if (deadPanel != null) deadPanel.SetActive(true);
    }

    public void PlayHitEffect()
    {
        hitDurationTimer = 0.3f;
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
}
