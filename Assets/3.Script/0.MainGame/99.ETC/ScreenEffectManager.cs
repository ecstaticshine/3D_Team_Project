using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ScreenEffectManager : MonoBehaviour
{
    public static ScreenEffectManager instance;

    [Header("1. 기본 화면 효과 (피격/체력)")]
    [SerializeField] private Volume globalVolume; // 피격용 붉은 비네팅이 들어있는 볼륨
    [SerializeField] private GameObject deadPanel;
    [SerializeField] private float maxIntensity = 0.5f;
    [SerializeField] private Color damageColor = Color.red;

    [Header("2. 되감기 효과")]
    [SerializeField] private GameObject rewindUI;
    [SerializeField] private Volume rewindVolume;

    [Header("3. 스킬 및 대쉬 이펙트 (NEW!)")]
    [SerializeField] private Camera mainCam; // 메인 카메라 (FOV 조절용)
    [SerializeField] private ParticleSystem dashSpeedLines; // 대쉬할 때 나오는 속도선 파티클
    [SerializeField] private Volume timeAbilityVolume; // 시간 능력 쓸 때 켜질 볼륨 (채도 감소, 왜곡 등)
    [SerializeField] private float dashFovAmount = 10f; // 대쉬 시 늘어날 시야각
    [SerializeField] private float effectTransitionSpeed = 5f; // 시간 능력 켜지는 속도

    // 내부 변수들
    private Vignette vignette;
    private bool isLowHealth = false;
    private bool isDead = false;
    private float blinkTimer = 0f;
    private float hitDurationTimer = 0f;
    private float targetIntensity = 0f;

    // 대쉬/스킬용 변수
    private float defaultFov;
    private Coroutine timeEffectCoroutine;

    private void Awake()
    {
        if (instance == null) { instance = this; DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);

        // Vignette 컴포넌트 가져오기 (피격 효과용)
        if (globalVolume != null && globalVolume.profile.TryGet(out Vignette v))
        {
            vignette = v;
        }

        // 카메라 기본 FOV 저장 (씬 이동 시 카메라가 바뀔 수 있으니 체크)
        if (mainCam == null) mainCam = Camera.main;
        if (mainCam != null) defaultFov = mainCam.fieldOfView;
    }

    private void Update()
    {
        HandleDamageEffect();
    }

    #region 1. 기본 데미지 이펙트 (기존 코드 유지)

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

    #endregion

    #region 2. 스킬 및 대쉬 이펙트 (NEW!)

    // [대쉬] Player 스크립트에서 대쉬 시작할 때 호출해줘! (duration: 대쉬 지속시간)
    public void PlayDashEffect(float duration)
    {
        StartCoroutine(DashRoutine(duration));
    }

    private IEnumerator DashRoutine(float duration)
    {
        // 카메라가 혹시 유실되었으면 다시 찾기
        if (mainCam == null) mainCam = Camera.main;

        // 1. 속도선 파티클 재생
        if (dashSpeedLines != null) dashSpeedLines.Play();

        // 2. FOV 줌 아웃 (빨려 들어가는 느낌)
        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (mainCam != null)
            {
                mainCam.fieldOfView = Mathf.Lerp(mainCam.fieldOfView, defaultFov + dashFovAmount, Time.unscaledDeltaTime * 15f);
            }
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        // 3. 파티클 정지
        if (dashSpeedLines != null) dashSpeedLines.Stop();

        // 4. FOV 원상복구
        while (mainCam != null && Mathf.Abs(mainCam.fieldOfView - defaultFov) > 0.1f)
        {
            mainCam.fieldOfView = Mathf.Lerp(mainCam.fieldOfView, defaultFov, Time.unscaledDeltaTime * 10f);
            yield return null;
        }
        if (mainCam != null) mainCam.fieldOfView = defaultFov;
    }

    // [시간 능력] 능력을 켜거나(true) 끌 때(false) 호출
    public void ToggleTimeEffect(bool isActive)
    {
        if (timeEffectCoroutine != null) StopCoroutine(timeEffectCoroutine);
        timeEffectCoroutine = StartCoroutine(FadeVolumeRoutine(isActive ? 1f : 0f));
    }

    private IEnumerator FadeVolumeRoutine(float targetWeight)
    {
        if (timeAbilityVolume == null) yield break;

        // Weight가 목표치에 도달할 때까지 부드럽게 전환
        while (Mathf.Abs(timeAbilityVolume.weight - targetWeight) > 0.01f)
        {
            timeAbilityVolume.weight = Mathf.Lerp(timeAbilityVolume.weight, targetWeight, Time.unscaledDeltaTime * effectTransitionSpeed);
            yield return null;
        }
        timeAbilityVolume.weight = targetWeight;
    }

    #endregion

    #region 3. 초기화 및 되감기 (기존 코드 유지)

    public void ResetEffect()
    {
        isDead = false;
        isLowHealth = false;
        targetIntensity = 0f;
        hitDurationTimer = 0f;
        if (deadPanel != null) deadPanel.SetActive(false);
        if (vignette != null) vignette.intensity.value = 0f;

        // [추가] 스킬 이펙트도 초기화
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