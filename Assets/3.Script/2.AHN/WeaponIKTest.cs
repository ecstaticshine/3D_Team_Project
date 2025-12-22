using UnityEngine;


[System.Serializable]
public class HumanBone
{
    public HumanBodyBones bone;
    public float weight = 1.0f;
}
public class WeaponIKTest : MonoBehaviour
{
    public Transform targetTransform;
    public Transform aimPoint;

    public int iterations = 10;
    [Range(0, 1)]
    public float weight = 1.0f;

    public float angleLimit = 90f;
    public float distanceLimit = 1.5f;
    public HumanBone[] humanBones;
    Transform[] boneTransform;

    private Animator animator;

    private void Start()
    {
        TryGetComponent(out animator);
        boneTransform = new Transform[humanBones.Length];
        for(int i = 0; i < boneTransform.Length; i++ )
        {
            boneTransform[i] = animator.GetBoneTransform(humanBones[i].bone);
        }
    }
    Vector3 GetTargetPosition()
    {
        Vector3 targetDirection = targetTransform.position - aimPoint.position;
        Vector3 aimDirection = aimPoint.forward;
        float blendOut = 0f;

        float targetAngle = Vector3.Angle(targetDirection, aimDirection);
        if(targetAngle >angleLimit)
        {
            blendOut += (targetAngle - angleLimit) / 50f;
        }

        float targetDistance = targetDirection.magnitude;
        if (targetDistance < distanceLimit)
        {
            blendOut += distanceLimit - targetDistance;
        }
        Vector3 direction = Vector3.Slerp(targetDirection, aimDirection, blendOut);
        return aimPoint.position + direction;
    }

    private void LateUpdate()
    {
        if(aimPoint == null)
        {
            return;
        }
        if(targetTransform == null)
        {
            return;
        }

        Vector3 targetPosition = GetTargetPosition();

        for(int i = 0; i< iterations; i++)
        {
            for(int j = 0; j< boneTransform.Length; j++)
            {
                Transform bone = boneTransform[j];
                float boneWeight = humanBones[j].weight * weight;
                AimAtTarget(bone, targetPosition, boneWeight);
            }
        }
    }

    private void AimAtTarget(Transform bone, Vector3 targetPosition, float weight)
    {
        Vector3 aimDirection = aimPoint.forward;
        Vector3 targetDirection = targetPosition - aimPoint.position;
        Quaternion aimTowards = Quaternion.FromToRotation(aimDirection, targetDirection);
        Quaternion blendedRotation = Quaternion.Slerp(Quaternion.identity, aimTowards, weight);
        bone.rotation = blendedRotation * bone.rotation;
    }

    public void SetTargetTransform(Transform target)
    {
        targetTransform = target;
    }
    public void SetAimTransform(Transform aim)
    {
        aimPoint = aim;
    }
}
