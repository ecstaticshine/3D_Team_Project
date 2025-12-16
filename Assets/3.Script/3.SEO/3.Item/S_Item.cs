using UnityEngine;

public class S_Item : MonoBehaviour
{
    private void Update()
    {
        transform.Rotate(Vector3.up * 50f * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.TryGetComponent(out S_Player player))
            {
                if (TryGetComponent(out S_IItem item))
                {
                    item.Use(player);
                }
            }

            Destroy(gameObject);
        }
    }
}