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
    public string saveFolder = "Assets/Resources/ItemIcons"; // PNG 저장 폴더

    public float orbitYaw = 45f;  // Y축 회전 (좌우)
    public float orbitPitch = 20f;  // X축 회전 (위/아래)
    public float distanceFactor = 2.0f; // 모델 반경 * distanceFactor 만큼 떨어진 곳에 카메라\
    public float orbitRoll = 0f;    // Z축 회전 (시계/반시계)

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

        orbitYaw = EditorGUILayout.Slider("Yaw (Y 회전)", orbitYaw, -180f, 180f);
        orbitPitch = EditorGUILayout.Slider("Pitch (X 회전)", orbitPitch, -80f, 80f);
        distanceFactor = EditorGUILayout.Slider("Distance Factor", distanceFactor, 1.0f, 4.0f);
        orbitRoll = EditorGUILayout.Slider("Roll (Z 회전)", orbitRoll, -180f, 180f);

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
        _tempInstance.transform.rotation = Quaternion.identity;
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
            if (radius < 0.001f) radius = 0.5f;

            // yaw/pitch로 "모델을 바라보는 방향" 계산
            Quaternion orbitRot = Quaternion.Euler(orbitPitch, orbitYaw, 0f);
            Vector3 dir = orbitRot * Vector3.forward; // 카메라 → 모델 방향

            float distance = radius * distanceFactor;

            // 기본 카메라 위치 & LookAt
            Vector3 camPos = center - dir * distance;
            iconCamera.transform.position = camPos;
            iconCamera.transform.LookAt(center); // 여기서 up은 기본 (0,1,0)

            // ★ 여기서 roll 추가: 카메라 자신의 forward 축 기준 회전
            iconCamera.transform.Rotate(Vector3.forward, orbitRoll, Space.Self);

            // 오쏘 사이즈 맞추기
            if (iconCamera.orthographic)
            {
                float maxExtent = Mathf.Max(bounds.extents.x, bounds.extents.y, bounds.extents.z);
                iconCamera.orthographicSize = maxExtent * 1.3f;
            }
            else
            {
                float fov = iconCamera.fieldOfView;
                float dist = radius / Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad);
                iconCamera.transform.position = center - iconCamera.transform.forward * dist * distanceFactor;
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

        // 🔹 미리보기와 완전히 같은 세팅으로 한 번 더 렌더
        RenderToRenderTexture();

        // 🔹 RenderTexture → Texture2D
        RenderTexture.active = renderTexture;
        Texture2D tex = new Texture2D(iconSize, iconSize, TextureFormat.RGBA32, false);
        tex.ReadPixels(new Rect(0, 0, iconSize, iconSize), 0, 0);
        tex.Apply();
        RenderTexture.active = null;

        // 🔹 PNG 저장
        if (!Directory.Exists(saveFolder))
        {
            Directory.CreateDirectory(saveFolder);
        }

        string fileName = "ICON_" + prefabToCapture.name + ".png";
        string path = Path.Combine(saveFolder, fileName);

        byte[] png = tex.EncodeToPNG();
        File.WriteAllBytes(path, png);
        Debug.Log($"[IconCapture] Saved icon: {path}");

        // 🔹 에셋 DB 갱신 + Sprite 임포트
        AssetDatabase.ImportAsset(path);
        var importer = (TextureImporter)TextureImporter.GetAtPath(path);
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.SaveAndReimport();
        }

        // 메모리 정리
        Object.DestroyImmediate(tex);
    }

}
#endif
