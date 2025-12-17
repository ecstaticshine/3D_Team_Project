using UnityEngine;

public class S_Item_Gun : MonoBehaviour, S_IItem
{
    private S_GunData gunData;

    public void SetGunData(S_GunData data)
    {
        if (data == null) return;

        gunData = data;

        transform.localScale = new Vector3(3f, 3f, 3f);
    }

    public void Use(S_Player player)
    {
        if (gunData != null)
        {
            player.HandleGunPickup(gunData);

            Destroy(gameObject);
        }
    }
}