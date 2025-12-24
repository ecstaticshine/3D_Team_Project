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
            Hit(hit.collider);
        }

        prevPosition = transform.position;
    }

    #region Function

    private void Hit(Collider other)
    {
        if (isReleased) return;

        Enemy enemy = other.GetComponentInParent<Enemy>();
        Player player = other.GetComponentInParent<Player>();

        float finalDamage = baseDamage;
        bool isHeadShot = false;

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

            enemy.TakeDamage(finalDamage, isHeadShot);

            SpawnHitEffect(transform.position, -transform.forward);
        }
        else if (player != null)
        {
            player.TakeDamage(baseDamage);
            SpawnHitEffect(transform.position, -transform.forward);
        }
        //else
        //{
        //    SpawnHitEffect(transform.position, -transform.forward);
        //}

        ReturnPool();
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

    #endregion

    private void OnTriggerEnter(Collider other)
    {
        Hit(other);

        Debug.Log($"[총알] 쾅! 부딪힌 물체 이름: {other.gameObject.name} / 태그: {other.gameObject.tag}");

        if (other.gameObject.CompareTag("Target"))
        {
            // [로그 2] 태그 조건 통과 확인
            Debug.Log("[총알] 'Target' 태그 확인됨!");

            TargetScript target = other.gameObject.GetComponent<TargetScript>();

            if (target != null)
            {
                target.isHit = true;
                // [로그 3] 스크립트 찾아서 변수 바꿨다고 출력
                Debug.Log("[총알] TargetScript 발견! isHit를 True로 변경함!");
            }
            else
            {
                // [로그 4] 태그는 맞는데 스크립트가 없을 때
                Debug.LogError("[총알] 태그는 맞는데, 그 물체에 'TargetScript'가 안 붙어 있어!");
            }
        }
    }
}