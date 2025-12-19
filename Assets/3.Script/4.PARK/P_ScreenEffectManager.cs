using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering; // 필수
using UnityEngine.Rendering.Universal; // URP 필수

public class P_ScreenEffectManager : MonoBehaviour
{
    public static P_ScreenEffectManager instance;

    [Header("연결")]
    [SerializeField] private Volume globalVolume; // Global Volume을 여기에 넣으세요
    [SerializeField] private GameObject deadPanel; //  패널 연결

    [Header("설정")]
    [SerializeField] private float effectSpeed = 5f; // 효과가 켜지고 꺼지는 속도
    [SerializeField] private float maxIntensity = 0.5f; // 최대 얼마나 어둡게 할지 (0.0 ~ 1.0)
    [SerializeField] private Color damageColor = Color.red; // 피격 시 비네트 색

    // 내부 변수
    private Vignette vignette;
    private float targetIntensity = 0f;
    private bool isLowHealth = false;
    private bool isDead = false;
    private float blinkTimer = 0f;

    // [추가] 피격 효과 지속 시간 타이머
    private float hitDurationTimer = 0f;

    private void Awake()
    {
        if (instance == null) instance = this;

        // 볼륨 프로필에서 비네트 컴포넌트를 찾아옵니다.
        if (globalVolume.profile.TryGet(out Vignette v))
        {
            vignette = v;
        }
    }

    private void Update()
    {
        if (vignette == null) return;

        // 우선순위 1: 사망 (최우선 - 고정)
        if (isDead)
        {
            vignette.color.value = damageColor;
            vignette.intensity.value = Mathf.Lerp(vignette.intensity.value, 0.5f, Time.unscaledDeltaTime * 2f);
        }
        // 우선순위 2: 피격 당함! (0.5초간 강하게 붉어짐) [새로 추가된 로직]
        else if (hitDurationTimer > 0)
        {
            hitDurationTimer -= Time.unscaledDeltaTime;

            vignette.color.value = damageColor;

            // 깜빡이는 게 아니라, 순간적으로 진하게(0.6) 갔다가 서서히 빠짐
            vignette.intensity.value = Mathf.Lerp(vignette.intensity.value, 0.6f, Time.unscaledDeltaTime * 20f);
        }
        // 우선순위 3: 빈사 상태 (붉은색 깜빡임)
        else if (isLowHealth)
        {
            vignette.color.value = damageColor;
            blinkTimer += Time.unscaledDeltaTime * 10f;
            float blink = 0.4f + Mathf.Sin(blinkTimer) * 0.1f;
            vignette.intensity.value = Mathf.Lerp(vignette.intensity.value, blink, Time.unscaledDeltaTime * 10f);
        }
        // 우선순위 4: 평상시 (효과 없음)
        else
        {
            vignette.intensity.value = Mathf.Lerp(vignette.intensity.value, 0f, Time.unscaledDeltaTime * 5f);
        }
    }

    // 플레이어 체력 상태를 받아오는 함수
    public void CheckHealthStatus(float currentHp, float maxHp)
    {
        if (isDead) return;

        // 체력이 30% 이하면 빈사 상태 발동
        if (currentHp / maxHp <= 0.3f && currentHp > 0)
        {
            isLowHealth = true;
        }
        else
        {
            isLowHealth = false;
        }
    }
    
    // 사망 처리 함수
    public void SetDeathEffect()
    {
        isDead = true;
        isLowHealth = false; // 깜빡임 멈춤

        // 사망 패널 켜기
        if (deadPanel != null) deadPanel.SetActive(true);
    }

    public void PlayHitEffect()
    {
        // 타이머를 0.2~0.5초로 설정하여 '피격 상태'로 진입시킴
        hitDurationTimer = 0.3f;
    }
    //=================================================================
    // Player 스크립트에서 이 함수를 호출할 겁니다.
    public void UpdateEffect(float currentGauge, float maxGauge)
    {
        // 게이지 비율 계산 (0.0 ~ 1.0)
        float ratio = currentGauge / maxGauge;

        // 게이지가 30% 미만일 때 효과 발동
        if (ratio <= 0.3f)
        {
            // 게이지가 적을수록 더 진하게 (0에 가까우면 maxIntensity)
            // 30%일 때 0, 0%일 때 1이 되도록 역산
            float dangerFactor = 1.0f - (ratio / 0.3f);
            targetIntensity = dangerFactor * maxIntensity;
        }
        else
        {
            // 30% 이상이면 효과 없음
            targetIntensity = 0f;
        }
    }
}
