using UnityEngine;
using UnityEditor; // 에디터 기능을 사용하기 위한 라이브러리

public class MapMeshCombiner : EditorWindow
{
    // 1. 유니티 상단 메뉴바에 'Tools > Mesh Combiner' 메뉴를 생성합니다.
    [MenuItem("Tools/Mesh Combiner")]
    public static void ShowWindow()
    {
        // 클릭 시 이 창을 띄웁니다.
        GetWindow<MapMeshCombiner>("Mesh Combiner");
    }

    private GameObject targetRoot;

    // 2. 에디터 윈도우 UI를 그리는 부분입니다.
    void OnGUI()
    {
        GUILayout.Label("배경 메쉬 통합 도구", EditorStyles.boldLabel);

        // 합칠 대상인 부모 오브젝트를 슬롯에 넣을 수 있게 합니다.
        targetRoot = (GameObject)EditorGUILayout.ObjectField("대상 Root 오브젝트", targetRoot, typeof(GameObject), true);

        if (GUILayout.Button("메쉬 통합 실행 (Combine)"))
        {
            if (targetRoot != null) CombineStaticMeshes();
            else EditorUtility.DisplayDialog("경고", "대상 오브젝트를 할당해주세요!", "확인");
        }
    }

    private void CombineStaticMeshes()
    {
        // 3. 대상의 자식들로부터 모든 메쉬 필터를 가져옵니다.
        MeshFilter[] meshFilters = targetRoot.GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];

        for (int i = 0; i < meshFilters.Length; i++)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            // 월드 좌표 행렬을 계산하여 합쳐진 후에도 위치를 유지합니다.
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            // 원본은 비활성화합니다.
            meshFilters[i].gameObject.SetActive(false);
        }

        // 4. 새로운 통합 메쉬 오브젝트 생성
        GameObject combinedObj = new GameObject("Combined_Map_Result");
        MeshFilter mf = combinedObj.AddComponent<MeshFilter>();
        MeshRenderer mr = combinedObj.AddComponent<MeshRenderer>();

        mf.mesh = new Mesh();
        // 메모리 제한을 넘지 않도록 32비트 인덱스를 사용합니다. (오브젝트가 많을 때 필수)
        mf.mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mf.mesh.CombineMeshes(combine, true);

        if (meshFilters.Length > 0)
            mr.sharedMaterial = meshFilters[0].GetComponent<MeshRenderer>().sharedMaterial;

        // 5. 생성된 오브젝트를 선택 상태로 만들어 바로 확인할 수 있게 합니다.
        Selection.activeGameObject = combinedObj;
        Debug.Log($"{meshFilters.Length}개의 메쉬가 하나로 통합되었습니다.");
    }
}