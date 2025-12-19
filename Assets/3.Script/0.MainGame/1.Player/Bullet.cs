using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float baseDamage = 50f;

    private ObjectPool<GameObject> bulletManagedPool;
    private TrailRenderer trailEffect;
    private bool isReleased = false;
    private GameObject hitEffectPrefab;
    private LayerMask collisionMask;
    private Vector3 prevPosition;

    public void SetHitEffect(GameObject effect)
    {
        hitEffectPrefab = effect;
    }

    public void SetManagedPool(ObjectPool<GameObject> pool)
    {
        bulletManagedPool = pool;
    }

    private void Awake()
    {
        trailEffect = GetComponentInChildren<TrailRenderer>();
    }

    private void OnEnable()
    {
        isReleased = false;

        prevPosition = transform.position;

        if (trailEffect != null)
        {
            trailEffect.Clear();
            trailEffect.emitting = false;

            Invoke(nameof(EnableTrail), 0.05f);
        }

        StartCoroutine(DisableBulletAfterTime(3f));
    }

    private void Update()
    {
        if (isReleased) return;

        Vector3 direction = (transform.position - prevPosition).normalized;
        float distance = Vector3.Distance(prevPosition, transform.position);

        if (distance > 0 && Physics.Raycast(prevPosition, direction, out RaycastHit hit, distance, collisionMask))
        {
            HandleHit(hit.collider);
        }

        //if (Physics.Raycast(prevPosition, direction, out RaycastHit hit, distance))
        //{
        //    if (hit.collider.CompareTag("Target") || hit.collider.CompareTag("Player"))
        //    {
        //        HandleHit(hit.collider);
        //    }
        //}

        prevPosition = transform.position;
    }

    public void SetCollisionMask(LayerMask mask)
    {
        collisionMask = mask;
    }

    private void EnableTrail()
    {
        if (trailEffect != null && gameObject.activeSelf)
        {
            trailEffect.emitting = true;
        }
    }

    private IEnumerator DisableBulletAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        ReturnPool();
    }

    private void OnTriggerEnter(Collider other)
    {
        HandleHit(other);
    }

    private void HandleHit(Collider other)
    {
        if (isReleased) return;

        Enemy enemy = other.GetComponentInParent<Enemy>();
        Player player = other.GetComponentInParent<Player>();

        if (enemy != null)
        {
            float finalDamage = baseDamage;

            if (other.TryGetComponent(out HitBox hitBox))
            {
                finalDamage *= hitBox.damageMultiplier;
            }

            enemy.TakeDamage(finalDamage);

            SpawnHitEffect(transform.position, -transform.forward);
        }
        else if (player != null)
        {
            player.TakeDamage(baseDamage);
            SpawnHitEffect(transform.position, -transform.forward);
        }
        else
        {
            SpawnHitEffect(transform.position, -transform.forward);
        }

        ReturnPool();
    }

    private void SpawnHitEffect(Vector3 position, Vector3 direction)
    {
        if (hitEffectPrefab == null) return;

        Quaternion rot = Quaternion.LookRotation(direction);

        GameObject vfx = Instantiate(hitEffectPrefab, position, rot);
        Destroy(vfx, 2.0f);
    }

    private void ReturnPool()
    {
        if (isReleased || !gameObject.activeSelf) return;

        isReleased = true;

        if (trailEffect != null)
        {
            trailEffect.emitting = false;
        }

        if (bulletManagedPool != null)
        {
            bulletManagedPool.Release(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}