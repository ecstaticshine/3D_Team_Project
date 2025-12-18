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
    private GameObject hitEffectPrefab; // 이펙트

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

        if (trailEffect != null)
        {
            trailEffect.Clear();
            trailEffect.emitting = false;

            Invoke(nameof(EnableTrail), 0.05f);
        }

        StartCoroutine(DisableBulletAfterTime(3f));
    }

    private void EnableTrail()
    {
        if (trailEffect != null && gameObject.activeSelf)
        {
            trailEffect.emitting = true; // 3. 이제 다시 그려!
        }
    }

    private IEnumerator DisableBulletAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        ReturnPool();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Target")) return;

        Enemy enemy = other.GetComponentInParent<Enemy>();

        if (enemy != null)
        {
            float finalDamage = baseDamage;

            if (other.TryGetComponent(out HitBox hitBox))
            {
                finalDamage *= hitBox.damageMultiplier;
            }

            enemy.TakeDamage(finalDamage);

            // [핵심] 피 생성 (위치: 맞은 곳, 회전: 총알의 반대 방향)
            SpawnHitEffect(transform.position, -transform.forward);
        }
        else
        {
            Debug.Log($"벽 또는 오브젝트 : {other.name}");
        }

        ReturnPool();
    }

    // [핵심] 이펙트 생성 함수
    private void SpawnHitEffect(Vector3 position, Vector3 direction)
    {
        if (hitEffectPrefab == null) return;

        // Quaternion.LookRotation(direction): 해당 방향을 바라보는 회전값을 만듭니다.
        // direction에 -transform.forward(총알 반대)를 넣었으니, 사수 쪽을 보게 됩니다.
        Quaternion rot = Quaternion.LookRotation(direction);

        GameObject vfx = Instantiate(hitEffectPrefab, position, rot);
        Destroy(vfx, 2.0f); // 2초 뒤 삭제
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
