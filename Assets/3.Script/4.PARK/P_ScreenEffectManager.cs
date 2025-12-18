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
    [SerializeField] private Volume globalVolume; // 아까 만든 Global Volume을 여기에 넣으세요

    [Header("설정")]
    [SerializeField] private float effectSpeed = 5f; // 효과가 켜지고 꺼지는 속도
    [SerializeField] private float maxIntensity = 0.5f; // 최대 얼마나 어둡게 할지 (0.0 ~ 1.0)

    // 내부 변수
    private Vignette vignette;
    private float targetIntensity = 0f;

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
        if (vignette != null)
        {
            // 현재 값에서 목표 값으로 부드럽게 변경 (Lerp)
            // 깜빡거리지 않고 자연스럽게 빨개집니다.
            float current = vignette.intensity.value;
            vignette.intensity.value = Mathf.Lerp(current, targetIntensity, Time.unscaledDeltaTime * effectSpeed);
        }
    }

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
