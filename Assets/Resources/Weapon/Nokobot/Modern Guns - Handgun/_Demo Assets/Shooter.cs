using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;



public class Shooter : MonoBehaviour
{
    [Header("Prefab Refrences")]
    public GameObject bulletPrefab;
    public GameObject casingPrefab;
    public GameObject muzzleFlashPrefab;

    [Header("Location Refrences")]
    //[SerializeField] private Animator gunAnimator;
    [SerializeField] private Transform barrelLocation;
    [SerializeField] private Transform casingExitLocation;

    [Header("Settings")]
    [Tooltip("Specify time to destory the casing object")] [SerializeField] private float destroyTimer = 2f;
    [Tooltip("Bullet Speed")] [SerializeField] private float shotPower = 500f;
    [Tooltip("Casing Ejection Speed")] [SerializeField] private float ejectPower = 150f;
    [SerializeField] float CasingExitforce = 1f;


    void Start()
    {
        if (barrelLocation == null)
            barrelLocation = transform;

        //if (gunAnimator == null)
           // gunAnimator = GetComponentInChildren<Animator>();
    }

    void OnFire(InputValue value)
    {
        // value.isPressed는 버튼을 누르고 있을 때 true, 떼면 false가 됩니다.
        // 여기서는 단발 사격을 위해 버튼을 '누른 순간'에만 작동하도록 작성합니다.
        if (value.isPressed)
        {
            // 애니메이터의 "Fire" 트리거를 작동시켜 총 쏘는 동작을 시작합니다.
            // 실제 발사(Shoot)는 애니메이션 도중에 이벤트로 발생합니다.
            //gunAnimator.SetTrigger("Fire");
            Shoot();
            CasingRelease();
        }
    }

    // [중요] 이 함수는 애니메이션의 특정 프레임(총구 화염이 보일 때)에서 호출되도록 설계되었습니다.
    void Shoot()
    {
        if (muzzleFlashPrefab)
        {
            // 총구 화염을 임시 변수 tempFlash에 생성하여 저장합니다.
            GameObject tempFlash;
            tempFlash = Instantiate(muzzleFlashPrefab, barrelLocation.position, barrelLocation.rotation);

            // 생성된 총구 화염을 destroyTimer 시간이 지난 후에 삭제합니다.
            Destroy(tempFlash, destroyTimer);
        }

        // 총알 프리팹이 연결되어 있지 않다면 아래 코드를 실행하지 않고 함수를 종료합니다.
        //if (!bulletPrefab)
        //{ return; }

        // 총알을 생성하고, Rigidbody 컴포넌트를 가져와 총구 앞방향(forward)으로 힘(AddForce)을 줍니다.
        //Instantiate(bulletPrefab, barrelLocation.position, barrelLocation.rotation).GetComponent<Rigidbody>().AddForce(barrelLocation.forward * shotPower);
    }

    // [중요] 이 함수는 애니메이션 도중 탄피가 튀어나와야 하는 프레임에서 호출됩니다.
    void CasingRelease()
    {
        // 탄피 배출구 위치나 탄피 프리팹이 없다면 함수를 종료합니다.
        if (!casingExitLocation || !casingPrefab)
        { return; }

        // 탄피를 임시 변수 tempCasing에 생성합니다.
        GameObject tempCasing;
        tempCasing = Instantiate(casingPrefab, casingExitLocation.position, casingExitLocation.rotation) as GameObject;

        // 탄피에 폭발적인 힘(AddExplosionForce)을 주어 옆으로 튀어 나가게 만듭니다.
        tempCasing.GetComponent<Rigidbody>().AddExplosionForce(Random.Range(ejectPower * 0.7f, ejectPower), (casingExitLocation.position - casingExitLocation.right * 0.3f - casingExitLocation.up * CasingExitforce), 10f);

        // 탄피가 공중에서 무작위로 회전하도록 회전력(AddTorque)을 줍니다.
        tempCasing.GetComponent<Rigidbody>().AddTorque(new Vector3(0, Random.Range(100f, 500f), Random.Range(100f, 1000f)), ForceMode.Impulse);

        // 생성된 탄피를 destroyTimer 시간이 지난 후에 삭제합니다.
        Destroy(tempCasing, destroyTimer);
    }

}
