using UnityEngine;
using System.Linq;

public class PolygonChecker : MonoBehaviour
{
    // [기능 설명]
    // 씬 안의 모든 메쉬를 검사하여 폴리곤 수가 높은 순서대로 출력합니다.

    [ContextMenu("Find Heavy Objects")] // 컴포넌트 우클릭 후 이 메뉴를 누르세요.
    public void FindHeavyObjects()
    {
        // 1. 씬의 모든 MeshFilter(형태 데이터)를 가져옵니다.
        MeshFilter[] filters = FindObjectsOfType<MeshFilter>();

        // 2. 폴리곤(Triangles) 수 기준으로 내림차순 정렬하여 상위 10개를 뽑습니다.
        var heavyObjects = filters
            .OrderByDescending(f => f.sharedMesh != null ? f.sharedMesh.triangles.Length / 3 : 0)
            .Take(10);

        Debug.Log("--- [폴리곤 상위 10개 리스트] ---");

        foreach (var f in heavyObjects)
        {
            if (f.sharedMesh != null)
            {
                int triCount = f.sharedMesh.triangles.Length / 3;
                // 3. 오브젝트 이름과 폴리곤 수를 출력합니다.
                Debug.Log($"{f.gameObject.name}: {triCount} Triangles");
            }
        }
    }
}