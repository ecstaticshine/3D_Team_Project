using UnityEngine;
using UnityEngine.Animations.Rigging; // 애니메이션 리깅 패키지 사용

public class EnemyAimAI : MonoBehaviour
{
    [Header("Targeting")]
    public Transform playerTransform; // 우리가 맞춰야 할 타겟 (플레이어)
    public float aimSpeed = 3.0f; // 조준이 따라가는 속도 (낮을수록 피하기 쉬워짐)

    [Header("Rigging Components")]
    public Rig aimRig; // 전체 리깅 시스템의 가중치 관리
    public MultiAimConstraint bodyAim; // 상체(Spine) 조준용
    public MultiAimConstraint weaponAim; // 총기(Weapon/Hand) 강제 조준용
    public TwoBoneIKConstraint leftHandIK; // 왼손 고정용

    [Header("Weapon Transform")]
    public Transform weaponLeftHandTarget; // 무기에 배치한 왼손 위치 (handPosition)

    private Transform aimTargetProxy; // 플레이어를 부드럽게 쫓아가는 가상의 조준점
    private float currentWeight = 0f; // 조준 가중치 (0에서 1로 서서히 변화)

    void Start()
    {
        // 1. 플레이어를 즉시 보지 않고 완충 역할을 해줄 가상 타겟 생성
        aimTargetProxy = new GameObject("AimTargetProxy").transform;
        aimTargetProxy.position = playerTransform.position;

        // 2. 상체 조준 설정: 가상 타겟을 바라보게 등록
        var bodyData = bodyAim.data.sourceObjects;
        bodyData.Clear();
        bodyData.Add(new WeightedTransform(aimTargetProxy, 1f));
        bodyAim.data.sourceObjects = bodyData;

        // 3. 총기 강제 조준 설정: 총기도 동일한 가상 타겟을 바라보게 하여 일체감 부여
        var weaponData = weaponAim.data.sourceObjects;
        weaponData.Clear();
        weaponData.Add(new WeightedTransform(aimTargetProxy, 1f));
        weaponAim.data.sourceObjects = weaponData;

        // 4. 왼손 IK 연결: 총기가 돌아가더라도 왼손은 항상 무기 손잡이에 붙어있게 함
        leftHandIK.data.target = weaponLeftHandTarget;

        // 5. 변경된 리깅 구조를 시스템에 새로고침
        GetComponent<RigBuilder>().Build();
    }

    void Update()
    {
        if (playerTransform == null) return;

        // 6. 가상 타겟이 플레이어를 aimSpeed 속도로 추격 (이게 있어야 조준 지연이 발생함)
        aimTargetProxy.position = Vector3.Lerp(aimTargetProxy.position, playerTransform.position, Time.deltaTime * aimSpeed);

        // 7. 조준 가중치를 부드럽게 상승 (갑자기 총을 겨누지 않도록)
        currentWeight = Mathf.Lerp(currentWeight, 1f, Time.deltaTime * 2f);
        aimRig.weight = currentWeight;
    }
}