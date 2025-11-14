using UnityEngine;
using UnityEditor;

// GridMapDesigner용 커스텀 인스펙터
[CustomEditor(typeof(GridMapDesigner))]
public class GridMapDesignerEditor : Editor
{
    bool isDragging = false;    // 드래그 상태인지
    bool dragErase = false;     // 드래그 중 지우개 모드인지
    int lastGx = int.MinValue;  // 마지막으로 처리한 그리드 좌표
    int lastGz = int.MinValue;
    public override void OnInspectorGUI()
    {
        // 기본 인스펙터 그리기
        base.OnInspectorGUI();

        // target은 현재 선택된 GridMapDesigner
        GridMapDesigner designer = (GridMapDesigner)target;

        GameObject current = designer.CurrentPrefab;
        string prefabName = current != null ? current.name : "없음";
        EditorGUILayout.HelpBox($"현재 선택된 프리팹: {prefabName}", MessageType.Info);

        EditorGUILayout.Space();

        if(designer.prefabs!=null && designer.prefabs.Length>0)
        {
            EditorGUILayout.LabelField("프리팹 선택:", EditorStyles.boldLabel);

            for(int i=0;i<designer.prefabs.Length; i++)
            {
                GameObject prefab = designer.prefabs[i];
                string name = prefab != null ? prefab.name : "빈 슬롯";

                // 현재 선택된 인덱스면 앞에 표시
                string label = (i == designer.selectedIndex) ? $"▶ [{i}] {name}" : $"   [{i}] {name}";

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(label);

                if(GUILayout.Button("선택", GUILayout.Width(50)))
                {
                    designer.selectedIndex = i;
                    // 인스펙터 갱신
                    EditorUtility.SetDirty(designer);
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("이 좌표에 프리팹 배치하기"))
        {
            designer.PlacePrefabOnGrid();
        }
    }

    private void OnSceneGUI()
    {
        GridMapDesigner designer = (GridMapDesigner)target;
        if(designer.grid == null || designer.CurrentPrefab == null)
            return;
        Event e = Event.current;

        // Alt 키가 눌린 상태에서는 처리하지 않음 (카메라 회전을 위해)
        if (e.alt) return;

        // 마우스 위치 -> 그리드 좌표 구하는 함수
        bool TryGetGridPos(GridMapDesigner d, Vector2 mousePos, out int gx, out int gz)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(mousePos);
            Plane plane = new Plane(Vector3.up, d.grid.transform.position);
            if (plane.Raycast(ray, out float enter))
            {
                Vector3 hitPos = ray.GetPoint(enter);
                return d.grid.WorldToGrid(hitPos, out gx, out gz);
            }

            gx = gz = 0;
            return false;
        }

        // 1) 마우스 버튼 눌렸을 때 (드래그 시작)
        if(e.type == EventType.MouseDown && e.button == 0)
        {
            if(TryGetGridPos(designer, e.mousePosition, out int gx, out int gz))
            {
                dragErase = e.control || e.command;
                isDragging = true;
                lastGx = int.MinValue;
                lastGz = int.MinValue;

                PaintAt(designer, gx, gz, dragErase);
                lastGx = gx;
                lastGz = gz;

                e.Use();
            }
        }
        else if(e.type == EventType.MouseDrag && e.button ==0&&isDragging)
        {
            if(TryGetGridPos(designer, e.mousePosition, out int gx, out int gz))
            {
                if(gx != lastGx || gz != lastGz)
                {
                    PaintAt(designer, gx, gz, dragErase);
                    lastGx = gx;
                    lastGz = gz;
                }
                e.Use();
            }
        }
        else if(e.type == EventType.MouseUp && e.button ==0 && isDragging)
        {
            isDragging = false;
            lastGx = lastGz = int.MinValue;
            e.Use();
        }
    }

    private void PaintAt(GridMapDesigner designer, int gx, int gz, bool erase)
    {
        if(erase)
            designer.EraseAt(gx, gz);
        else
            designer.PlacePrefabOnGridAt(gx, gz);
    }
}
