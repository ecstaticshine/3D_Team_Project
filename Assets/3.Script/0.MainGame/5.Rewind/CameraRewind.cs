using System.Collections.Generic;
using UnityEngine;

public class CameraRewind : RewindableObject
{
    private List<Quaternion> cameraRotations = new List<Quaternion>();

    [SerializeField] private Transform fpsCamera;
    private CharacterController charController;

    protected override void Awake()
    {
        base.Awake();
        TryGetComponent(out charController);
    }

    public override void Record()
    {
        base.Record(); // [유니] 부모가 먼저 위치랑 몸통 회전을 저장합니다.

        // [유니] 카메라도 똑같은 길이만큼만 저장해야 싱크가 맞겠지?
        int maxFrameCount = Mathf.RoundToInt(recordDuration / Time.fixedDeltaTime);

        if (fpsCamera != null)
        {
            if (cameraRotations.Count > maxFrameCount)
            {
                cameraRotations.RemoveAt(0);
            }
            cameraRotations.Add(fpsCamera.localRotation);
        }
    }

    public override void StartRewind()
    {
        if (charController != null) charController.enabled = false;
        base.StartRewind();
    }

    public override bool RewindStep()
    {
        bool hasData = base.RewindStep();

        if (hasData && fpsCamera != null && cameraRotations.Count > 0)
        {
            int last = cameraRotations.Count - 1;
            fpsCamera.localRotation = cameraRotations[last];
            cameraRotations.RemoveAt(last);
        }

        return hasData;
    }

    public override void StopRewind()
    {
        base.StopRewind();
        cameraRotations.Clear();
        if (charController != null) charController.enabled = true;
    }
}