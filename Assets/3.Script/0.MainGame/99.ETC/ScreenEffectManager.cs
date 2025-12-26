using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ScreenEffectManager : MonoBehaviour
{
    public static ScreenEffectManager instance;

    [Header("1. 기본 화면 효과 (피격/체력)")]
    [SerializeField] private Volume globalVolume;
    [SerializeField] private GameObject deadPanel;
    [SerializeField] private float maxIntensity = 0.5f;
    [SerializeField] private Color damageColor = Color.red;

    [Header("2. 되감기 효과")]
    [SerializeField] private GameObject rewindUI;
    [SerializeField] private Volume rewindVolume;

    [Header("3. 스킬 및 대쉬 이펙트")]
    [SerializeField] private Camera mainCam;
    [SerializeField] private ParticleSystem dashSpeedLines;
    [SerializeField] private Volume timeAbilityVolume;
    [SerializeField] private float dashFovAmount = 10f;
    [SerializeField] private float effectTransitionSpeed = 5f;

    [Header("3-1. 스킬 동적 연출 설정")]
    [SerializeField] private float minLensDistortion = 0.3f;
    [SerializeField] private float maxLensDistortion = 0.5f;
    [SerializeField] private float minVignette = 0.4f;
    [SerializeField] private float maxVignette = 0.55f;

    private Vignette damageVignette;

    private LensDistortion abilityLensDistortion;
    private Vignette abilityVignette;

    private bool isLowHealth = false;
    private bool isDead = false;
    private float blinkTimer = 0f;
    private float hitDurationTimer = 0f;
    private float targetIntensity = 0f;
    private float defaultFov;
    private Coroutine timeEffectCoroutine;

    private void Awake()
    {
        if (instance == null) { instance = this; DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);

        if (globalVolume != null && globalVolume.profile.TryGet(out Vignette v))
        {
            damageVignette = v;
        }

        if (timeAbilityVolume != null)
        {
            timeAbilityVolume.profile.TryGet(out abilityLensDistortion);
            timeAbilityVolume.profile.TryGet(out abilityVignette);
        }

        if (mainCam == null) mainCam = Camera.main;
        if (mainCam != null) defaultFov = mainCam.fieldOfView;
    }

    private void Update()
    {
        HandleDamageEffect();
    }

    #region 1. 기본 데미지 이펙트
    private void HandleDamageEffect()
    {
        if (damageVignette == null) return;

        if (isDead)
        {
            damageVignette.color.value = damageColor;
            damageVignette.intensity.value = Mathf.Lerp(damageVignette.intensity.value, 0.5f, Time.unscaledDeltaTime * 2f);
        }
        else if (hitDurationTimer > 0)
        {
            hitDurationTimer -= Time.unscaledDeltaTime;
            damageVignette.color.value = damageColor;
            damageVignette.intensity.value = Mathf.Lerp(damageVignette.intensity.value, 0.6f, Time.unscaledDeltaTime * 20f);
        }
        else if (isLowHealth)
        {
            damageVignette.color.value = damageColor;
            blinkTimer += Time.unscaledDeltaTime * 10f;
            float blink = targetIntensity + (Mathf.Sin(blinkTimer) * 0.1f);
            blink = Mathf.Clamp(blink, 0.2f, 0.8f);
            damageVignette.intensity.value = Mathf.Lerp(damageVignette.intensity.value, blink, Time.unscaledDeltaTime * 10f);
        }
        else
        {
            damageVignette.intensity.value = Mathf.Lerp(damageVignette.intensity.value, 0f, Time.unscaledDeltaTime * 5f);
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
        else targetIntensity = 0f;
    }
    public void PlayHitEffect() { hitDurationTimer = 0.3f; }
    public void SetDeathEffect() { isDead = true; isLowHealth = false; if (deadPanel != null) deadPanel.SetActive(true); }
    #endregion

    #region 2. 스킬 및 대쉬 이펙트

    public void UpdateAbilityIntensity(float currentGauge, float maxGauge)
    {
        if (timeAbilityVolume == null) return;

        float ratio = Mathf.Clamp01(currentGauge / maxGauge);

        float t = 1.0f - ratio;

        // 3. Lens Distortion 조절
        if (abilityLensDistortion != null)
        {
            abilityLensDistortion.intensity.value = Mathf.Lerp(minLensDistortion, maxLensDistortion, t);
        }

        // 4. Vignette 조절
        if (abilityVignette != null)
        {
            abilityVignette.intensity.value = Mathf.Lerp(minVignette, maxVignette, t);
        }
    }

    public void PlayDashEffect(float duration) { StartCoroutine(DashRoutine(duration)); }

    private IEnumerator DashRoutine(float duration)
    {
        if (mainCam == null) mainCam = Camera.main;
        if (dashSpeedLines != null) dashSpeedLines.Play();

        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (mainCam != null)
                mainCam.fieldOfView = Mathf.Lerp(mainCam.fieldOfView, defaultFov + dashFovAmount, Time.unscaledDeltaTime * 15f);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        if (dashSpeedLines != null) dashSpeedLines.Stop();
        while (mainCam != null && Mathf.Abs(mainCam.fieldOfView - defaultFov) > 0.1f)
        {
            mainCam.fieldOfView = Mathf.Lerp(mainCam.fieldOfView, defaultFov, Time.unscaledDeltaTime * 10f);
            yield return null;
        }
        if (mainCam != null) mainCam.fieldOfView = defaultFov;
    }

    public void ToggleTimeEffect(bool isActive)
    {
        if (timeEffectCoroutine != null) StopCoroutine(timeEffectCoroutine);
        timeEffectCoroutine = StartCoroutine(FadeVolumeRoutine(isActive ? 1f : 0f));
    }

    private IEnumerator FadeVolumeRoutine(float targetWeight)
    {
        if (timeAbilityVolume == null) yield break;
        while (Mathf.Abs(timeAbilityVolume.weight - targetWeight) > 0.01f)
        {
            timeAbilityVolume.weight = Mathf.Lerp(timeAbilityVolume.weight, targetWeight, Time.unscaledDeltaTime * effectTransitionSpeed);
            yield return null;
        }
        timeAbilityVolume.weight = targetWeight;
    }
    #endregion

    #region 3. 초기화
    public void ResetEffect()
    {
        isDead = false;
        isLowHealth = false;
        targetIntensity = 0f;
        hitDurationTimer = 0f;
        if (deadPanel != null) deadPanel.SetActive(false);
        if (damageVignette != null) damageVignette.intensity.value = 0f;

        if (timeAbilityVolume != null) timeAbilityVolume.weight = 0f;
        if (dashSpeedLines != null) dashSpeedLines.Stop();
        if (mainCam != null) mainCam.fieldOfView = defaultFov;

        SetRewindActive(false);
    }
    public void SetRewindActive(bool isActive)
    {
        if (rewindUI != null) rewindUI.SetActive(isActive);
        if (rewindVolume != null) rewindVolume.weight = isActive ? 1.0f : 0.0f;
    }
    #endregion
}