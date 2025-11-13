using UnityEngine;
using UnityEditor;

// GridMapDesigner용 커스텀 인스펙터
[CustomEditor(typeof(GridMapDesigner))]
public class GridMapDesignerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 기본 인스펙터 그리기
        base.OnInspectorGUI();

        // target은 현재 선택된 GridMapDesigner
        GridMapDesigner designer = (GridMapDesigner)target;

        EditorGUILayout.Space();

        if (GUILayout.Button("이 좌표에 프리팹 배치하기"))
        {
            designer.PlacePrefabOnGrid();
        }
    }
}
