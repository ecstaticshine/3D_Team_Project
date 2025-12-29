using UnityEngine;

public class ShadowCasterChecker : MonoBehaviour
{
    [ContextMenu("Check Shadow Casters")] // 유니티 인스펙터에서 컴포넌트 우클릭으로 실행하는 기능입니다.
    public void CheckShadows()
    {
        // 1. MeshRenderer: 물체의 '그림자 켜짐/꺼짐' 상태를 확인하기 위해 가져옵니다.
        MeshRenderer[] renderers = FindObjectsOfType<MeshRenderer>();
        int shadowOnCount = 0; // 그림자가 켜진 물체의 총 개수를 셀 변수입니다.

        foreach (var r in renderers)
        {
            // 2. shadowCastingMode: 그림자를 생성(On)하고 있는지 체크합니다.
            if (r.shadowCastingMode != UnityEngine.Rendering.ShadowCastingMode.Off)
            {
                shadowOnCount++;

                // 3. GetComponent<MeshFilter>(): 실제 폴리곤 데이터인 Mesh를 가져오기 위해 필터 컴포넌트를 찾습니다.
                MeshFilter mf = r.GetComponent<MeshFilter>();

                // 4. mf.sharedMesh: 해당 물체가 가진 메쉬 데이터에 접근합니다.
                if (mf != null && mf.sharedMesh != null)
                {
                    // 5. triangles.Length / 3: 삼각형 정점 3개가 폴리곤 1개이므로 3으로 나눠 폴리곤 수를 계산합니다.
                    int triCount = mf.sharedMesh.triangles.Length / 3;

                    // 6. 폴리곤이 5,000개가 넘는데 실시간 그림자까지 켜져 있으면 로그로 알려줍니다.
                    if (triCount > 5000)
                    {
                        Debug.LogWarning($"[범인 검거] {r.gameObject.name} / 폴리곤: {triCount}개 / 실시간 그림자 연산 중!");
                    }
                }
            }
        }
        // 7. 결과 보고: 전체 그림자 생성 물체 개수를 출력합니다. 아까 349개였던 수치를 여기서 확인하세요.
        Debug.Log($"--- 검사 완료! 현재 그림자를 생성 중인 물체 총합: {shadowOnCount}개 ---");
    }
}