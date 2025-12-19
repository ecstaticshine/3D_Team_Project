using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    private Animator animator; 

    [Header("IK Targets")]
    public Transform leftHandObj;  
    public Transform rightHandObj; 

    [Header("Aiming Settings")]
    public Transform targetPlayer; // 조준할 플레이어의 Transform
    public Transform spineBone;    // 상체 회전을 위한 척추 뼈 (보통 Spine1, Spine2 등)
    public Transform firePoint;    // 총알이 나가는 위치

    [Header("IK Weight")]
    [Range(0, 1)]
    public float ikWeight = 1.0f;  

    void Start()
    {
        TryGetComponent(out animator);
    }

    // 유니티 내부 애니메이션 계산이 끝난 후 IK를 적용하기 위해 호출되는 함수입니다.
    // 애니메이터 설정의 'IK Pass'가 체크되어 있어야 작동합니다.
    void OnAnimatorIK(int layerIndex)
    {
        if (animator == null) return;

        // --- 오른손 IK 설정 ---
        animator.SetIKPositionWeight(AvatarIKGoal.RightHand, ikWeight);
        animator.SetIKRotationWeight(AvatarIKGoal.RightHand, ikWeight);
        animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandObj.position);
        animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandObj.rotation);

        // --- 왼손 IK 설정 ---
        animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, ikWeight);
        animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, ikWeight);
        animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandObj.position);
        animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandObj.rotation);
    }

    void LateUpdate()
    {
        if (targetPlayer == null || spineBone == null) return;

        // --- 상체 조준 로직 ---
        // FirePoint가 플레이어를 향하도록 방향 벡터를 계산 (플레이어 위치 - 총구 위치).
        Vector3 aimDirection = targetPlayer.position - firePoint.position;

        // 해당 방향을 바라보는 회전값을 계산
        Quaternion targetRotation = Quaternion.LookRotation(aimDirection);

        // 척추 뼈를 해당 방향으로 회전시킵니다. 
        spineBone.rotation = targetRotation;
    }
}