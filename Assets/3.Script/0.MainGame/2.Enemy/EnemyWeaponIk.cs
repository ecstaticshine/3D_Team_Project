using UnityEngine;

public class EnemyWeaponIK : MonoBehaviour
{
    [System.Serializable]
    public class BoneSetting
    {
        public HumanBodyBones bone;
        public float weight = 1.0f;
    }

    [Header("Targeting")]
    public Transform targetTransform;
    public Transform aimPoint;
    public float weight = 1.0f;
    public float smoothTime = 5f;

    [Header("Lower Body Fix (Male Pose)")]
    [Range(0, 1)] public float kneeSpreadWeight = 0.7f;
    public float kneeSpreadDistance = 0.35f;

    [Header("IK Settings")]
    public int iterations = 10;
    public BoneSetting[] humanBones;

    private Transform[] boneTransforms;
    private float currentWeight = 0f;
    private Animator animator;

    // ==========================================
    // 추가된 변수
    // ==========================================
    private AIController aiController; // AI의 현재 상태를 확인하기 위한 참조 변수

    private void Start()
    {
        // aiController: 같은 오브젝트에 부착된 AIController 컴포넌트를 가져와 할당합니다.
        aiController = GetComponent<AIController>();

        if (TryGetComponent(out animator))
        {
            boneTransforms = new Transform[humanBones.Length];
            for (int i = 0; i < humanBones.Length; i++)
            {
                boneTransforms[i] = animator.GetBoneTransform(humanBones[i].bone);
            }
        }
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (animator == null) return;

        animator.SetIKHintPosition(AvatarIKHint.RightKnee, animator.GetIKHintPosition(AvatarIKHint.RightKnee) + (transform.right * kneeSpreadDistance));
        animator.SetIKHintPositionWeight(AvatarIKHint.RightKnee, kneeSpreadWeight);

        animator.SetIKHintPosition(AvatarIKHint.LeftKnee, animator.GetIKHintPosition(AvatarIKHint.LeftKnee) - (transform.right * kneeSpreadDistance));
        animator.SetIKHintPositionWeight(AvatarIKHint.LeftKnee, kneeSpreadWeight);

        Quaternion footRot = Quaternion.LookRotation(transform.forward);
        animator.SetIKRotation(AvatarIKGoal.LeftFoot, footRot);
        animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, kneeSpreadWeight);
        animator.SetIKRotation(AvatarIKGoal.RightFoot, footRot);
        animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, kneeSpreadWeight);
    }

    private void LateUpdate()
    {
        if (boneTransforms == null || aimPoint == null || targetTransform == null || aiController == null) return;

        // targetWeight: AI가 전투 상태(CombatState)일 때만 설정된 weight(1.0)를 사용하고, 아니면 0으로 만듭니다.
        // aiController.currentState: 현재 AI의 상태 인스턴스입니다.
        // aiController.combatState: AIController에 미리 생성된 전투 상태 인스턴스입니다.
        float targetWeight = (aiController.currentState == aiController.combatState) ? weight : 0f;

        // currentWeight: 현재 적용 중인 가중치를 targetWeight를 향해 smoothTime 속도로 부드럽게 변화시킵니다.
        // Time.deltaTime: 프레임 간의 시간 간격을 곱해 일정한 속도로 보간합니다.
        currentWeight = Mathf.Lerp(currentWeight, targetWeight, Time.deltaTime * smoothTime);

        // 가중치가 거의 0이라면 연산을 생략하여 성능을 최적화합니다.
        if (currentWeight <= 0.001f) return;

        for (int i = 0; i < iterations; i++)
        {
            for (int j = 0; j < boneTransforms.Length; j++)
            {
                if (boneTransforms[j] == null) continue;

                // combinedWeight: 각 뼈의 개별 가중치와 전체 IK 가중치를 곱하여 최종 계산 값을 얻습니다.
                float combinedWeight = humanBones[j].weight * currentWeight;
                RotateBoneTowardsTarget(boneTransforms[j], targetTransform.position, combinedWeight);
            }
        }
    }

    private void RotateBoneTowardsTarget(Transform bone, Vector3 targetPosition, float weight)
    {
        // curAimDir: 현재 총구가 바라보고 있는 방향(Forward)입니다.
        Vector3 curAimDir = aimPoint.forward;
        // targetDir: 총구 위치에서 타겟(플레이어 등)을 향하는 벡터입니다.
        Vector3 targetDir = targetPosition - aimPoint.position;

        // aimTowards: 현재 방향에서 타겟 방향으로 회전해야 하는 회전값을 계산합니다.
        Quaternion aimTowards = Quaternion.FromToRotation(curAimDir, targetDir);
        // blendedRotation: 계산된 회전값을 가중치(weight)에 따라 부드럽게 섞습니다 (0이면 회전 안 함, 1이면 완전 조준).
        Quaternion blendedRotation = Quaternion.Slerp(Quaternion.identity, aimTowards, weight);

        // bone.rotation: 기존 뼈의 회전에 계산된 IK 회전값을 곱하여 뼈를 최종적으로 회전시킵니다.
        bone.rotation = blendedRotation * bone.rotation;
    }
}