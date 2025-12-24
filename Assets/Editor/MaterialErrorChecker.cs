using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

// 이 스크립트는 에디터가 실행될 때 자동으로 검사를 수행합니다.
[InitializeOnLoad]
public class MaterialErrorChecker
{
    // 클래스가 메모리에 올라갈 때(유니티 켤 때) 실행되는 생성자입니다.
    static MaterialErrorChecker()
    {
        // 유니티가 시작된 후 첫 번째 업데이트 때 검사를 실행하도록 예약합니다.
        EditorApplication.delayCall += CheckMaterials;
    }

    private static void CheckMaterials()
    {
        // 1. 프로젝트 내의 모든 머티리얼 에셋의 GUID(고유 식별자)를 찾습니다.
        // "t:Material"은 머티리얼 타입만 골라내라는 필터링 명령어입니다.
        string[] guids = AssetDatabase.FindAssets("t:Material");

        // 발견된 머티리얼 개수를 저장할 변수입니다.
        int totalMaterials = guids.Length;
        // 문제가 발견된 머티리얼 개수를 세기 위한 변수입니다.
        int issueCount = 0;

        Debug.Log($"[검사 시작] 총 {totalMaterials}개의 머티리얼을 검사합니다.");

        // 2. 찾은 모든 머티리얼을 하나씩 조사합니다.
        foreach (string guid in guids)
        {
            // GUID를 사용하여 실제 파일이 있는 경로(Path)를 알아냅니다.
            string path = AssetDatabase.GUIDToAssetPath(guid);

            // 경로에 있는 파일을 머티리얼 객체로 로드하여 'mat' 변수에 담습니다.
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);

            // 3. 머티리얼 자체가 없거나(Null), 연결된 셰이더가 없는 경우를 체크합니다.
            if (mat == null || mat.shader == null)
            {
                // 문제가 발견되면 콘솔에 빨간색 에러 로그를 남깁니다.
                Debug.LogError($"[문제 발견] 머티리얼이 깨졌거나 셰이더가 없습니다: {path}");
                issueCount++;
                continue; // 다음 머티리얼로 넘어갑니다.
            }

            // 4. URP를 사용 중인데 셰이더 이름에 'Internal'이나 'Hidden'이 들어간 잘못된 참조가 있는지 확인합니다.
            // (보통 NullReferenceException의 숨은 원인이 됩니다.)
            if (mat.shader.name.Contains("InternalErrorShader"))
            {
                Debug.LogError($"[셰이더 에러] 잘못된 셰이더를 참조 중인 머티리얼: {path}");
                issueCount++;
            }
        }

        // 모든 검사가 끝나면 최종 결과를 보고합니다.
        Debug.Log($"[검사 완료] 총 {issueCount}개의 문제가 있는 머티리얼을 찾았습니다.");
    }
}