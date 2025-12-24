using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Pool;

public class Gun : MonoBehaviour
{
    #region Variable

    public enum GunState { Ready, Empty, Reloading }

    [Header("설정")]
    [SerializeField] private GunData gunData;
    public GunData currentGunData => gunData;

    [Header("연결")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Animator gunAnimator;

    [Header("설정")]
    [SerializeField] private bool isPlayerGun = true;

    [Header("탄피 설정")]
    [SerializeField] private CasingType casingType;
    [SerializeField] private Transform casingExitLocation;
    [SerializeField] private float ExitPower = 300;

    [Header("Muzzle Flash 설정")]
    [SerializeField] private GameObject muzzleFlashPrefab;
    [SerializeField] private int flashPoolSize = 3;
    private List<ParticleSystem> _flashPool = new List<ParticleSystem>();
    private int _currentFlashIndex = 0;

    public GunState gunState { get; private set; }
    private int totalAmmo;
    private int currentAmmo;

    private ObjectPool<GameObject> bulletPool;
    private float currentTimer;
    private bool isTriggerHeld = false;

    #endregion

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

        if (muzzleFlashPrefab != null && firePoint != null)
        {
            for (int i = 0; i < flashPoolSize; i++)
            {
                GameObject obj = Instantiate(muzzleFlashPrefab, firePoint);
                if (obj.TryGetComponent(out ParticleSystem ps))
                {
                    ps.Stop(); 
                    _flashPool.Add(ps);
                }
            }
        }
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

    public void InitializeGun()
    {
        if (gunData == null) return;
        currentAmmo = gunData.maxAmmo;
        totalAmmo = gunData.maxAmmo * 2;
        gunState = GunState.Ready;
        UpdateAmmoUI();
    }

    #region Pool

    private GameObject CreateBulletObject()
    {
        GameObject bullet = Instantiate(gunData.bulletPrefab);

        if (bullet.TryGetComponent(out Bullet bulletScript))
        {
            bulletScript.SetManagedPool(bulletPool);
        }
        return bullet;
    }

    private void FireBulletFromPool(Vector3 direction)
    {
        GameObject bullet = bulletPool.Get();

        if (bullet == null) return;

        string layerName = isPlayerGun ? "PlayerBullet" : "EnemyBullet";

        bullet.layer = LayerMask.NameToLayer(layerName);

        bullet.transform.up = direction;

        if (bullet.TryGetComponent(out Rigidbody rb))
        {
            rb.linearVelocity = direction * gunData.bulletSpeed;
        }
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

        if (bullet.TryGetComponent(out Bullet bulletScript))
        {
            bulletScript.SetManagedPool(bulletPool);
            bulletScript.SetHitEffect(gunData.hitEffectPrefab);
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

    #endregion

    #region Fire

    private Vector3 GetAimTargetPoint()
    {
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        return Physics.Raycast(ray, out RaycastHit hit, 100f) ? hit.point : ray.GetPoint(100f);
    }
    public void TryFire()
    {
        if (gunData == null) return;

        if (gunState != GunState.Ready) return;

        if (currentTimer < gunData.fireDelay) return;

        if (currentAmmo <= 0)
        {
            Reload();
            return;
        }

        currentTimer = 0;
        currentAmmo--;

        SoundSystem.EmitSound(transform.position, 20f);

        if (gunAnimator != null) gunAnimator.SetTrigger("Fire");

        CasingRelease();
        PlayMuzzleFlash();

        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlaySFX(gunData.fireSoundName);
        }

        UpdateAmmoUI();
        ProcessShooting();

        if (ScoreManager.instance != null)
        {
            ScoreManager.instance.AddShotFired();
        }
    }
    public void SetTriggerPressed(bool isPressed)
    {
        isTriggerHeld = isPressed;

        if (isPressed && gunData.fireMode != GunFireMode.FullAuto)
        {
            TryFire();
        }
    }
    private void CheckContinuousFire()
    {
        if (isTriggerHeld && gunData.fireMode == GunFireMode.FullAuto)
        {
            TryFire();
        }
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
                FireBulletFromPool(spreadDir);
            }
        }
        else
        {
            FireBulletFromPool(baseDirection);
        }
    }
    private Vector3 GetSpreadDirection(Vector3 baseDir, float angle)
    {
        float x = Random.Range(-angle, angle);
        float y = Random.Range(-angle, angle);

        Quaternion spreadRot = Quaternion.LookRotation(baseDir) * Quaternion.Euler(x, y, 0);
        return spreadRot * Vector3.forward;
    }

    #endregion

    #region Reload

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

    #endregion

    #region Ammo

    public void AddAmmo(int amount)
    {
        totalAmmo += amount;
        UpdateAmmoUI();
    }
    private void UpdateAmmoUI()
    {
        if (gameObject.activeInHierarchy && UIManager.instance != null)
        {
            UIManager.instance.UpdateAmmoText(currentAmmo, totalAmmo);
        }
    }

    #endregion

    #region Casing
    void CasingRelease()
    {
        if (CasingManager.Instance == null)
        {
            Debug.LogWarning("CasingManager가 씬에 존재하지 않습니다!");
            return;
        }

        GameObject tempCasing = CasingManager.Instance.GetCasing(casingType);

        if (tempCasing != null)
        {
            tempCasing.transform.position = casingExitLocation.position;
            tempCasing.transform.rotation = casingExitLocation.rotation;

            if (tempCasing.TryGetComponent(out Rigidbody rb))
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;


                rb.AddExplosionForce(ExitPower, casingExitLocation.position - casingExitLocation.right * 0.3f, 1f);
            }
        }
    }
    private void PlayMuzzleFlash()
    {
        if (_flashPool == null || _flashPool.Count == 0)
        {
            return;
        }

        ParticleSystem currentPS = _flashPool[_currentFlashIndex];

        if (currentPS != null)
        {
            currentPS.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            currentPS.Play();
        }

        _currentFlashIndex = (_currentFlashIndex + 1) % _flashPool.Count;
    }
    #endregion
}