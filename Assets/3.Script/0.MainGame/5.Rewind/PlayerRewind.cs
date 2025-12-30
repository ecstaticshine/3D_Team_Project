using System.Collections.Generic;
using UnityEngine;

public class PlayerRewind : RewindableObject
{
    private CharacterController charController;

    protected override void Awake()
    {
        TryGetComponent(out charController);
    }

    public override void Record()
    {
        base.Record();
    }

    // [되감기 시작]
    public override void StartRewind()
    {
        base.StartRewind();

        if (charController != null) charController.enabled = false;
    }

    // [되감기 중]
    public override bool RewindStep()
    {
        bool hasData = base.RewindStep();
        return hasData;
    }

    public override void StopRewind()
    {
        base.StopRewind();

        if (charController != null) charController.enabled = true;
    }
}