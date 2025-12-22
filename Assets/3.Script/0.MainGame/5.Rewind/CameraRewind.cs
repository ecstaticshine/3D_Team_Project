using System.Collections.Generic;
using UnityEngine;

public class CameraRewind : RewindableObject
{
    [SerializeField] private Transform playerCamera;

    private List<Quaternion> cameraRotations = new List<Quaternion>();

    private CharacterController charController;

    protected override void Awake()
    {
        base.Awake();
        TryGetComponent(out charController);
    }

    public override void Record()
    {
        base.Record();

        int maxFrameCount = Mathf.RoundToInt(recordDuration / Time.fixedDeltaTime);

        if (playerCamera != null)
        {
            if (cameraRotations.Count > maxFrameCount)
            {
                cameraRotations.RemoveAt(0);
            }
            cameraRotations.Add(playerCamera.localRotation);
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

        if (hasData && playerCamera != null && cameraRotations.Count > 0)
        {
            int last = cameraRotations.Count - 1;
            playerCamera.localRotation = cameraRotations[last];
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