using UnityEngine;

public class S_Gun : MonoBehaviour
{
    [Header("전투 설정")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 50f;
    public float fireDelay = 1f;
    private float currentTimer;
    private float magazineCapacity = 60f;
    private float currentMagazine = 15f;

    [Header("조준 설정")]
    public Transform cameraTransform;

    private void Start()
    {
        currentTimer = fireDelay;
    }

    private void Update()
    {
        currentTimer += Time.unscaledDeltaTime;
    }

    public void Fire()
    {
        if (bulletPrefab == null || firePoint == null || cameraTransform == null || currentTimer < fireDelay) return;

        currentTimer = 0;

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