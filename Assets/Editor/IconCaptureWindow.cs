#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

public class IconCaptureWindow : EditorWindow
{
    [Header("아이콘 캡쳐 설정")]
    public Camera iconCamera;                 // IconCamera
    public RenderTexture renderTexture;       // RT_ItemIcon
    public GameObject prefabToCapture;        // 캡쳐할 프리팹

    public int iconSize = 256;
    public string saveFolder = "Assets/Arts/ItemIcons"; // PNG 저장 폴더

    public Vector3 modelEuler = Vector3.zero;

    GameObject _tempInstance;

    const float PreviewSize = 150f;

    [MenuItem("Tools/Item Icon Capture")]
    public static void ShowWindow()
    {
        GetWindow<IconCaptureWindow>("Item Icon Capture");
    }

    void OnGUI()
    {
        GUILayout.Label("아이템 아이콘 캡쳐", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();

        iconCamera = (Camera)EditorGUILayout.ObjectField("Icon Camera", iconCamera, typeof(Camera), true);
        renderTexture = (RenderTexture)EditorGUILayout.ObjectField("RenderTexture", renderTexture, typeof(RenderTexture), false);
        prefabToCapture = (GameObject)EditorGUILayout.ObjectField("Prefab", prefabToCapture, typeof(GameObject), false);

        iconSize = EditorGUILayout.IntField("Icon Size", iconSize);
        saveFolder = EditorGUILayout.TextField("Save Folder", saveFolder);

        modelEuler = EditorGUILayout.Vector3Field("Model Rotation (Euler)", modelEuler);

        bool changedByFields = EditorGUI.EndChangeCheck();

        if (GUILayout.Button("선택된 Prefab 자동 할당"))
        {
            var obj = Selection.activeObject as GameObject;
            if (obj != null)
            {
                prefabToCapture = obj;
            }
        }

        if (changedByFields)
        {
            if (iconCamera != null && renderTexture != null && prefabToCapture != null)
            {
                RenderToRenderTexture();
                Repaint(); // 창 다시 그리기
            }
        }

        GUILayout.Space(10);

        // ───── 미리보기 영역 ─────
        GUILayout.Label("미리보기", EditorStyles.boldLabel);

        Rect previewRect = GUILayoutUtility.GetRect(PreviewSize, PreviewSize, GUILayout.ExpandWidth(false));
        // 배경색
        EditorGUI.DrawRect(previewRect, new Color(0.15f, 0.15f, 0.15f, 1f));

        if (renderTexture != null)
        {
            // RT 내용을 툴에 그대로 그리기
            EditorGUI.DrawPreviewTexture(previewRect, renderTexture, null, ScaleMode.ScaleToFit);
        }
        else
        {
            GUI.Label(previewRect, "RenderTexture 없음", EditorStyles.centeredGreyMiniLabel);
        }

        GUILayout.Space(10);

        // ───── 버튼들 ─────
        using (new GUILayout.HorizontalScope())
        {
            if (GUILayout.Button("미리보기 갱신"))
            {
                RenderToRenderTexture(); // RT만 새로 그리기
            }

            if (GUILayout.Button("아이콘 캡쳐하기"))
            {
                CaptureIcon(); // RT 렌더 + PNG 저장
            }
        }
    }

    /// <summary>
    /// 프리팹을 임시 인스턴스로 만들고, 카메라로 RenderTexture에 렌더만 함.
    /// (디스크 저장은 안 함)
    /// </summary>
    void RenderToRenderTexture()
    {
        if (iconCamera == null || renderTexture == null || prefabToCapture == null)
        {
            Debug.LogError("[IconCapture] Camera / RenderTexture / Prefab 중 빠진 게 있습니다.");
            return;
        }

        // 1) 임시 인스턴스 생성
        _tempInstance = (GameObject)PrefabUtility.InstantiatePrefab(prefabToCapture);
        _tempInstance.transform.position = Vector3.zero;
        _tempInstance.transform.rotation = Quaternion.Euler(modelEuler);
        _tempInstance.transform.localScale = Vector3.one;

        // 2) 모델 bounds 계산해서 카메라 위치/사이즈 자동 조절
        var renderers = _tempInstance.GetComponentsInChildren<Renderer>();
        if (renderers.Length > 0)
        {
            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
                bounds.Encapsulate(renderers[i].bounds);

            Vector3 center = bounds.center;
            float radius = bounds.extents.magnitude;

            // 카메라가 Z- 방향에서 바라본다고 가정 (원하면 IconCamera의 rotation을 바꿔도 됨)
            iconCamera.transform.position = center + new Vector3(0, 0, -radius * 2f);
            iconCamera.transform.LookAt(center);

            if (iconCamera.orthographic)
            {
                float maxExtent = Mathf.Max(bounds.extents.x, bounds.extents.y, bounds.extents.z);
                iconCamera.orthographicSize = maxExtent * 1.3f; // 여유 조금
            }
            else
            {
                float fov = iconCamera.fieldOfView;
                float dist = radius / Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad);
                iconCamera.transform.position = center - iconCamera.transform.forward * dist * 1.3f;
            }
        }

        // 3) 카메라 → RenderTexture로 렌더
        var prevRT = iconCamera.targetTexture;
        iconCamera.targetTexture = renderTexture;

        iconCamera.Render();

        iconCamera.targetTexture = prevRT;

        // 4) 임시 인스턴스 정리
        if (_tempInstance != null)
            DestroyImmediate(_tempInstance);
    }


    void CaptureIcon()
    {
        if (iconCamera == null || renderTexture == null || prefabToCapture == null)
        {
            Debug.LogError("[IconCapture] Camera / RenderTexture / Prefab 중 빠진 게 있습니다.");
            return;
        }

        // 1) 임시로 프리팹 인스턴스 생성
        _tempInstance = (GameObject)PrefabUtility.InstantiatePrefab(prefabToCapture);
        _tempInstance.transform.position = Vector3.zero;
        _tempInstance.transform.rotation = Quaternion.Euler(modelEuler);
        _tempInstance.transform.localScale = Vector3.one;

        // 필요하면 여기서 bounds 계산해서 카메라 위치를 살짝 조정해도 됨
        // 모든 Renderer bounds 합치기
        var renderers = _tempInstance.GetComponentsInChildren<Renderer>();
        if (renderers.Length > 0)
        {
            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
                bounds.Encapsulate(renderers[i].bounds);

            Vector3 center = bounds.center;
            float radius = bounds.extents.magnitude; // 대략적인 크기

            // 카메라가 정면에서 본다고 가정 (Z-로 보는 경우)
            // 카메라가 다른 방향을 보게 하고 싶으면 forward / up만 조정하면 됨
            iconCamera.transform.position = center + new Vector3(0, 0, -radius * 2f);
            iconCamera.transform.LookAt(center);

            if (iconCamera.orthographic)
            {
                // 오쏘 카메라일 때 화면에 꽉 차게
                float maxExtent = Mathf.Max(bounds.extents.x, bounds.extents.y, bounds.extents.z);
                iconCamera.orthographicSize = maxExtent * 1.3f; // 여유 30%
            }
            else
            {
                // 퍼스 카메라일 때 거리 계산 (필요하면)
                float fov = iconCamera.fieldOfView;
                float dist = radius / Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad);
                iconCamera.transform.position = center - iconCamera.transform.forward * dist * 1.3f;
            }
        }
        // 지금은 IconScene에서 카메라/프리팹 위치를 대충 맞춰두고 쓰는 방식

        // 2) 카메라에 RenderTexture 세팅
        var prevRT = iconCamera.targetTexture;
        iconCamera.targetTexture = renderTexture;

        // 3) 렌더링
        iconCamera.Render();

        // 4) RenderTexture → Texture2D로 읽기
        RenderTexture.active = renderTexture;
        Texture2D tex = new Texture2D(iconSize, iconSize, TextureFormat.RGBA32, false);
        tex.ReadPixels(new Rect(0, 0, iconSize, iconSize), 0, 0);
        tex.Apply();

        iconCamera.targetTexture = prevRT;
        RenderTexture.active = null;

        // 5) PNG로 저장
        if (!Directory.Exists(saveFolder))
        {
            Directory.CreateDirectory(saveFolder);
        }

        string fileName = prefabToCapture.name + "_icon.png";
        string path = Path.Combine(saveFolder, fileName);

        byte[] png = tex.EncodeToPNG();
        File.WriteAllBytes(path, png);
        Debug.Log($"[IconCapture] Saved icon: {path}");

        // 6) 에셋 DB 갱신 + Sprite로 임포트 설정
        AssetDatabase.ImportAsset(path);
        var importer = (TextureImporter)TextureImporter.GetAtPath(path);
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.SaveAndReimport();
        }

        // 임시 인스턴스 정리
        if (_tempInstance != null)
        {
            DestroyImmediate(_tempInstance);
        }

        // 메모리 정리
        Object.DestroyImmediate(tex);
    }
}
#endif
