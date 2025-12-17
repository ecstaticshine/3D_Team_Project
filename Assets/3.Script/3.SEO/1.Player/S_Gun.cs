using UnityEngine;
using System.Collections;

public class S_Gun : MonoBehaviour
{
    public enum GunState { Ready, Empty, Reloading }

    [Header("설정")]
    [SerializeField] private S_GunData gunData;
    public S_GunData currentGunData => gunData;

    [Header("연결")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Animator gunAnimator;

    [Header("탄약")]
    private int totalAmmo;
    private int currentAmmo;
    public GunState gunState { get; private set; }
    private float currentTimer;

    private void OnEnable()
    {
        gunState = GunState.Ready;

        UpdateAmmoUI();
    }

    private void Start()
    {
        if (gunData != null)
        {
            currentAmmo = gunData.maxAmmo;
            totalAmmo = gunData.maxAmmo * 2;
        }

        if (cameraTransform == null && Camera.main != null) cameraTransform = Camera.main.transform;

        UpdateAmmoUI();
    }

    private void Update()
    {
        currentTimer += Time.unscaledDeltaTime;
    }

    public void Fire()
    {
        if (gunState != GunState.Ready) return;
        if (currentTimer < gunData.fireDelay) return;
        if (currentAmmo <= 0)
        {
            Reload();
            return;
        }

        currentTimer = 0;
        currentAmmo--;

        if (gunAnimator != null) gunAnimator.SetTrigger("Fire");

        Vector3 targetPoint = GetAimTargetPoint();
        Vector3 dir = (targetPoint - firePoint.position).normalized;

        GameObject bullet = Instantiate(gunData.bulletPrefab, firePoint.position, Quaternion.identity);
        bullet.transform.up = dir;

        if (bullet.TryGetComponent(out Rigidbody rb))
        {
            rb.linearVelocity = dir * gunData.bulletSpeed;
        }

        AudioManager.instance.PlaySFX(gunData.fireSoundName);

        UpdateAmmoUI();
        Destroy(bullet, 3f);
    }

    public void Reload()
    {
        if (gunState == GunState.Reloading || currentAmmo >= gunData.maxAmmo || totalAmmo <= 0) return;
        StartCoroutine(ReloadCoroutine());
    }

    private IEnumerator ReloadCoroutine()
    {
        gunState = GunState.Reloading;
        if (gunAnimator != null) gunAnimator.SetTrigger("Reload");

        yield return new WaitForSecondsRealtime(gunData.reloadTime);

        int need = gunData.maxAmmo - currentAmmo;
        int take = Mathf.Min(need, totalAmmo);

        currentAmmo += take;
        totalAmmo -= take;

        gunState = GunState.Ready;
        UpdateAmmoUI();
    }

    public void AddAmmo(int amount)
    {
        totalAmmo += amount;
        UpdateAmmoUI();
    }

    public void InitializeGun()
    {
        if (gunData == null) return;

        currentAmmo = gunData.maxAmmo;

        totalAmmo = gunData.maxAmmo * 2;

        gunState = GunState.Ready;

        UpdateAmmoUI();
    }

    private void UpdateAmmoUI()
    {
        if (gameObject.activeInHierarchy && S_UIManager.instance != null)
        {
            S_UIManager.instance.UpdateAmmoText(currentAmmo, totalAmmo);
        }
    }

    private Vector3 GetAimTargetPoint()
    {
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        return Physics.Raycast(ray, out RaycastHit hit, 100f) ? hit.point : ray.GetPoint(100f);
    }
}