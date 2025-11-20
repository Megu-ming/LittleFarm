using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ToolChangerUI : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] RectTransform _wheelRoot;   // UI_ToolChange의 RectTransform
    [SerializeField] Camera _uiCamera;           // Overlay면 비워둬도 됨

    [Tooltip("각 슬롯의 Image (Slot_0~Slot_5)")]
    [SerializeField] Image[] _slotImages = new Image[6];

    [Header("비주얼")]
    [SerializeField] float _normalScale = 1f;
    [SerializeField] float _highlightScale = 1.5f;
    [SerializeField] float _centerDeadZone = 30f; // 중앙 원(픽셀) 안은 선택 없음

    private Canvas _canvas;
    Player _player;
    private bool _isOpen = false;
    private int _currentIndex = -1;

    private void Awake()
    {
        _canvas = GetComponentInParent<Canvas>();
        if (_wheelRoot != null)
            _wheelRoot.gameObject.SetActive(false);
    }

    public void Initialize(Player player)
    {
        _player = player;

        RefreshIcons();
        HighlightSlot(-1);
    }

    private void Update()
    {
        if (_isOpen)
        {
            UpdateSelectionByMouse();
        }
    }

    /// <summary>
    /// PlayerInput → Tab 액션에 연결해서 사용.
    /// started : 휠 열기
    /// canceled : 선택 확정 후 닫기
    /// </summary>
    public void OnToolWheel(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            OpenWheel();
        }
        else if (context.canceled)
        {
            ConfirmSelection();
            CloseWheel();
        }
    }

    private void OpenWheel()
    {
        if (_wheelRoot == null)
            return;

        _isOpen = true;
        _wheelRoot.gameObject.SetActive(true);

        RefreshIcons();
    }

    private void CloseWheel()
    {
        if (_wheelRoot == null)
            return;

        _isOpen = false;
        _wheelRoot.gameObject.SetActive(false);
        HighlightSlot(-1);
    }

    private void ConfirmSelection()
    {
        if (_currentIndex < 0 || _player ==null || _currentIndex >= _player.Tools.Length)
            return;

        ToolData selected = _player.Tools[_currentIndex];
        if (selected == null)
            return;

        _player.EquipTool(selected);
        Debug.Log($"[ToolChangerUI] 선택된 도구: {selected.DisplayName} ({selected.ToolType})");
    }

    private void RefreshIcons()
    {
        if (_slotImages == null)
            return;

        for (int i = 0; i < _slotImages.Length; i++)
        {
            if (_slotImages[i] == null)
                continue;

            Sprite icon = null;
            if (_player.Tools != null && i < _player.Tools.Length && _player.Tools[i] != null)
            {
                icon = _player.Tools[i].Icon;
            }

            _slotImages[i].sprite = icon;
            _slotImages[i].enabled = (icon != null);
        }
    }

    /// <summary>
    /// 마우스 방향을 보고 현재 선택 슬롯 인덱스를 계산
    /// 0 = 위, 1 = 우상단, 2 = 우하단, 3 = 아래, 4 = 좌하단, 5 = 좌상단
    /// </summary>
    private void UpdateSelectionByMouse()
    {
        if (_wheelRoot == null || _slotImages == null || _slotImages.Length == 0)
            return;
        if (Mouse.current == null)
            return;
        if (_canvas == null)
            return;

        // 1) 마우스 스크린 좌표
        Vector2 mousePos = Mouse.current.position.ReadValue();

        // 2) 캔버스 로컬좌표로 변환
        var canvasRect = _canvas.transform as RectTransform;
        Camera cam = _canvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : _canvas.worldCamera;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            mousePos,
            cam,
            out Vector2 localMousePos
        );

        // 3) 휠 중심도 같은 좌표계(캔버스 로컬)에서 사용
        Vector2 center = _wheelRoot.anchoredPosition;   // 도구변경 UI의 anchoredPosition

        Vector2 dir = localMousePos - center;
        float sqrMag = dir.sqrMagnitude;

        // 중앙 근처면 선택 해제
        if (sqrMag < _centerDeadZone * _centerDeadZone)
        {
            HighlightSlot(-1);
            return;
        }

        // 4) 각도 계산
        float angleDeg = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (angleDeg < 0f)
            angleDeg += 360f;

        // "위에서 시작, 시계 방향" 각도로 변환
        float cwFromTop = 90f - angleDeg;
        if (cwFromTop < 0f)
            cwFromTop += 360f;

        int count = _player.Tools.Length;   // 6
        float sectorSize = 360f / count;    // 60도

        int index = Mathf.FloorToInt((cwFromTop + sectorSize * 0.5f) / sectorSize) % count;

        HighlightSlot(index);
    }

    private void HighlightSlot(int index)
    {
        _currentIndex = index;

        if (_slotImages == null)
            return;

        for (int i = 0; i < _slotImages.Length; i++)
        {
            if (_slotImages[i] == null)
                continue;

            var rect = _slotImages[i].rectTransform;
            float targetScale = (i == index) ? _highlightScale : _normalScale;
            rect.localScale = Vector3.one * targetScale;
        }
    }
}
