using UnityEngine;

public class S_Enemy : MonoBehaviour
{
    [Header("적 정보")]
    public float maxHp = 100f;
    private float currentHp;

    [Header("적 장비")]
    public S_GunData enemyGunData;

    [Header("드랍 아이템")]
    public GameObject medicinePrefab;

    void Start()
    {
        currentHp = maxHp;
    }

    public void TakeDamage(float damage)
    {
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

        if (enemyGunData != null)
        {
            GameObject drop_gun = Instantiate(enemyGunData.gunPrefab, transform.position, Quaternion.identity);

            if (drop_gun.TryGetComponent(out S_Item_Gun item_gun))
            {
                item_gun.SetGunData(enemyGunData);
            }
        }

        Destroy(gameObject);
    }
}