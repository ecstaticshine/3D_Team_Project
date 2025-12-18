using UnityEngine;

public class Item : MonoBehaviour
{
    private void Update()
    {
        transform.Rotate(Vector3.up * 50f * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.TryGetComponent(out Player player))
            {
                if (TryGetComponent(out IItem item))
                {
                    item.Use(player);
                }
            }
        }
    }
}