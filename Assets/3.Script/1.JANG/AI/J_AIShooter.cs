using System.Collections;
using UnityEngine;

public class J_AIShooter : MonoBehaviour
{
    public enum GunState { Ready, Empty, Reloading }

    [Header("설정")]
    [SerializeField] private GunData gunData;
    public GunData currentGunData => gunData;

    [Header("연결")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private Animator gunAnimator;

    [Header("탄약 및 상태")]
    private int totalAmmo;
    private int currentAmmo;
    public GunState gunState { get; private set; }
    private float currentTimer;
    private bool isTriggerHeld = false;

    private void OnEnable()
    {
        gunState = GunState.Ready;
    }

    private void Start()
    {
        if (gunData != null)
        {
            currentAmmo = gunData.maxAmmo;
            totalAmmo = gunData.maxAmmo * 2;
        }
    }

    private void Update()
    {
        currentTimer += Time.deltaTime;

        if (gunData == null || gunState == GunState.Reloading) return;

    }

    public void TryFire()
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
        AudioManager.instance.PlaySFX(gunData.fireSoundName);

        ProcessShooting();
    }

    private void ProcessShooting()
    {
        Vector3 targetPoint = GetAimTargetPoint();
        Vector3 baseDirection = (targetPoint - firePoint.position).normalized;

        if (gunData.fireMode == GunFireMode.Shotgun)
        {
            for (int i = 0; i < gunData.pelletCount; i++)
            {
                Vector3 spreadDir = GetSpreadDirection(baseDirection, gunData.spreadAngle);
                CreateBullet(spreadDir);
            }
        }
        else
        {
            CreateBullet(baseDirection);
        }
    }

    private void CreateBullet(Vector3 direction)
    {
        GameObject bullet = Instantiate(gunData.bulletPrefab, firePoint.position, Quaternion.identity);

        bullet.layer = LayerMask.NameToLayer("EnemyBullet");

        bullet.transform.up = direction;

        if (bullet.TryGetComponent(out Bullet bulletScript))
        {
            LayerMask mask = LayerMask.GetMask("Player", "Default", "Wall");
            bulletScript.SetCollisionMask(mask);
        }

        if (bullet.TryGetComponent(out Rigidbody rb))
        {
            rb.linearVelocity = direction * gunData.bulletSpeed;
        }

        Destroy(bullet, 3f);
    }

    private Vector3 GetSpreadDirection(Vector3 baseDir, float angle)
    {
        float x = Random.Range(-angle, angle);
        float y = Random.Range(-angle, angle);

        Quaternion spreadRot = Quaternion.LookRotation(baseDir) * Quaternion.Euler(x, y, 0);
        return spreadRot * Vector3.forward;
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
    }

    public void InitializeGun()
    {
        if (gunData == null) return;
        currentAmmo = gunData.maxAmmo;
        totalAmmo = gunData.maxAmmo * 2;
        gunState = GunState.Ready;
    }


    private Vector3 GetAimTargetPoint()
    {
        Ray ray = new Ray(firePoint.position, firePoint.forward);
        return Physics.Raycast(ray, out RaycastHit hit, 100f) ? hit.point : ray.GetPoint(100f);
    }
}