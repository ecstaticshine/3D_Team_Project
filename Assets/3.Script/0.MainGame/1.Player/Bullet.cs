using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class Bullet : MonoBehaviour
{
    #region Variable

    [SerializeField] private float baseDamage = 50f;

    private ObjectPool<GameObject> bulletManagedPool;
    private GameObject hitEffectPrefab;
    private TrailRenderer trailEffect;
    private LayerMask collisionMask;
    private Vector3 prevPosition;
    private bool isReleased = false;

    #endregion

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
            Hit(hit.collider, hit.point, direction);
        }

        prevPosition = transform.position;
    }

    #region Function

    private void Hit(Collider other, Vector3 hitPoint, Vector3 hitDirection)
    {
        if (isReleased) return;

        int otherLayer = other.gameObject.layer;
        int myLayer = gameObject.layer;

        if ((otherLayer == LayerMask.NameToLayer("PlayerBullet") ||
             otherLayer == LayerMask.NameToLayer("EnemyBullet")) &&
            otherLayer != myLayer)
        {
            SpawnCrashEffect(transform.position);

            if (other.TryGetComponent(out Bullet otherBullet))
            {
                otherBullet.gameObject.SetActive(false);
            }

            ReturnPool();
            return;
        }

        float finalDamage = baseDamage;
        bool isHeadShot = false;

        Enemy enemy = other.GetComponentInParent<Enemy>();
        Player player = other.GetComponentInParent<Player>();

        if (enemy != null)
        {
            ScoreManager.instance.AddShotHit();

            if (other.TryGetComponent(out HitBox hitBox))
            {
                finalDamage *= hitBox.damageMultiplier;

                if (hitBox.damageMultiplier >= 2.0f)
                {
                    isHeadShot = true;
                }
            }

            enemy.TakeDamage(finalDamage, hitPoint, hitDirection, isHeadShot);

            SpawnHitEffect(hitPoint, -hitDirection);
        }
        else if (player != null)
        {
            player.TakeDamage(baseDamage);
            SpawnHitEffect(hitPoint, -hitDirection);
        }
        else
        {
            SpawnHitEffect(hitPoint, -hitDirection);
        }

        ReturnPool();
    }

    private void SpawnCrashEffect(Vector3 position)
    {
        SoundSystem.EmitSound(position,20f);

        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlaySFX("BulletCrash"); 
        }

    }
    private void EnableTrail()
    {
        if (trailEffect != null && gameObject.activeSelf)
        {
            trailEffect.emitting = true;
        }
    }
    public void SetCollisionMask(LayerMask mask)
    {
        collisionMask = mask;
    }
    private IEnumerator DisableBulletAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        ReturnPool();
    }

    #endregion

    #region Effect

    public void SetHitEffect(GameObject effect)
    {
        hitEffectPrefab = effect;
    }
    private void SpawnHitEffect(Vector3 position, Vector3 direction)
    {
        if (hitEffectPrefab == null) return;

        Quaternion rot = Quaternion.LookRotation(direction);

        GameObject vfx = Instantiate(hitEffectPrefab, position, rot);
        Destroy(vfx, 2.0f);
    }

    #endregion

    #region Pool

    public void SetManagedPool(ObjectPool<GameObject> pool)
    {
        bulletManagedPool = pool;
    }
    public void ReturnPool()
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

    #endregion

    private void OnTriggerEnter(Collider other)
    {
        Vector3 hitPoint = other.ClosestPoint(transform.position);
        Vector3 hitDirection = transform.forward;

        Hit(other, hitPoint, hitDirection);
    }
}