using UnityEngine;
using UnityEngine.InputSystem; // New Input System 기능을 사용하기 위해 필요합니다.

public class GunZoom : MonoBehaviour
{
    [Header("참조 변수")]
    public Transform weaponModel;
    public Transform hipPosition;
    public Transform adsPosition;//줌 조준 위치

    public Camera mainCamera;
    public Camera overlayCamera;

    [Header("설정")]
    public float adsSpeed = 10f;
    public float normalFOV = 60f; // 평상시 시야각
    public float zoomFOV = 40f;   // 조준 시 시야각 (값이 작을수록 더 크게 확대돼요)
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

        float targetFOV = isAds ? zoomFOV : normalFOV;

        // 메인 카메라 줌 (실제 확대 효과)
        if (mainCamera != null)
        {
            mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, targetFOV, Time.deltaTime * adsSpeed);
        }

        // 오버레이 카메라 줌 (무기가 너무 어색하게 커지지 않도록 조절)
        if (overlayCamera != null)
        {
            // 무기 카메라도 메인과 보조를 맞춰주면 화면이 훨씬 자연스러워진답니다.
            overlayCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, targetFOV, Time.deltaTime * adsSpeed);
        }
    }
}