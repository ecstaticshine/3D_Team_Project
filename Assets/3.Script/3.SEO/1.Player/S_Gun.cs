using UnityEngine;
using System.Collections;
using UnityEngine.Pool;

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

    [Header("탄약 및 상태")]
    private int totalAmmo;
    private int currentAmmo;
    public GunState gunState { get; private set; }

    private ObjectPool<GameObject> bulletPool;
    private float currentTimer;
    private bool isTriggerHeld = false;

    private void Awake()
    {
        bulletPool = new ObjectPool<GameObject>(
            createFunc: CreateBulletObject,
            actionOnGet: OnGetBullet,
            actionOnRelease: OnReleaseBullet,
            actionOnDestroy: OnDestroyBullet,
            collectionCheck: true,
            defaultCapacity: 20,
            maxSize: 100
        );
    }

    private GameObject CreateBulletObject()
    {
        GameObject bullet = Instantiate(gunData.bulletPrefab);

        if (bullet.TryGetComponent(out S_Bullet bulletScript))
        {
            bulletScript.SetManagedPool(bulletPool);
        }
        return bullet;
    }

    private void OnGetBullet(GameObject bullet)
    {
        if (bullet == null) return;

        bullet.transform.position = firePoint.position;
        bullet.transform.rotation = Quaternion.identity;

        bullet.SetActive(true);

        if (bullet.TryGetComponent(out Rigidbody rb))
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        // [추가] 총알에게 "너는 이 피 이펙트를 써라"라고 알려줌
        if (bullet.TryGetComponent(out S_Bullet bulletScript))
        {
            bulletScript.SetManagedPool(bulletPool); // 기존 코드
            bulletScript.SetHitEffect(gunData.hitEffectPrefab); // [여기 추가!]
        }
    }

    private void OnReleaseBullet(GameObject bullet)
    {
        if (bullet == null) return;

        bullet.SetActive(false);
    }

    private void OnDestroyBullet(GameObject bullet)
    {
        Destroy(bullet);
    }

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

        if (gunData == null || gunState == GunState.Reloading) return;

        CheckContinuousFire();
    }

    public void SetTriggerPressed(bool isPressed)
    {
        isTriggerHeld = isPressed;

        if (isPressed && gunData.fireMode != FireMode.FullAuto)
        {
            TryFire();
        }
    }

    private void CheckContinuousFire()
    {
        if (isTriggerHeld && gunData.fireMode == FireMode.FullAuto)
        {
            TryFire();
        }
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

        UpdateAmmoUI();
        ProcessShooting();
    }

    private void ProcessShooting()
    {
        Vector3 targetPoint = GetAimTargetPoint();
        Vector3 baseDirection = (targetPoint - firePoint.position).normalized;

        if (gunData.fireMode == FireMode.Shotgun)
        {
            for (int i = 0; i < gunData.pelletCount; i++)
            {
                Vector3 spreadDir = GetSpreadDirection(baseDirection, gunData.spreadAngle);
                FireBulletFromPool(spreadDir);
            }
        }
        else
        {
            FireBulletFromPool(baseDirection);
        }
    }

    private void FireBulletFromPool(Vector3 direction)
    {
        GameObject bullet = bulletPool.Get();

        if (bullet == null) return;

        bullet.transform.up = direction;

        if (bullet.TryGetComponent(out Rigidbody rb))
        {
            rb.linearVelocity = direction * gunData.bulletSpeed;
        }
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
        if (gameObject.activeInHierarchy && P_UIManager.instance != null)
        {
            P_UIManager.instance.UpdateAmmoText(currentAmmo, totalAmmo);
        }
    }

    private Vector3 GetAimTargetPoint()
    {
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        return Physics.Raycast(ray, out RaycastHit hit, 100f) ? hit.point : ray.GetPoint(100f);
    }
}