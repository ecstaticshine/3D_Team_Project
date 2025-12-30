using UnityEngine;
using UnityEditor;

public class LightmapBakeDisabler : MonoBehaviour
{
    // [변수 설명]
    // 이 스크립트는 씬의 모든 물체를 전수 조사하여 
    // 오클루전(성능 향상)은 남기고 라이트맵(메모리 폭주)만 비활성화합니다.

    [ContextMenu("Disable All Lightmap Baking")] // 인스펙터 우클릭으로 실행
    public void DisableBaking()
    {
        // 1. 씬에 존재하는 모든 MeshRenderer를 가져옵니다.
        MeshRenderer[] renderers = FindObjectsOfType<MeshRenderer>();
        int count = 0;

        foreach (var r in renderers)
        {
            GameObject obj = r.gameObject;

            // 2. 현재 오브젝트의 Static 플래그를 가져옵니다.
            StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(obj);

            // 3. ContributeGI(라이트맵 베이킹 참여) 플래그만 제거합니다.
            // 초보자 참고: '&= ~' 연산은 특정 옵션만 끄는 명령어입니다.
            flags &= ~StaticEditorFlags.ContributeGI;

            // 4. 변경된 플래그를 다시 적용합니다.
            GameObjectUtility.SetStaticEditorFlags(obj, flags);
            count++;
        }

        Debug.Log($"[작업 완료] {count}개 오브젝트의 라이트맵 연산을 해제했습니다. 이제 베이킹 없이 실시간 조명으로 구동됩니다.");
    }
}