using UnityEngine;
using UnityEditor;

// GridMapDesigner용 커스텀 인스펙터
[CustomEditor(typeof(GridTileDesigner))]
public class GridTileDesignerEditor : Editor
{
    bool isDragging = false;    // 드래그 상태인지
    int lastGx = int.MinValue;  // 마지막으로 처리한 그리드 좌표
    int lastGz = int.MinValue;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GridTileDesigner designer = (GridTileDesigner)target;

        // 현재 선택된 타일 머티리얼 표시
        TilePaintPreset current = designer.CurrentPreset;
        string displayName = "없음";

        if(current!=null)
        {
            string matName = current.material ? current.material.name : "머티리얼 없음";
            displayName = $"{current.name}  ({current.tileType}, {matName})";
        }

        EditorGUILayout.HelpBox($"현재 선택된 브러시: {displayName}", MessageType.Info);

        if(designer.presets !=null && designer.presets.Length>0)
        {
            EditorGUILayout.LabelField("브러시 프리셋 선택", EditorStyles.boldLabel);

            for(int i=0;i<designer.presets.Length; i++)
            {
                TilePaintPreset p = designer.presets[i];
                string n = p != null ? (string.IsNullOrEmpty(p.name) ? $"Preset {i}" : p.name) : "빈 프리셋";
                string label = (i == designer.selectedIndex) ? $"▶ [{i}] {n}" : $"   [{i}] {n}";

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(label);

                if(GUILayout.Button("선택", GUILayout.Width(50)))
                {
                    designer.selectedIndex = i;
                    EditorUtility.SetDirty(designer);
                }

                EditorGUILayout.EndHorizontal();
            }
        }
    }

    // 씬 뷰용 유틸: 마우스 위치 → 그리드 좌표
    private bool TryGetGridPos(GridTileDesigner designer, Vector2 mousePos, out int gx, out int gz)
    {
        gx = gz = 0;
        if (designer.grid == null) return false;

        Ray ray = HandleUtility.GUIPointToWorldRay(mousePos);
        Plane plane = new Plane(Vector3.up, designer.grid.transform.position);

        if (plane.Raycast(ray, out float enter))
        {
            Vector3 hitPos = ray.GetPoint(enter);
            return designer.grid.WorldToGrid(hitPos, out gx, out gz);
        }

        return false;
    }

    private void OnSceneGUI()
    {
        GridTileDesigner designer = (GridTileDesigner)target;
        if (designer.grid == null || designer.presets == null)
            return;

        Event e = Event.current;
        if (e.alt) return; // 카메라 회전 중이면 무시

        // 마우스 다운 = 드래그 시작 + 첫 칸 칠하기
        if (e.type == EventType.MouseDown && e.button == 0)
        {
            if (TryGetGridPos(designer, e.mousePosition, out int gx, out int gz))
            {
                isDragging = true;
                lastGx = int.MinValue;
                lastGz = int.MinValue;

                Paint(designer, gx, gz);
                lastGx = gx;
                lastGz = gz;

                e.Use();
            }
        }
        // 드래그 중 = 마우스가 다른 칸으로 이동할 때마다 칠하기
        else if (e.type == EventType.MouseDrag && e.button == 0 && isDragging)
        {
            if (TryGetGridPos(designer, e.mousePosition, out int gx, out int gz))
            {
                if (gx != lastGx || gz != lastGz)
                {
                    Paint(designer, gx, gz);
                    lastGx = gx;
                    lastGz = gz;
                }

                e.Use();
            }
        }
        // 마우스 업 = 드래그 종료
        else if (e.type == EventType.MouseUp && e.button == 0 && isDragging)
        {
            isDragging = false;
            lastGx = lastGz = int.MinValue;
            e.Use();
        }
    }

    private void Paint(GridTileDesigner designer, int gx, int gz)
    {
        designer.PaintTile(gx, gz);
    }
}
