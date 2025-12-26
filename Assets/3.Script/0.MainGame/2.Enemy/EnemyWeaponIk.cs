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

    private void Start()
    {
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


        Vector3 rightKneePos = animator.GetIKHintPosition(AvatarIKHint.RightKnee) + (transform.right * kneeSpreadDistance);
        animator.SetIKHintPosition(AvatarIKHint.RightKnee, rightKneePos);
        animator.SetIKHintPositionWeight(AvatarIKHint.RightKnee, kneeSpreadWeight);

        Vector3 leftKneePos = animator.GetIKHintPosition(AvatarIKHint.LeftKnee) - (transform.right * kneeSpreadDistance);
        animator.SetIKHintPosition(AvatarIKHint.LeftKnee, leftKneePos);
        animator.SetIKHintPositionWeight(AvatarIKHint.LeftKnee, kneeSpreadWeight);

        Quaternion footRot = Quaternion.LookRotation(transform.forward);
        animator.SetIKRotation(AvatarIKGoal.LeftFoot, footRot);
        animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, kneeSpreadWeight);
        animator.SetIKRotation(AvatarIKGoal.RightFoot, footRot);
        animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, kneeSpreadWeight);
    }

    private void LateUpdate()
    {
        if (boneTransforms == null || aimPoint == null || targetTransform == null) return;

        currentWeight = Mathf.Lerp(currentWeight, weight, Time.deltaTime * smoothTime);
        if (currentWeight <= 0.001f) return;

        for (int i = 0; i < iterations; i++)
        {
            for (int j = 0; j < boneTransforms.Length; j++)
            {
                if (boneTransforms[j] == null) continue;
                float combinedWeight = humanBones[j].weight * currentWeight;
                RotateBoneTowardsTarget(boneTransforms[j], targetTransform.position, combinedWeight);
            }
        }
    }

    private void RotateBoneTowardsTarget(Transform bone, Vector3 targetPosition, float weight)
    {
        Vector3 curAimDir = aimPoint.forward;
        Vector3 targetDir = targetPosition - aimPoint.position;

        Quaternion aimTowards = Quaternion.FromToRotation(curAimDir, targetDir);
        Quaternion blendedRotation = Quaternion.Slerp(Quaternion.identity, aimTowards, weight);

        bone.rotation = blendedRotation * bone.rotation;
    }
}