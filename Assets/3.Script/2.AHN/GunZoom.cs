using UnityEngine;
using UnityEngine.InputSystem; // New Input System 기능을 사용하기 위해 필요합니다.

public class GunZoom : MonoBehaviour
{
    [Header("참조 변수")]
    public Transform weaponModel;
    public Transform hipPosition;
    public Transform adsPosition;//줌 조준 위치

    [Header("설정")]
    public float adsSpeed = 10f;
    private bool isAds = false;//조준 상태 확인

    private void OnZoom(InputValue value)
    {
        // value.isPressed: 버튼이 눌려 있으면 true, 떼면 false를 반환합니다.
        // 이 값을 isAds 변수에 실시간으로 저장합니다.
        isAds = value.isPressed;
        Debug.Log("ADS 상태: " + isAds);
    }

    void Update()
    {
        // 1. 목표 지점 결정
        Vector3 targetPos;
        Quaternion targetRot;

        if (isAds) // 조준키 입력
        {
            targetPos = adsPosition.localPosition;
            targetRot = adsPosition.localRotation;
        }
        else // 조준 키 해제
        {
            targetPos = hipPosition.localPosition;
            targetRot = hipPosition.localRotation;
        }

        // 2. 부드러운 이동 처리
        weaponModel.localPosition = Vector3.Lerp(weaponModel.localPosition, targetPos, Time.deltaTime * adsSpeed);

        weaponModel.localRotation = Quaternion.Slerp(weaponModel.localRotation, targetRot, Time.deltaTime * adsSpeed);
    }
}