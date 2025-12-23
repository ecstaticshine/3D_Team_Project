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
        // 1. 조준 활성화 조건 판단
        // - 현재 상태가 Patrol(순찰) 상태가 아니어야 함.
        // - 시야(CanSeePlayer)에 플레이어가 들어와야 함.
        bool isNotPatrolling = !(aiController.currentState is J_PatrolState);
        bool canSee = aiController.CanSeePlayer();

        // 두 조건이 모두 충족되면 목표 가중치는 1, 아니면 0입니다.
        float targetWeight = (isNotPatrolling && canSee) ? 1.0f : 0.0f;

        // 2. 가중치를 부드럽게 보간 (Lerp)
        // 불릿타임(timeScaleMultiplier)을 고려하여 속도를 조절합니다.
        ikWeight = Mathf.MoveTowards(ikWeight, targetWeight, Time.deltaTime * weightSpeed * aiController.timeScaleMultiplier);

        // 3. 타겟 위치 갱신
        if (aiController.player != null)
            targetTransform = aiController.player.transform;
    }

    // [Step 1] 애니메이션 엔진의 IK 계산 단계 (손 위치 고정)
    private void OnAnimatorIK(int layerIndex)
    {
        if (animator == null) return;

        // 시선(LookAt) 처리: ikWeight만큼 타겟을 바라봅니다.
        animator.SetLookAtWeight(ikWeight);
        if (targetTransform != null) animator.SetLookAtPosition(targetTransform.position);

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
        // 가중치가 거의 0이거나 필수 참조가 없으면 계산을 생략합니다.
        if (ikWeight <= 0.001f || targetTransform == null || aimPoint == null) return;

        // 현재 총구(aimPoint)에서 타겟을 향하는 방향 벡터를 구합니다.
        Vector3 targetDirection = targetTransform.position - aimPoint.position;

        // 설정된 횟수만큼 반복하며 뼈를 조금씩 회전시켜 오차를 줄입니다 (CCD IK와 유사한 방식).
        for (int i = 0; i < iterations; i++)
        {
            for (int j = 0; j < boneTransforms.Length; j++)
            {
                // 각 뼈에 할당된 개별 weight와 전체 ikWeight를 곱해 최종 반영 수치를 결정합니다.
                float finalBoneWeight = humanBones[j].weight * ikWeight;
                AimAtTarget(boneTransforms[j], targetDirection, finalBoneWeight);
            }
        }
    }

    private void AimAtTarget(Transform bone, Vector3 targetDirection, float weight)
    {
        // 현재 총구가 바라보는 방향(aimPoint.forward)을 구합니다.
        Vector3 currentAimDir = aimPoint.forward;
        // '현재 조준 방향'에서 '타겟 방향'으로 가기 위한 회전값(Quaternion)을 계산합니다.
        Quaternion aimTowards = Quaternion.FromToRotation(currentAimDir, targetDirection);
        // weight만큼만 회전하도록 보간합니다.
        Quaternion blendedRotation = Quaternion.Slerp(Quaternion.identity, aimTowards, weight);
        // 뼈의 기존 회전에 계산된 회전값을 곱해 최종 회전력을 적용합니다.
        bone.rotation = blendedRotation * bone.rotation;
    }
}