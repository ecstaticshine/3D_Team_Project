using System;
using UnityEngine;
public class EnemyIKController : MonoBehaviour
{
    // Animator 컴포넌트를 참조하기 위한 변수입니다.
    private Animator animator;

    [Header("Targeting Settings")]
    // 적이 바라보고 조준할 대상의 위치(Transform)입니다.
    public Transform targetTransform;
    // IK가 적용되는 강도입니다 (0: 애니메이션 원본, 1: IK 완전 적용).
    [Range(0, 1)] public float ikWeight = 1.0f;

    [Header("Weapon Hand Anchors")]
    // 무기에 미리 설정해둔 '오른손이 잡을 위치' 오브젝트입니다.
    public Transform rightHandObj;
    // 무기에 미리 설정해둔 '왼손이 잡을 위치' 오브젝트입니다.
    public Transform leftHandObj;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void OnAnimatorIK(int layerIndex)
    {
        if (animator == null) return;

        animator.SetLookAtWeight(ikWeight);

        if (targetTransform != null)
        {
            // 캐릭터의 머리와 상체가 타겟의 위치를 바라보도록 설정합니다.
            animator.SetLookAtPosition(targetTransform.position);
        }

        if (rightHandObj != null)
        {
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, ikWeight);
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, ikWeight);
            animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandObj.position);
            animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandObj.rotation);
        }

        if (leftHandObj != null)
        {
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, ikWeight);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, ikWeight);
            animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandObj.position);
            animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandObj.rotation);
        }
    }
}