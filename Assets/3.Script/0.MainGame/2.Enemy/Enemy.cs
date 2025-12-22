using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("적 정보")]
    private bool isDead = false;
    public float MeleeDamage = 50f;
    public float maxHp = 100f;
    private float currentHp;

    [Header("적 장비")]
    public GunData enemyGunData;

    [Header("드랍 아이템")]
    public GameObject medicinePrefab;

    void Start()
    {
        isDead = false;
        currentHp = maxHp;
    }

    public void TakeDamage(float damage, bool isHeadshot = false)
    {
        if (isDead) return;

        currentHp -= damage;

        if (currentHp <= 0)
        {
            Die(isHeadshot);
        }
    }

    private void Die(bool isHeadshot)
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

        if (ScoreManager.instance != null)
        {
            ScoreManager.instance.AddKill(isHeadshot);
        }

        DropItem();

        Destroy(gameObject);
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
}