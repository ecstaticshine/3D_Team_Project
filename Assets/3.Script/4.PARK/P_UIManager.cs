using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro 사용 시 필수
using UnityEngine.InputSystem; // [필수] 뉴 인풋 시스템 네임스페이스

public class P_UIManager : MonoBehaviour
{
    // 어디서든 접근 가능한 싱글톤
    public static P_UIManager instance;

    [Header("중앙 HUD")]
    [SerializeField] private Image hpGauge;      // 붉은색 HP 게이지 (Filled 타입)
    [SerializeField] private Image apGauge;      // 노란색 시간(AP) 게이지 (Filled 타입)
    [SerializeField] private Image dashGauge;

    [Header("월드 UI")]
    [SerializeField] private TextMeshProUGUI ammoText; // 총에 붙은 텍스트

    [Header("패널 연결")]
    [SerializeField] private GameObject settingsPanel; // 방금 만든 Panel_Settings 연결

    [Header("입력 연결 (New Input System)")]
    // 인스펙터에서 만들어둔 'Pause'나 'Menu' 액션을 여기에 드래그해서 넣으세요
    [SerializeField] private InputActionReference menuAction;

    private bool isSettingsOpen = false;

    private void Awake()
    {
        if (instance == null) instance = this;
    }
    private void OnEnable()
    {
        if (menuAction != null)
        {
            menuAction.action.Enable();
            // 버튼이 눌렸을 때(performed) ToggleSettingsPanel을 실행해라! 라고 구독
            menuAction.action.performed += OnMenuPerformed;
        }
    }
    void Start()
    {
        // 시작할 땐 설정창 닫아두기
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    // 2. 스크립트가 꺼질 때 입력 감지 해제 (메모리 누수 방지)
    private void OnDisable()
    {
        if (menuAction != null)
        {
            menuAction.action.Disable();
            menuAction.action.performed -= OnMenuPerformed;
        }
    }

    // 입력 시스템이 호출하는 함수 (Callback)
    private void OnMenuPerformed(InputAction.CallbackContext context)
    {
        ToggleSettingsPanel();
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

    // 2. 능력(시간) 게이지 업데이트
    public void UpdateAbilitySlider(float currentGauge) // 기존 코드와 호환
    {
        // 1. 현재 게이지 비율 계산 (0 ~ 100 -> 0.0 ~ 1.0)
        float ratio = currentGauge / 100f;

        // 2. 전체 원의 21%만 사용하도록 보정 (HP바와 대칭)
        apGauge.fillAmount = ratio * 0.21f;
    }

    // 3. 탄약 텍스트 업데이트
    // [수정됨] 함수 이름을 S_Gun에서 호출하는 이름과 똑같이 맞췄습니다.
    // S_Gun에서는 (current, total) 두 개를 보내므로 여기서도 두 개를 받아야 에러가 안 납니다.
    public void UpdateAmmoText(int currentAmmo, int totalAmmo)
    {
        // 스승님의 디자인대로 '현재 탄약'만 크게 보여줍니다.
        ammoText.text = currentAmmo.ToString();

        // (옵션) 만약 "7 / 35" 처럼 전체 탄약도 보고 싶으시면 아래 주석을 해제하세요.
         ammoText.text = $"{currentAmmo} / {totalAmmo}";

        // 탄약이 0이면 빨간색 경고
        ammoText.color = currentAmmo == 0 ? Color.red : Color.white;
    }
    public void UpdateDashSlider(float amount)
    {
        dashGauge.fillAmount = amount * 0.21f;

        dashGauge.color = (amount >= 1f) ? Color.skyBlue : new Color(1, 1, 1, 0.5f);
    }
    public void ToggleSettingsPanel()
    {
        isSettingsOpen = !isSettingsOpen;

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(isSettingsOpen);
        }

        // 설정창이 열리면 시간을 멈추고 마우스 커서를 보여줌
        if (isSettingsOpen)
        {
            Time.timeScale = 0f;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Time.timeScale = 1f;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }


}
