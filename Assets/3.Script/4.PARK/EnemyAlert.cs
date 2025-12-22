using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAlert : MonoBehaviour
{
    [Header("아이콘 연결")]
    [SerializeField] private GameObject questionIcon;    // ? (의심)
    [SerializeField] private GameObject exclamationIcon; // ! (발각)

    [Header("설정")]
    [SerializeField] private float alertDuration = 2.0f; // ! 떠있는 시간

    // 상태 관리용 (외부에서 호출)
    public enum AlertState { None, Suspicious, Detected }

    private AlertState currentState = AlertState.None;

    public void SetState(AlertState state)
    {
        if (currentState == state) return; // 같은 상태면 무시
        currentState = state;

        // 일단 다 끄고 시작
        HideAll();

        switch (state)
        {
            case AlertState.Suspicious:
                if (questionIcon != null) questionIcon.SetActive(true);
                // 의심 상태는 계속 유지되거나, AI 로직에 따라 꺼질 것임
                break;

            case AlertState.Detected:
                if (exclamationIcon != null) exclamationIcon.SetActive(true);
                // 발각(!)은 보통 2초 뒤에 사라지거나 전투 모드로 들어감
                Invoke(nameof(HideAll), alertDuration);
                break;

            case AlertState.None:
                HideAll();
                break;
        }
    }

    private void HideAll()
    {
        if (questionIcon != null) questionIcon.SetActive(false);
        if (exclamationIcon != null) exclamationIcon.SetActive(false);
    }
}
