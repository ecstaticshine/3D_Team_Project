using System.Collections.Generic;
using UnityEngine;

public class RewindableObject : MonoBehaviour
{
    [Header("되감기 설정")]
    [Tooltip("최대 몇 초 전까지 되돌릴까요? (초 단위)")]
    [SerializeField] protected float recordDuration = 15f; // [유니] 기본 15초로 늘려놨어!

    protected List<Vector3> positions = new List<Vector3>();
    protected List<Quaternion> rotations = new List<Quaternion>();

    protected Rigidbody rb;
    protected bool isRewinding = false;

    protected virtual void Awake()
    {
        TryGetComponent(out rb);
    }

    protected virtual void OnEnable()
    {
        // [유니] 매니저 등록 로직 (안전을 위해 그대로 유지)
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
        // [유니] 여기가 핵심! 고정된 300 대신, 오빠가 설정한 시간만큼 계산해서 저장해.
        // FixedUpdate는 보통 0.02초마다 도니까, (시간 / 0.02) 하면 저장할 개수가 나와!
        int maxFrameCount = Mathf.RoundToInt(recordDuration / Time.fixedDeltaTime);

        if (positions.Count > maxFrameCount)
        {
            positions.RemoveAt(0);
            rotations.RemoveAt(0);
        }

        positions.Add(transform.position);
        rotations.Add(transform.rotation);
    }

    public virtual void StartRewind()
    {
        isRewinding = true;
        if (rb != null) rb.isKinematic = true;
    }

    public virtual void StopRewind()
    {
        isRewinding = false;
        if (rb != null) rb.isKinematic = false;
        ClearData();
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

    protected void ClearData()
    {
        positions.Clear();
        rotations.Clear();
    }
}