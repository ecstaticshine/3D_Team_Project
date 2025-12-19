using UnityEngine;

public class PlayerShooter : MonoBehaviour
{
    public enum WeaponType { Pistol, Rifle, ShotGun }
    [Header("현재 총기")]
    public WeaponType currentWeapon = WeaponType.Pistol;

    [Tooltip("메인카메라 권총에 자식으로 되어있습니다")]
    [Header("권총 좌표값")]
    public Transform gunPivot;      
    public Transform leftHandMount; 
    public Transform rightHandMount; 

    [Header("라이플 좌표값")]
    public Transform RiflePivot;
    public Transform leftHandMount_Rifle;
    public Transform rightHandMount_Rifle;

    [Header("샷건 좌표값")]
    public Transform ShotGunPivot;
    public Transform leftHandMount_ShotGun;
    public Transform rightHandMount_ShotGun;



    [Range(0, 1)] public float aimWeight = 1.0f; // 조준 강도 (스크립트에서 조절 가능)

    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (animator == null) return;

        Transform targetLeft = (currentWeapon == WeaponType.Rifle) ? leftHandMount_Rifle : leftHandMount;
        Transform targetRight = (currentWeapon == WeaponType.Rifle) ? rightHandMount_Rifle : rightHandMount;

        if (targetLeft == null || targetRight == null) return;

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