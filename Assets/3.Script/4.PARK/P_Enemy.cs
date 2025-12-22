using UnityEngine;

public class P_Enemy : MonoBehaviour
{
    [Header("적 정보")]
    private bool isDead = false;
    public float MeleeDamage = 50f;
    public float maxHp = 100f;
    private float currentHp;
    private EnemyAlert alertSystem;
    private Transform playerTransform;

    [Header("적 장비")]
    public GunData enemyGunData;

    [Header("드랍 아이템")]
    public GameObject medicinePrefab;

    void Start()
    {
        isDead = false;
        currentHp = maxHp;
        // [추가 2] 내 몸에 붙은 알림 시스템 가져오기
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
    }

    void Update()
    {
        if (isDead) return;

        // 플레이어를 못 찾았으면 아무것도 안 함
        if (playerTransform == null) return;

        // [핵심 로직] 플레이어와의 거리 계산
        float distance = Vector3.Distance(transform.position, playerTransform.position);

        if (alertSystem != null)
        {
            if (distance < 5.0f) // 5미터 이내: 발각 (!)
            {
                alertSystem.SetState(EnemyAlert.AlertState.Detected);
            }
            else if (distance < 10.0f) // 10미터 이내: 의심 (?)
            {
                alertSystem.SetState(EnemyAlert.AlertState.Suspicious);
            }
            else // 멀어지면: 해제
            {
                alertSystem.SetState(EnemyAlert.AlertState.None);
            }
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHp -= damage;

        if (currentHp <= 0) Die();
    }

    private void Die()
    {
        //int randomValue = Random.Range(0, 2);
        //
        //switch (randomValue)
        //{
        //    case 0:
        //        if (medicinePrefab != null)
        //        {
        //            Instantiate(medicinePrefab, transform.position, Quaternion.identity);
        //        }
        //        break;
        //
        //    case 1:
        //        if (enemyGunData != null)
        //        {
        //            GameObject droppedGun = Instantiate(enemyGunData.gunPrefab, transform.position, Quaternion.identity);
        //
        //            if (droppedGun.TryGetComponent(out S_Item_Gun itemScript))
        //            {
        //                itemScript.SetGunData(enemyGunData);
        //            }
        //        }
        //        break;
        //}
        isDead = true;

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

        Destroy(gameObject);
    }
}