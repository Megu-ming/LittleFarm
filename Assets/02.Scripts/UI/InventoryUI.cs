using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] Inventory _inventory;
    [SerializeField] Transform _slotParent;
    [SerializeField] InventorySlotUI _slotPrefab;

    [SerializeField] InventorySlotUI[] _slotUIs;

    ItemDatabase _database;

    bool _isOpen = false;

    [Header("드래그 아이콘")]
    [SerializeField] Vector2 _dragIconSize = new Vector2(120, 120);

    Canvas _rootCanvas;
    RectTransform _dragIconRT;
    Image _dragIconImage;

    int _dragFromIndex = -1;
    bool _isDragging = false;

    public void Initialize(Inventory inventory, ItemDatabase database)
    {
        _inventory = inventory;
        _database = database;

        CreateSlots();
    }

    private void CreateSlots()
    {
        if(_slotUIs != null)
        {
            for (int i = 0; i < _slotUIs.Length; i++)
                Destroy(_slotUIs[i].gameObject);
        }

        int count = _inventory.SlotCount;
        _slotUIs = new InventorySlotUI[count];

        for(int i=0;i<count;i++)
        {
            InventorySlotUI slot = Instantiate(_slotPrefab, _slotParent);

            slot.SetOwner(this);
            slot.SetIndex(i);

            _slotUIs[i] = slot;
        }

        RefreshAll();
    }

    public void Toggle()
    {
        SetOpen(!_isOpen);
    }

    public void SetOpen(bool open)
    {
        _isOpen = open;
        gameObject.SetActive(open);

        if(_isOpen)
        {
            RefreshAll();
            // 인벤토리 오픈 시 시간 정지
            Time.timeScale = 0f;
        }
        else
            Time.timeScale = 1f;
    }

    public void RefreshAll()
    {
        if (_inventory == null || _slotUIs == null)
            return;

        if (_database == null)
            return;

        int unlocked = _inventory.UnlockedSlots;

        for(int i=0;i<_slotUIs.Length;i++)
        {
            var slotUI = _slotUIs[i];
            if(slotUI == null) continue;

            var stack = _inventory.GetSlot(i);
            bool isUnlocked = i < unlocked;

            slotUI.Refresh(stack, isUnlocked, _database);
        }
    }

    public void RefreshSlot(int index)
    {
        if(_inventory == null || _slotUIs == null) return;
        if(_database == null) return;
        if(index < 0 || index >= _slotUIs.Length) return;

        int unlocked = _inventory.UnlockedSlots;
        bool isUnlocked = index < unlocked;

        var slotUI = _slotUIs[index];
        if (slotUI == null) return;

        var stack = _inventory.GetSlot(index);
        slotUI.Refresh(stack, isUnlocked, _database);
    }

    bool IsUnlocked(int index) => _inventory != null && index >= 0 && index < _inventory.UnlockedSlots;

    // 드래그 & 드랍 슬롯 호출 API
    public void OnSlotBeginDrag(int fromIndex, PointerEventData eventData)
    {
        if(!_isOpen) return;
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if(_inventory == null || _database == null) return;
        if (!IsUnlocked(fromIndex)) return;

        var stack = _inventory.GetSlot(fromIndex);
        if (stack == null || stack.IsEmpty) return;

        var spec = _database.GetById(stack.ItemId);
        if (spec == null || spec.iconSprite == null) return;

        _dragFromIndex = fromIndex;
        _isDragging = true;

        CreateDragIconIfNeeded();
        _dragIconImage.sprite = spec.iconSprite;
        _dragIconImage.enabled = true;

        UpdateDragIconPosition(eventData);
    }

    public void OnSlotDrag(PointerEventData eventData)
    {
        if (!_isDragging) return;
        UpdateDragIconPosition(eventData);
    }

    public void OnSlotDrop(int targetIndex, PointerEventData eventData)
    {
        if(!_isDragging) return;
        if (eventData.button != PointerEventData.InputButton.Left) return;

        if (_inventory == null) return;
        if(!IsUnlocked(targetIndex)) return;

        if (targetIndex == _dragFromIndex) return;

        bool changed = _inventory.TryMoveOrMerge(_dragFromIndex, targetIndex);

        if (changed)
        {
            RefreshSlot(_dragFromIndex);
            RefreshSlot(targetIndex);
        }
    }

    public void OnSlotEndDrag(PointerEventData eventData)
    {
        if (!_isDragging) return;

        CancelDrag();
    }

    // Drag Icon

    // 드래그 이미지가 없으면 생성
    void CreateDragIconIfNeeded()
    {
        if (_dragIconRT != null) return;

        if(_rootCanvas == null) _rootCanvas = GetComponentInParent<Canvas>();
        if (_rootCanvas == null) return;

        var go = new GameObject("DragIcon", typeof(RectTransform), typeof(Canvas), typeof(Image));
        go.transform.SetParent(_rootCanvas.transform, false);

        _dragIconRT = go.GetComponent<RectTransform>();
        _dragIconRT.sizeDelta = _dragIconSize;

        _dragIconImage = go.GetComponent<Image>();
        _dragIconImage.raycastTarget = false;
        _dragIconImage.preserveAspect = true;
        _dragIconImage.enabled = false;
    }

    void UpdateDragIconPosition(PointerEventData eventData)
    {
        if (_dragIconRT == null || _rootCanvas == null) return;

        RectTransform canvasRT = _rootCanvas.transform as RectTransform;

        if(RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRT, eventData.position, eventData.pressEventCamera,
            out Vector2 localPos))
        {
            _dragIconRT.anchoredPosition = localPos;
        }
    }

    void CancelDrag()
    {
        _isDragging = false;
        _dragFromIndex = -1;

        if(_dragIconRT != null )
        {
            Destroy(_dragIconRT.gameObject);
            _dragIconRT = null;
            _dragIconImage = null;
        }
    }
}
