using UnityEngine;

public class S_Enemy : MonoBehaviour
{
    [Header("상태")]
    public float maxHp = 100f;
    private float currentHp;

    [Header("드랍 아이템")]
    public GameObject medicinePrefab;
    public GameObject gunPrefab;

    private GameObject itemToDrop;

    void Start()
    {
        currentHp = maxHp;
    }

    public void TakeDamage(float damage)
    {
        currentHp -= damage;

        if (currentHp <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        int randomValue = Random.Range(0, 2);

        switch (randomValue)
        {
            case 0:
                itemToDrop = medicinePrefab;
                break;
            case 1:
                itemToDrop = gunPrefab;
                break;
        }


        if (itemToDrop != null)
        {
            Instantiate(itemToDrop, transform.position, itemToDrop.transform.rotation);
        }

        Destroy(gameObject);
    }
}