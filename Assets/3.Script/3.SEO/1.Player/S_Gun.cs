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

    [Header("총 데이터")]
    [SerializeField] private S_GunData gunData;
    private Animator gunAnimator;

    [Header("발사 위치")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Transform firePoint;

    [Header("탄약 설정")]
    [SerializeField] private int totalAmmo = 120;
    [SerializeField] private int currentAmmo;
    public GunState gunState { get; private set; }
    private float currentTimer;


    private void Start()
    {
        if (gunData == null) return;

        if(gunData.gunAnimation != null && gunAnimator != null)
        {
            gunAnimator.runtimeAnimatorController = gunData.gunAnimation;
        }

        gunState = GunState.Ready;
        currentTimer = gunData.fireDelay;
        currentAmmo = gunData.maxAmmo;
        UpdateAmmoUI();
    }

    private void Update()
    {
        currentTimer += Time.unscaledDeltaTime;
    }

    public void Fire()
    {
        if (gunState == GunState.Reloading || gunState == GunState.Empty) return;

        if (gunData.bulletPrefab == null || firePoint == null || cameraTransform == null || currentTimer < gunData.fireDelay) return;

        if (gunAnimator != null) gunAnimator.SetTrigger("Fire");

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

        GameObject newBullet = Instantiate(gunData.bulletPrefab, firePoint.position, Quaternion.identity);
        newBullet.transform.up = fireDirection;

        if (newBullet.TryGetComponent(out Rigidbody bulletRb))
        {
            bulletRb.linearVelocity = fireDirection * gunData.bulletSpeed;
        }
        AudioManager.instance.PlaySFX("Fire");
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
        if (gunState == GunState.Reloading || currentAmmo >= gunData.maxAmmo || totalAmmo <= 0) return;

        Debug.Log("장전 시작");

        StartCoroutine(ReloadCoroutine());

    }

    private IEnumerator ReloadCoroutine()
    {
        gunState = GunState.Reloading;

        if (gunAnimator != null) gunAnimator.SetTrigger("Reload");

        yield return new WaitForSecondsRealtime(gunData.reloadTime);

        int ammoToFill = gunData.maxAmmo - currentAmmo;
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