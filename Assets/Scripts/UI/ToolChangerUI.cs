using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ToolChangerUI : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private RectTransform _wheelRoot;   // 휠 전체 패널 (비활성/활성)
    [SerializeField] private PlayerAction _playerAction; // 도구 장비할 대상
    [SerializeField] private Camera _uiCamera;           // Screen Space - Camera면 지정, Overlay면 비워둬도 됨

    [Header("도구 슬롯")]
    [Tooltip("6개 도구 데이터 (도끼/곡괭이/괭이 등)")]
    [SerializeField] private ToolData[] _tools = new ToolData[6];

    [Tooltip("각 슬롯 아이콘 이미지 (UI Image)")]
    [SerializeField] private Image[] _slotImages = new Image[6];

    [Header("비주얼")]
    [SerializeField] private Color _normalColor = Color.white;
    [SerializeField] private Color _highlightColor = Color.yellow;
    [SerializeField] private float _centerDeadZone = 30f; // 휠 중앙 근처는 선택 없음

    private bool _isOpen = false;
    private int _currentIndex = -1;

    private void Awake()
    {
        if (_wheelRoot != null)
            _wheelRoot.gameObject.SetActive(false);

        // 아이콘 초기화
        RefreshIcons();
        HighlightSlot(-1);
    }

    private void Update()
    {
        if (!_isOpen) return;

        UpdateSelectionByMouse();
    }

    /// <summary>
    /// PlayerInput → Tab(툴휠) 액션에 연결해서 사용.
    /// started -> 열기, canceled -> 선택 확정 후 닫기
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
        if (_wheelRoot == null) return;

        _isOpen = true;
        _currentIndex = -1;
        _wheelRoot.gameObject.SetActive(true);

        RefreshIcons();
        HighlightSlot(-1);

        // 필요하면: 플레이어 이동 입력 잠시 무시하는 플래그 세팅도 가능
    }

    private void CloseWheel()
    {
        if (_wheelRoot == null) return;

        _isOpen = false;
        _wheelRoot.gameObject.SetActive(false);
        HighlightSlot(-1);
    }

    private void ConfirmSelection()
    {
        if (_currentIndex < 0 || _currentIndex >= _tools.Length)
            return;

        ToolData selected = _tools[_currentIndex];
        if (selected == null || _playerAction == null)
            return;

        //_playerAction.EquipTool(selected);
        Debug.Log($"[ToolWheel] 선택된 도구: {selected.DisplayName} ({selected.ToolType})");
    }

    private void RefreshIcons()
    {
        if (_slotImages == null) return;

        for (int i = 0; i < _slotImages.Length; i++)
        {
            if (_slotImages[i] == null) continue;

            Sprite icon = null;
            if (_tools != null && i < _tools.Length && _tools[i] != null)
            {
                icon = _tools[i].Icon;
            }

            _slotImages[i].sprite = icon;
            _slotImages[i].enabled = (icon != null);
        }
    }

    private void UpdateSelectionByMouse()
    {
        if (_wheelRoot == null || _slotImages == null || _slotImages.Length == 0)
            return;

        Vector2 mousePos = Mouse.current.position.ReadValue();

        // 휠 중심 (스크린 좌표)
        Vector2 center = RectTransformUtility.WorldToScreenPoint(
            _uiCamera ? _uiCamera : null,
            _wheelRoot.position
        );

        Vector2 dir = mousePos - center;
        float sqrMag = dir.sqrMagnitude;

        // 중앙 근처면 선택 해제
        if (sqrMag < _centerDeadZone * _centerDeadZone)
        {
            HighlightSlot(-1);
            return;
        }

        // 방향 → 각도(도) 0~360
        float angleDeg = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (angleDeg < 0f) angleDeg += 360f;

        // 6분할 (각 60도) → 0~5 인덱스
        // 중앙을 슬롯 방향에 맞추기 위해 30도 오프셋
        int count = _tools.Length;
        float sectorSize = 360f / count;  // 60도
        int index = Mathf.FloorToInt((angleDeg + sectorSize / 2f) / sectorSize) % count;

        HighlightSlot(index);
    }

    private void HighlightSlot(int index)
    {
        _currentIndex = index;

        if (_slotImages == null) return;

        for (int i = 0; i < _slotImages.Length; i++)
        {
            if (_slotImages[i] == null) continue;

            Color c = (i == index) ? _highlightColor : _normalColor;
            _slotImages[i].color = c;
        }
    }
}
