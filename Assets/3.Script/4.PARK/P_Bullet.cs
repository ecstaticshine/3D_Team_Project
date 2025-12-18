using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

public class P_Bullet : MonoBehaviour 
{
//    [SerializeField] private float baseDamage = 50f;
//
//    private ObjectPool<GameObject> bulletManagedPool;
//    private TrailRenderer trailEffect;
//    private GameObject hitEffectPrefab; // 이펙트 저장용
//    private bool isReleased = false;
//
//    private void Awake()
//    {
//        trailEffect = GetComponentInChildren<TrailRenderer>();
//    }
//
//    public void SetManagedPool(ObjectPool<GameObject> pool)
//    {
//        bulletManagedPool = pool;
//    }
//
//    // [핵심] 총에서 이펙트 정보를 받아오는 함수
//    public void SetHitEffect(GameObject effect)
//    {
//        hitEffectPrefab = effect;
//    }
//
//    private void OnEnable()
//    {
//        isReleased = false;
//        if (trailEffect != null)
//        {
//            trailEffect.Clear();
//            trailEffect.emitting = false;
//            Invoke(nameof(EnableTrail), 0.05f);
//        }
//        StartCoroutine(DisableBulletAfterTime(3f));
//    }
//
//    private void EnableTrail()
//    {
//        if (trailEffect != null && gameObject.activeSelf) trailEffect.emitting = true;
//    }
//
//    private IEnumerator DisableBulletAfterTime(float time)
//    {
//        yield return new WaitForSeconds(time);
//        ReturnPool();
//    }
//
//    private void OnTriggerEnter(Collider other)
//    {
//        // 태그 확인 (Target)
//        if (other.CompareTag("Target"))
//        {
//            // S_Enemy는 아직 안 바꿨다고 가정합니다. (나중에 P_Enemy로 바꾸면 여기도 수정 필요)
//            S_Enemy enemy = other.GetComponentInParent<S_Enemy>();
//
//            if (enemy != null)
//            {
//                float finalDamage = baseDamage;
//                if (other.TryGetComponent(out S_HitBox hitBox))
//                {
//                    finalDamage *= hitBox.damageMultiplier;
//                }
//                enemy.TakeDamage(finalDamage);
//
//                // [핵심] 피 생성 (위치: 맞은 곳, 방향: 총알의 반대 방향)
//                SpawnHitEffect(transform.position, -transform.forward);
//            }
//        }
//        else
//        {
//            // 벽이나 바닥에 맞았을 때 (추후 벽 타격 이펙트 추가 가능)
//            // Debug.Log($"벽 충돌: {other.name}");
//        }
//
//        ReturnPool();
//    }
//
//    // [VFX 생성 로직]
//    private void SpawnHitEffect(Vector3 position, Vector3 direction)
//    {
//        if (hitEffectPrefab == null) return;
//
//        // direction(-transform.forward)을 바라보는 회전값 생성
//        Quaternion rot = Quaternion.LookRotation(direction);
//
//        GameObject vfx = Instantiate(hitEffectPrefab, position, rot);
//        Destroy(vfx, 2.0f); // 2초 뒤 삭제
//    }
//
//    private void ReturnPool()
//    {
//        if (isReleased || !gameObject.activeSelf) return;
//        isReleased = true;
//        if (trailEffect != null) trailEffect.emitting = false;
//
//        if (bulletManagedPool != null) bulletManagedPool.Release(gameObject);
//        else Destroy(gameObject);
//    }
}