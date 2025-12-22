using UnityEngine;

public class Item_Gun : MonoBehaviour, IItem
{
    private GunData gunData;

    public void SetGunData(GunData data)
    {
        if (data == null) return;

        gunData = data;

        transform.localScale = gunData.itemScale;
    }

    public void Use(Player player)
    {
        if (gunData != null)
        {
            player.GunPickup(gunData);

            Destroy(gameObject);
        }
    }
}