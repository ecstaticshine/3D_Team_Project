using UnityEngine;
using System.Collections;

public class S_Gun : MonoBehaviour
{
    public enum GunState
    {
        Ready,
        Empty,
        Reloading
    }

    [Header("총 설정")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 50f;

    public float fireDelay = 0.7f;
    private float currentTimer;

    public float reloadTime = 2.0f;

    public GunState gunState { get; private set; }

    [Header("탄약 설정")]
    public int totalAmmo = 120;
    public int maxAmmo = 30;
    public int currentAmmo;

    [Header("조준 설정")]
    public Transform cameraTransform;

    private void Start()
    {
        gunState = GunState.Ready;
        currentTimer = fireDelay;
        currentAmmo = maxAmmo;
        UpdateAmmoUI();
    }

    private void Update()
    {
        currentTimer += Time.unscaledDeltaTime;
    }

    public void Fire()
    {
        if (gunState == GunState.Reloading || gunState == GunState.Empty) return;

        if (bulletPrefab == null || firePoint == null || cameraTransform == null || currentTimer < fireDelay) return;

        currentTimer = 0;
        currentAmmo--;

        if (currentAmmo <= 0)
        {
            currentAmmo = 0;
            gunState = GunState.Empty;
        }

        UpdateAmmoUI();

        Vector3 targetPoint = GetAimTargetPoint();
        Vector3 fireDirection = (targetPoint - firePoint.position).normalized;

        GameObject newBullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        newBullet.transform.up = fireDirection;

        if (newBullet.TryGetComponent(out Rigidbody bulletRb))
        {
            bulletRb.linearVelocity = fireDirection * bulletSpeed;
        }

        Destroy(newBullet, 3.0f);
    }

    public void AddAmmo(int amount)
    {
        totalAmmo += amount;
        UpdateAmmoUI();
    }

    private void UpdateAmmoUI()
    {
        if (S_UIManager.instance != null)
        {
            S_UIManager.instance.UpdateAmmoText(currentAmmo, totalAmmo);
        }
    }

    public void Reload()
    {
        if (gunState == GunState.Reloading || currentAmmo >= maxAmmo || totalAmmo <= 0) return;

        Debug.Log("장전 시작");

        StartCoroutine(ReloadCoroutine());

    }

    private IEnumerator ReloadCoroutine()
    {
        gunState = GunState.Reloading;

        yield return new WaitForSecondsRealtime(reloadTime);

        int ammoToFill = maxAmmo - currentAmmo;
        int ammoToTake = Mathf.Min(ammoToFill, totalAmmo);

        currentAmmo += ammoToTake;
        totalAmmo -= ammoToTake;

        gunState = GunState.Ready;
        Debug.Log("장전 끝");

        UpdateAmmoUI();
    }

    private Vector3 GetAimTargetPoint()
    {
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            return hit.point;
        }
        else
        {
            return ray.GetPoint(100f);
        }
    }
}