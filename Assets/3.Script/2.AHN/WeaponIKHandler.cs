using UnityEngine;
using UnityEngine.Animations.Rigging; 

public class WeaponIKHandler : MonoBehaviour
{
    [Header("IK Constraints")]
    public TwoBoneIKConstraint leftHandIK;
    public TwoBoneIKConstraint rightHandIK;

    private GameObject currentWeapon;

    public void SetupWeaponIK(GameObject newWeapon)
    {
        currentWeapon = newWeapon;

        Transform leftTarget = currentWeapon.transform.Find("LeftHand_Target");
        Transform rightTarget = currentWeapon.transform.Find("RightHand_Target");

        if (leftTarget != null)
        {
            leftHandIK.data.target = leftTarget;
        }

        if (rightTarget != null)
        {
            rightHandIK.data.target = rightTarget;
        }

    }

    public void SetIKWeight(float weight)
    {
        leftHandIK.weight = weight;
        rightHandIK.weight = weight;
    }
}