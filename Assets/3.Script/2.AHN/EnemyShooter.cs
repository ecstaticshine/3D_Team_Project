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
        // 스크립트가 시작될 때 해당 오브젝트의 Animator를 가져와 변수에 할당합니다.
        animator = GetComponent<Animator>();
    }

    // 유니티 엔진에서 IK 연산이 일어날 때마다 자동으로 호출되는 특수 함수입니다.
    void OnAnimatorIK(int layerIndex)
    {
        // animator 변수가 비어있지 않은지 확인합니다.
        if (animator == null) return;

        // IK 가중치를 설정합니다. 1이면 타겟에 완전히 고정되고, 0이면 무시합니다.
        animator.SetLookAtWeight(ikWeight);

        // 타겟이 존재할 경우에만 시선 처리를 수행합니다.
        if (targetTransform != null)
        {
            // 캐릭터의 머리와 상체가 타겟의 위치를 바라보도록 설정합니다.
            animator.SetLookAtPosition(targetTransform.position);
        }

        // --- 오른손(Right Hand) IK 설정 ---
        if (rightHandObj != null)
        {
            // 오른손의 위치(Position) 가중치를 설정합니다.
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, ikWeight);
            // 오른손의 회전(Rotation) 가중치를 설정합니다.
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, ikWeight);
            // 오른손을 무기의 오른손 잡는 지점(rightHandObj)의 위치로 이동시킵니다.
            animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandObj.position);
            // 오른손의 회전값을 무기의 잡는 지점 회전값과 일치시킵니다.
            animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandObj.rotation);
        }

        // --- 왼손(Left Hand) IK 설정 ---
        if (leftHandObj != null)
        {
            // 왼손의 위치(Position) 가중치를 설정합니다.
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, ikWeight);
            // 왼손의 회전(Rotation) 가중치를 설정합니다.
            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, ikWeight);
            // 왼손을 무기의 왼손 잡는 지점(leftHandObj)의 위치로 이동시킵니다.
            animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandObj.position);
            // 왼손의 회전값을 무기의 잡는 지점 회전값과 일치시킵니다.
            animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandObj.rotation);
        }
    }
}