using UnityEngine;

public class PlayerShooter : MonoBehaviour
{
    public Transform gunPivot;      // 총의 부모 오브젝트 (보통 오른손 근처에 위치)
    public Transform leftHandMount;  // 총 모델에 붙어있는 왼손잡이 위치
    public Transform rightHandMount; // 총 모델에 붙어있는 오른손잡이 위치

    [Range(0, 1)] public float aimWeight = 1.0f; // 조준 강도 (스크립트에서 조절 가능)

    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    // OnAnimatorIK는 매 프레임 애니메이터가 동작할 때 호출됩니다.
    private void OnAnimatorIK(int layerIndex)
    {
        if (animator == null) return;

        animator.SetIKPositionWeight(AvatarIKGoal.RightHand, aimWeight);
        animator.SetIKRotationWeight(AvatarIKGoal.RightHand, aimWeight);
        animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandMount.position);
        animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandMount.rotation);

        animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, aimWeight);
        animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, aimWeight);
        animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandMount.position);
        animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandMount.rotation);

        animator.SetLookAtWeight(aimWeight);
        animator.SetLookAtPosition(rightHandMount.position + rightHandMount.forward * 10f);
    }
}