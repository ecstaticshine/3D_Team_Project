using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager instance = null;
    [Header("중앙 HUD")]
    [SerializeField] private Image hpGauge;      // 붉은색 HP 게이지 (Filled 타입)
    [SerializeField] private Image apGauge;      // 노란색 시간(AP) 게이지 (Filled 타입)

    [Header("월드 UI")]
    [SerializeField] private TextMeshProUGUI ammoText; // 총에 붙은 텍스트

    private void Awake()
    {
        if(instance == null) instance = this;
        else Destroy(gameObject);
    }

    // 1. HP 게이지 업데이트 (0.0 ~ 1.0 사이 값으로 받음)
    public void UpdateHP(float currentHp, float maxHp)
    {
        // 1. 현재 체력 비율 계산 (0.0 ~ 1.0)
        float ratio = currentHp / maxHp;
        // 2. 전체 원의 21%만 사용하도록 보정
        // 체력이 꽉 차도 fillAmount는 0.21이 됩니다.
        hpGauge.fillAmount = ratio * 0.21f;
    }


    public void UpdateAbilitySlider(float currentGauge)
    {
        // 1. 현재 게이지 비율 계산 (0 ~ 100 -> 0.0 ~ 1.0)
        float ratio = currentGauge / 100f;

        // 2. 전체 원의 21%만 사용하도록 보정 (HP바와 대칭)
        apGauge.fillAmount = ratio * 0.21f;
    }

    // 3. 탄약 텍스트 업데이트
    public void UpdateAmmoText(int currentAmmo, int totalAmmo)
    {
        // 스승님의 디자인대로 '현재 탄약'만 크게 보여줍니다.
        ammoText.text = currentAmmo.ToString();

        //  "7 / 35" 처럼 전체 탄약 확인.
        ammoText.text = $"{currentAmmo} / {totalAmmo}";

        // 탄약이 0이면 빨간색 경고
        ammoText.color = currentAmmo == 0 ? Color.red : Color.white;
    }
}
