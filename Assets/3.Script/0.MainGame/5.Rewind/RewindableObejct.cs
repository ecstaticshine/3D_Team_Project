using System.Collections.Generic;
using UnityEngine;

public class RewindableObject : MonoBehaviour
{
    protected List<Vector3> positions = new List<Vector3>();
    protected List<Quaternion> rotations = new List<Quaternion>();

    protected Rigidbody rigidBody;
    protected bool isRewinding = false;

    protected virtual void Awake()
    {
        TryGetComponent(out rigidBody);
    }

    protected virtual void OnEnable()
    {
        if (TimeRewindManager.Instance != null)
        {
            TimeRewindManager.Instance.RegisterObject(this);
        }
        else
        {
            TimeRewindManager manager = FindAnyObjectByType<TimeRewindManager>();
            if (manager != null) manager.RegisterObject(this);
        }
    }

    protected virtual void OnDisable()
    {
        if (TimeRewindManager.Instance != null) TimeRewindManager.Instance.UnregisterObject(this);
        ClearData();
    }

    public virtual void Record()
    {
        positions.Add(transform.position);
        rotations.Add(transform.rotation);
    }

    public virtual void StartRewind()
    {
        isRewinding = true;
        if (rigidBody != null) rigidBody.isKinematic = true;
    }

    public virtual bool RewindStep()
    {
        if (positions.Count > 0)
        {
            int lastIndex = positions.Count - 1;
            transform.position = positions[lastIndex];
            transform.rotation = rotations[lastIndex];

            positions.RemoveAt(lastIndex);
            rotations.RemoveAt(lastIndex);
            return true;
        }
        return false;
    }

    public virtual void StopRewind()
    {
        isRewinding = false;
        if (rigidBody != null) rigidBody.isKinematic = false;
        ClearData();
    }

    protected void ClearData()
    {
        positions.Clear();
        rotations.Clear();
    }
}