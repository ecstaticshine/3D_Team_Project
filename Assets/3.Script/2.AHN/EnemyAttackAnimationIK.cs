using UnityEngine;

public class EnemyAttackAnimationIK : MonoBehaviour
{
    private Animator animator;

    [Header("¼Õ À§Ä¡")]
    [SerializeField] private Transform LeftHandPosition;
    [SerializeField] private Transform RightHandPosition;
    [SerializeField] private Transform FirePoint;

    [Range(0, 1)] public float IkWeight = 1f;

    private void Start()
    {
        TryGetComponent(out animator);
    }
    public void RandomAttack()
    {
        int randomAction = Random.Range(0, 2);

        animator.SetInteger("AttackID", randomAction);

        animator.SetTrigger("IsAttack");
    }
    void OnAnimatorIK(int layerIndex)
    {
        if (animator == null) return;

        if (LeftHandPosition != null)
        {
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, IkWeight);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, IkWeight);

            animator.SetIKPosition(AvatarIKGoal.LeftHand, LeftHandPosition.position);
            animator.SetIKRotation(AvatarIKGoal.LeftHand, LeftHandPosition.rotation);
        }

        if (RightHandPosition != null)
        {
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, IkWeight);
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, IkWeight);

            animator.SetIKPosition(AvatarIKGoal.RightHand, RightHandPosition.position);
            animator.SetIKRotation(AvatarIKGoal.RightHand, RightHandPosition.rotation);
        }
    }
}
