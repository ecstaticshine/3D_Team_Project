using UnityEngine;

// 뼈의 정보와 제약 조건을 저장하기 위한 클래스
[System.Serializable]
public class HumanBone
{
    public HumanBodyBones bone; // 유니티 Humanoid 뼈 종류
    public float weight = 1.0f; // 해당 뼈가 조준에 기여하는 가중치 (0~1)
    public float maxAngle = 40.0f; // 해당 뼈가 회전할 수 있는 최대 각도 제한
}

public class WeaponIKTest : MonoBehaviour
{
    public Transform targetTransform; // 우리가 조준해야 할 실제 목표물
    public Transform aimPoint;        // 총구 등 실제 조준선이 나가는 시작점

    public int iterations = 10;       // IK 계산을 반복할 횟수 (높을수록 정확하지만 성능 소모)
    [Range(0, 1)]
    public float weight = 1.0f;       // 전체 시스템의 작동 가중치

    public float angleLimit = 90f;    // 조준 가능한 최대 허용 각도
    public float distanceLimit = 1.5f; // 조준 가능한 최소 거리
    public float smoothTime = 5f;     // 가중치 변화 시 부드러운 전환 속도

    public HumanBone[] humanBones;    // 조준에 사용할 뼈들의 배열
    Transform[] boneTransforms;       // 실제 캐릭터의 Transform 컴포넌트 배열

    private float currentWeight = 0f; // 현재 적용 중인 실시간 가중치 (애니메이션 튐 방지)
    private Animator animator;        // 캐릭터의 애니메이터

    private void Start()
    {
        // 캐릭터의 Animator 컴포넌트를 가져옴
        TryGetComponent(out animator);

        // 설정된 뼈의 개수만큼 Transform 배열 크기 할당
        boneTransforms = new Transform[humanBones.Length];

        for (int i = 0; i < humanBones.Length; i++)
        {
            // 애니메이터에서 실제 Humanoid 뼈의 위치 정보를 찾아 저장
            boneTransforms[i] = animator.GetBoneTransform(humanBones[i].bone);
        }
    }

    // 현재 조준이 가능한 상태인지 판단하고 가중치를 계산하는 함수
    float CalculateTargetWeight()
    {
        if (targetTransform == null || aimPoint == null) return 0f;

        // 조준점에서 목표물까지의 방향 벡터 계산
        Vector3 targetDirection = targetTransform.position - aimPoint.position;
        // 캐릭터가 바라보는 정면 방향
        Vector3 forwardDirection = transform.forward;

        // 1. 각도 체크: 정면에서 목표물이 너무 뒤에 있으면 조준 불가
        float angle = Vector3.Angle(forwardDirection, targetDirection);
        if (angle > angleLimit) return 0f;

        // 2. 거리 체크: 목표물이 너무 가까우면 조준 불가
        float distance = targetDirection.magnitude;
        if (distance < distanceLimit) return 0f;

        return weight; // 모든 조건 만족 시 설정된 가중치 반환
    }

    private void LateUpdate()
    {
        if (boneTransforms == null || aimPoint == null) return;

        // 목표 가중치를 계산하고 Lerp를 통해 부드럽게 변화시킴 (갑작스러운 회전 방지)
        float targetW = CalculateTargetWeight();
        currentWeight = Mathf.Lerp(currentWeight, targetW, Time.deltaTime * smoothTime);

        // 가중치가 거의 0이라면 계산 생략
        if (currentWeight <= 0.001f) return;

        // 정해진 횟수만큼 IK 반복 계산 (CCD 알고리즘 핵심)
        for (int i = 0; i < iterations; i++)
        {
            for (int j = 0; j < boneTransforms.Length; j++)
            {
                Transform bone = boneTransforms[j];
                // 전체 가중치와 개별 뼈의 가중치를 결합
                float combinedWeight = humanBones[j].weight * currentWeight;

                // 실제 회전 처리를 담당하는 함수 호출
                RotateBoneTowardsTarget(bone, targetTransform.position, combinedWeight);
            }
        }
    }

    private void RotateBoneTowardsTarget(Transform bone, Vector3 targetPosition, float weight)
    {
        // 1. 현재 뼈의 상태에서 총구가 목표를 향하기 위해 필요한 회전량 계산
        Vector3 curAimDir = aimPoint.forward; // 현재 총구가 보는 방향
        Vector3 targetDir = targetPosition - aimPoint.position; // 총구에서 목표까지의 방향

        // 현재 방향에서 목표 방향으로 가기 위한 회전값(Quaternion) 생성
        Quaternion aimTowards = Quaternion.FromToRotation(curAimDir, targetDir);

        // 2. 뼈의 현재 회전에 계산된 회전량을 곱함 (무게값 적용)
        // Quaternion.Slerp를 사용하여 weight만큼만 회전하도록 보간
        Quaternion blendedRotation = Quaternion.Slerp(Quaternion.identity, aimTowards, weight);

        // 뼈의 회전값 갱신 (부모 뼈부터 순차적으로 회전이 누적됨)
        bone.rotation = blendedRotation * bone.rotation;

        // 3. 가동 범위 제한 (선택 사항: 뼈가 너무 기괴하게 꺾이지 않도록 함)
        // 여기서는 간단하게 설명하기 위해 로직 흐름만 유지합니다.
    }
}