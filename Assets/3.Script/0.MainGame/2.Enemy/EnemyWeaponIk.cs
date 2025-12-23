using UnityEngine;

public class EnemyWeaponIK : MonoBehaviour
{
    // ===============================
    // 컴포넌트 및 참조 변수
    // ===============================
    private Animator animator; // 캐릭터의 애니메이션을 제어하는 컴포넌트입니다.
    private J_AIController aiController; // 현재 AI의 상태(Patrol 등)를 확인하기 위한 메인 컨트롤러입니다.

    [Header("Targeting")]
    public Transform targetTransform; // 조준할 대상(플레이어)의 위치입니다.
    public Transform aimPoint; // 총구 끝이나 조준점 등 '실제 발사 방향'의 기준이 되는 오브젝트입니다.

    [Range(0, 1)]
    public float ikWeight = 0f; // 전체적인 IK 적용 강도입니다 (0: 꺼짐, 1: 완전 조준).
    public float weightSpeed = 2.5f; // 조준 자세로 전환되는 속도입니다.
    private Vector3 smoothedTargetPos;

    [Header("Hand IK (Step 1)")]
    // 무기 모델에 자식으로 붙어있는 '오른손/왼손 잡이용 빈 오브젝트'를 할당합니다.
    public Transform rightHandAnchor;
    public Transform leftHandAnchor;

    [Header("Upper Body IK (Step 2)")]
    public int iterations = 10; // 조준 계산을 반복하여 정확도를 높이는 횟수입니다.
    public HumanBone[] humanBones; // 조준에 관여할 뼈(Spine, Chest, Neck 등)의 배열입니다.
    private Transform[] boneTransforms; // animator에서 찾은 실제 뼈의 Transform들입니다.

    private void Awake()
    {
        // 1. 필요한 컴포넌트들을 내 오브젝트에서 가져옵니다.
        animator = GetComponent<Animator>();
        aiController = GetComponent<J_AIController>();

        // 2. humanBones 배열에 설정된 HumanBodyBones 정보를 실제 Transform으로 변환하여 저장합니다.
        boneTransforms = new Transform[humanBones.Length];
        for (int i = 0; i < humanBones.Length; i++)
        {
            // animator.GetBoneTransform을 통해 유니티 휴머노이드 뼈대를 직접 참조합니다.
            boneTransforms[i] = animator.GetBoneTransform(humanBones[i].bone);
        }
    }

    private void Update()
    {
        bool isNotPatrolling = !(aiController.currentState is J_PatrolState);
        bool canSee = aiController.CanSeePlayer();
        float targetWeight = (isNotPatrolling && canSee) ? 1.0f : 0.0f;

        ikWeight = Mathf.MoveTowards(ikWeight, targetWeight, Time.deltaTime * weightSpeed * aiController.timeScaleMultiplier);

        if (aiController.EnemyChasePosition != null)
        {
            targetTransform = aiController.EnemyChasePosition;
        }
        else if (aiController.player != null)
        {
            targetTransform = aiController.player.transform;
        }
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (animator == null || targetTransform == null) return;

        // 3. [수정] 시선 처리 시에도 정확한 타겟 좌표 사용
        animator.SetLookAtWeight(ikWeight);
        // targetTransform.position: 이제 가슴 위치를 가리키므로 바닥을 보지 않습니다.
        animator.SetLookAtPosition(targetTransform.position);
        // 오른손 위치 고정
        if (rightHandAnchor != null)
        {
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, ikWeight);
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, ikWeight);
            animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandAnchor.position);
            animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandAnchor.rotation);
        }

        // 왼손 위치 고정
        if (leftHandAnchor != null)
        {
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, ikWeight);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, ikWeight);
            animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandAnchor.position);
            animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandAnchor.rotation);
        }
    }

    // [Step 2] 모든 애니메이션 계산 후 실행 (상체 회전 미세 조정)
    private void LateUpdate()
    {
        if (ikWeight <= 0.001f || targetTransform == null || aimPoint == null) return;

        // 1. [핵심] 타겟의 위치를 즉시 반영하지 않고 미세하게 부드럽게 만듭니다. (떨림 방지)
        // Vector3.Lerp를 통해 현재 프레임의 플레이어 위치로 서서히 이동시켜 "지직"거리는 오차를 흡수합니다.
        smoothedTargetPos = Vector3.Lerp(smoothedTargetPos, targetTransform.position, 0.2f);

        // 2. 보정된 좌표로 방향 벡터를 구합니다.
        Vector3 targetDirection = smoothedTargetPos - aimPoint.position;

        for (int i = 0; i < iterations; i++)
        {
            for (int j = 0; j < boneTransforms.Length; j++)
            {
                float finalBoneWeight = humanBones[j].weight * ikWeight;
                AimAtTarget(boneTransforms[j], targetDirection, finalBoneWeight);
            }
        }
    }

    private void AimAtTarget(Transform bone, Vector3 targetDirection, float weight)
    {
        // aimPoint.forward: 현재 총구가 가리키는 실제 방향입니다.
        Vector3 currentAimDir = aimPoint.forward;

        // 1. 현재 조준 방향에서 타겟 방향으로 가기 위한 차이(Rotation)를 계산합니다.
        Quaternion aimTowards = Quaternion.FromToRotation(currentAimDir, targetDirection);

        // 2. Quaternion.identity(회전 없음)와 aimTowards 사이를 weight만큼 보간합니다.
        Quaternion blendedRotation = Quaternion.Slerp(Quaternion.identity, aimTowards, weight);

        // 3. 기존 회전에 자연스럽게 더해줍니다. 
        // 이때 뼈의 회전이 너무 급격하면 떨릴 수 있으므로, 최종 결과값에 제한을 주는 효과가 있습니다.
        bone.rotation = blendedRotation * bone.rotation;
    }
}