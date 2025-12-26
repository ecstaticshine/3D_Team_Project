using UnityEngine;
using UnityEngine.AI;

using System.Collections;

public class Enemy : MonoBehaviour
{
    [Header("적 정보")]
    private bool isDead = false;
    public float MeleeDamage = 50f;
    public float maxHp = 100f;
    private float currentHp;
    [SerializeField] private float impactForce = 30f;

    [Header("적 장비")]
    public GunData enemyGunData;

    [Header("드랍 아이템")]
    public GameObject medicinePrefab;

    [Header("Ragdoll Components")]
    private Rigidbody[] ragdollRigidbodies;
    private Animator animator;
    private NavMeshAgent navAgent;
    private CapsuleCollider mainCollider;
    private AIController aiController;
    private EnemyWeaponIK weaponIK;
    private AIShooter aiShooter;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        navAgent = GetComponent<NavMeshAgent>();
        mainCollider = GetComponent<CapsuleCollider>();

        aiController = GetComponent<AIController>();
        weaponIK = GetComponent<EnemyWeaponIK>();
        aiShooter = GetComponent<AIShooter>();

        ragdollRigidbodies = GetComponentsInChildren<Rigidbody>();
    }

    void Start()
    {
        isDead = false;
        currentHp = maxHp;

        DisableRagdoll();
    }

    public void TakeDamage(float damage, Vector3 hitPoint, Vector3 hitDirection, bool isHeadshot = false)
    {
        if (isDead) return;

        currentHp -= damage;

        if (aiController != null && aiController.player != null)
        {
            aiController.OnSoundHeard(aiController.player.transform.position);
        }

        if (currentHp <= 0)
        {
            Die(hitPoint, hitDirection, isHeadshot);
        }
    }

    public void TakeDamage(float damage, bool isHeadshot = false)
    {
        TakeDamage(damage, transform.position + Vector3.up, transform.forward * -1, isHeadshot);
    }

    private void Die(Vector3 hitPoint, Vector3 hitDirection, bool isHeadshot)
    {
        if (isDead) return;
        isDead = true;

        if (aiController != null) aiController.enabled = false;
        if (aiShooter != null) aiShooter.enabled = false;
        if (weaponIK != null) weaponIK.enabled = false;

        if (ScoreManager.instance != null) ScoreManager.instance.AddKill(isHeadshot);

        DropItem();

        ActivateRagdoll(hitPoint, hitDirection);

        StartCoroutine(DestroyAfterTime(5f));
    }

    private void DropItem()
    {
        if (enemyGunData != null)
        {
            Vector3 dropPosition = transform.position;

            if (Physics.Raycast(dropPosition + Vector3.up * 0.5f, Vector3.down, out RaycastHit hit, 2f))
            {
                dropPosition = hit.point + Vector3.up * 0.5f;
            }
            else
            {
                dropPosition.y += 0.5f;
            }

            GameObject drop_gun = Instantiate(enemyGunData.itemPrefab, dropPosition, Quaternion.identity);

            if (drop_gun.TryGetComponent(out Item_Gun item_gun))
            {
                item_gun.SetGunData(enemyGunData);
            }
        }
    }

    private void DisableRagdoll()
    {
        foreach (var rb in ragdollRigidbodies) rb.isKinematic = true;
        if (animator != null) animator.enabled = true;
        if (navAgent != null) { navAgent.enabled = true; navAgent.isStopped = false; }
        if (mainCollider != null) mainCollider.enabled = true;

        if (aiController != null) aiController.enabled = true;
        if (weaponIK != null) weaponIK.enabled = true;
        if (aiShooter != null) aiShooter.enabled = true;
    }

    private void ActivateRagdoll(Vector3 hitPoint, Vector3 hitDirection)
    {
        if (animator != null) animator.enabled = false;
        if (navAgent != null) { navAgent.isStopped = true; navAgent.enabled = false; }
        if (mainCollider != null) mainCollider.enabled = false;

        Rigidbody closestRB = null;
        float closestDist = float.MaxValue;

        foreach (var rb in ragdollRigidbodies)
        {
            rb.isKinematic = false;

            float dist = Vector3.SqrMagnitude(rb.position - hitPoint);
            if (dist < closestDist)
            {
                closestDist = dist;
                closestRB = rb;
            }
        }

        if (closestRB != null)
        {
            closestRB.AddForceAtPosition(hitDirection.normalized * impactForce, hitPoint, ForceMode.Impulse);
        }
    }

    private void ActivateRagdoll()
    {
        ActivateRagdoll(transform.position, Vector3.zero);
    }

    private IEnumerator DestroyAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }
}