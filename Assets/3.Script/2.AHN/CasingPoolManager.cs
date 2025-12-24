using UnityEngine;
using UnityEngine.Pool; 
using System.Collections.Generic;

public enum CasingType { Pistol, Rifle, Shotgun }

public class CasingManager : MonoBehaviour
{
    public static CasingManager Instance;

    [System.Serializable]
    public struct CasingData
    {
        public CasingType type;      // 탄피 종류 (Enum)
        public GameObject prefab;    // 생성할 탄피 프리팹
        public int defaultSize;      // 처음에 미리 만들어둘 개수
    }

    [Header("탄피 설정 리스트")]
    public List<CasingData> casingSettings;

    private Dictionary<CasingType, IObjectPool<GameObject>> _pools = new Dictionary<CasingType, IObjectPool<GameObject>>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        foreach (var data in casingSettings)
        {
            if (data.prefab == null) continue;

            IObjectPool<GameObject> pool = new ObjectPool<GameObject>(
                createFunc: () => {
                    GameObject obj = Instantiate(data.prefab); // 프리팹 생성
                    obj.GetComponent<Casing>().Initialize(data.type); // 타입 정보 주입
                    return obj;
                },
                actionOnGet: (obj) => obj.SetActive(true), // 가져갈 때 활성화
                actionOnRelease: (obj) => obj.SetActive(false), // 반납할 때 비활성화
                actionOnDestroy: (obj) => Destroy(obj), // 풀 초과 시 삭제
                collectionCheck: true,
                defaultCapacity: data.defaultSize,
                maxSize: 100
            );

            _pools.Add(data.type, pool); // 사전에 등록

            for (int i = 0; i < data.defaultSize; i++)
            {
                GameObject temp = pool.Get();
                pool.Release(temp);
            }
        }
    }

    // 총기에서 호출하여 탄피를 빌려가는 함수
    public GameObject GetCasing(CasingType type)
    {
        if (_pools.TryGetValue(type, out IObjectPool<GameObject> pool)) return pool.Get();
        return null;
    }

    // 탄피가 스스로 반납할 때 호출하는 함수
    public void ReturnCasing(CasingType type, GameObject casing)
    {
        if (_pools.TryGetValue(type, out IObjectPool<GameObject> pool)) pool.Release(casing);
    }
}