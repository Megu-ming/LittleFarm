using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

/// <summary>
/// 입력 인스펙터 연결 담당
/// 플레이어의 모든 컴포넌트에 Initialize 담당
/// </summary>
public class Player : MonoBehaviour
{
    [Header("외부 참조")]
    [SerializeField] GridSelector _gridSelector;
    [SerializeField] ToolChangerUI _toolChangerUI;
    [SerializeField] CropManager _cropManager;

    [Header("내부 참조")]
    [SerializeField] CharacterController _characterController;
    [SerializeField] PlayerController _controller;
    [SerializeField] PlayerAction _action;
    [SerializeField] PlayerInteraction _interaction;
    [SerializeField] Animator _animator;
    [SerializeField] PlayerItemMagnet _itemMagnet;    

    [Header("인벤토리")]
    [SerializeField] Inventory _inventory;
    [SerializeField] InventoryUI _inventoryUI;
    [SerializeField] QuickSlotUI _quickSlotUI;

    [Header("도구")]
    [SerializeField] ToolData[] _tools = new ToolData[6];
    [Tooltip("도구를 들고 있는 오른손 트랜스폼")]
    [SerializeField] Transform _rightHandPropTransform;
    [Tooltip("현재 들고 있는 도구")]
    [SerializeField] GameObject _currentToolInstance;
    [SerializeField] ToolData _currentToolData;
    [Header("손 아이템 Props")]
    [Tooltip("아이템을 들고 있는 오른손 트랜스폼")]
    [SerializeField] Transform _rightHandItemPropTransform;
    [Tooltip("현재 들고 있는 아이템")]
    [SerializeField] GameObject _currentHandItemInstance;
    [SerializeField] int _currentHandItemId = -1;
    [SerializeField] int _lastItemHand = 0;

    [Header("현재 상태")]
    [SerializeField] PlayerState _currentState = PlayerState.Idle;
    [SerializeField] int _hand = -1;     // 키보드 상단의 1~0 만약 도구를 들고있다면 -1

    // 읽기 전용 프로퍼티
    public PlayerState CurrentState => _currentState;
    public ToolData[] Tools => _tools;
    public ToolData CurrentToolData => _currentToolData;
    public int Hand => _hand;

    // Hand 변경 이벤트 구독 함수
    public Action<int> HandChanged;
    // Hand 변경 애니메이션
    static readonly int IsHoldingHash = Animator.StringToHash("IsHolding");


    public void SetState(PlayerState state)
    {
        _currentState = state;
    }

    public void SetHand(int index)
    {
        if (index < 0 || index >= 10)
            return;

        // 도구 상태(-1)에서 아이템으로 전환하면 도구 프롭 제거
        if (_hand == -1)
            ClearToolVisual();

        _hand = index;
        _lastItemHand = index;

        HandChanged?.Invoke(_hand);
        RefreshHandItemProp();
        UpdateHoldingAnimFlag();
    }

    public void Initialize(InventoryUI inventoryUI, ToolChangerUI toolChangerUI, QuickSlotUI quickSlotUI, GridSelector gridSelector, CropManager cropManager)
    {
        _gridSelector = gridSelector;
        _cropManager = cropManager;

        _controller.Initialize(this, _gridSelector);
        _action.Initialize(this, _gridSelector, _animator);
        _interaction.Initialize(_gridSelector);
        _itemMagnet.Initialize(this);

        _inventoryUI = inventoryUI;
        _quickSlotUI = quickSlotUI;
        _toolChangerUI = toolChangerUI;

        if (_inventory != null)
            _inventory.SlotChanged += OnInventorySlotChanged;

        RefreshHandItemProp();
        UpdateHoldingAnimFlag();
    }

    private void Update()
    {
        if(actionHold)
            _action.Action();

        if(interactHold)
            _interaction.OnInteract();
    }

    #region Input Actions
    public void OnMove(InputAction.CallbackContext context)
    {
        _controller.Move(context.ReadValue<Vector2>());
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        _controller.OnSprint(context);
    }

    bool interactHold = false;
    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed)
            interactHold = true;
        if (context.canceled)
            interactHold = false;
    }

    bool actionHold = false;
    public void OnAction(InputAction.CallbackContext context)
    {
        if (context.performed)
            actionHold = true;
        if(context.canceled)
            actionHold = false;
    }

    public void OnTab(InputAction.CallbackContext context)
    {
        _toolChangerUI.OnToolWheel(context);
    }

    public void OnInfo(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (_inventoryUI != null)
            _inventoryUI.Toggle();
    }

    public void OnQuickSlotSelect(InputAction.CallbackContext context)
    {
        if(!context.performed) return;

        if (context.control is not KeyControl keyControl) return;

        int handIndex = KeyToHandIndex(keyControl.keyCode);
        if (handIndex < 0) return;

        SetHand(handIndex);
    }

    int KeyToHandIndex(Key key)
    {
        switch (key)
        {
            case Key.Digit1: return 0;
            case Key.Digit2: return 1;
            case Key.Digit3: return 2;
            case Key.Digit4: return 3;
            case Key.Digit5: return 4;
            case Key.Digit6: return 5;
            case Key.Digit7: return 6;
            case Key.Digit8: return 7;
            case Key.Digit9: return 8;
            case Key.Digit0: return 9;
        }

        return -1;
    }

    public void OnQuickSlotScroll(InputAction.CallbackContext context)
    {
        if(!context.performed) return;

        Vector2 scroll = context.ReadValue<Vector2>();
        float y = scroll.y;

        if (Mathf.Abs(y) < 0.01f) return;

        int dir = (y < 0f) ? 1 : -1;

        int baseIndex = (_hand == -1) ? _lastItemHand : _hand;
        int nextIndex = (baseIndex + dir) % 10;
        if (nextIndex < 0) nextIndex += 10;
        
        SetHand(nextIndex);
    }
    #endregion

    /// <summary>
    /// 도구 장착 (없으면 오브젝트 생성)
    /// </summary>
    /// <param name="selected"></param>
    public void EquipTool(ToolData selected)
    {
        // 도구 해제
        if (selected == null)
        {
            ClearToolVisual();

            // 도구 상태였다면 마지막 아이템 슬롯로 복귀
            if (_hand == -1)
                SetHand(_lastItemHand);

            return;
        }

        // 아이템 들고 있던 상태라면 복귀 인덱스 저장
        if (_hand != -1)
            _lastItemHand = _hand;

        // 아이템 프롭 제거(도구랑 동시에 보이지 않게)
        ClearHandItemProp();

        // 기존 도구 제거 후 교체
        ClearToolVisual();
        _currentToolData = selected;

        if (_currentToolData.ToolPrefab == null)
        {
            Debug.Log("[Player] _currentToolData에 ToolPrefab이 없습니다.");
            return;
        }

        if (_rightHandPropTransform == null)
        {
            Debug.LogWarning("[Player] RightHandPropTransform이 비어 있습니다.");
            return;
        }

        _currentToolInstance = Instantiate(_currentToolData.ToolPrefab, _rightHandPropTransform);

        _hand = -1;                 
        HandChanged?.Invoke(_hand);
        UpdateHoldingAnimFlag();
    }

    public bool TryPickupItem(int itemId, int amount)
    {
        if(_inventory == null || amount <= 0)
            return false;

        bool allAdded = _inventory.TryAddItem(itemId, amount, out int remainder);
        if (_inventoryUI != null && _quickSlotUI)
        {
            _inventoryUI.RefreshAll();
            _quickSlotUI.RefreshAll();
        }

        return allAdded && remainder == 0;
    }

    public bool TryUseItem(FarmTile targetTile)
    {
        if (_hand < 0) return false;
        if (_inventory == null || _inventory.Database == null) return false;

        var slot = _inventory.GetSlot(_hand);
        if (slot == null || slot.IsEmpty) return false;

        int itemId = slot.ItemId;
        var spec = _inventory.Database.GetById(itemId);
        if (spec == null) return false;

        bool used = false;

        switch (spec.category)
        {
            case ItemCategory.Seed:
                used = TryUseSeed(itemId, targetTile);
                break;
            // case ItemCategory.Food: ...
            // case ItemCategory.Resource: ...
            default:
                used = false;
                break;
        }

        if (!used) return false;

        bool consumed = _inventory.TryConsumeFromSlot(_hand, 1);
        if (!consumed) return false;

        if (_inventoryUI != null && _quickSlotUI != null)
        {
            _inventoryUI.RefreshAll();
            _quickSlotUI.RefreshAll();
        }

        return true;
    }

    bool TryUseSeed(int seedItemId, FarmTile targetTile)
    {
        if (targetTile == null) return false;

        // 씨앗은 “경작된 타일”에서만
        if (!targetTile.CanPlantSeed) return false;

        if (_cropManager == null) return false;

        return _cropManager.PlantSeed(targetTile, seedItemId, 3);
    }

    void OnInventorySlotChanged(int slotIndex)
    {
        // 현재 손이 가리키는 슬롯이 바뀌면 손 프롭도 갱신
        if (_hand == slotIndex)
        {
            RefreshHandItemProp();
            UpdateHoldingAnimFlag();
        }
    }

    void RefreshHandItemProp()
    {
        if (_hand < 0)
        {
            ClearHandItemProp();
            return;
        }

        if (_inventory == null || _inventory.Database == null)
        {
            ClearHandItemProp();
            return;
        }

        var slot = _inventory.GetSlot(_hand);
        if (slot == null || slot.IsEmpty)
        {
            ClearHandItemProp();
            return;
        }

        var spec = _inventory.Database.GetById(slot.ItemId);
        if (spec == null || spec.handPrefab == null)
        {
            ClearHandItemProp();
            return;
        }

        if (_rightHandItemPropTransform == null)
        {
            Debug.LogWarning("[Player] _rightHandItemPropTransform이 비어 있습니다.");
            return;
        }

        // 같은 아이템이면 재생성하지 않음
        if (_currentHandItemInstance != null && _currentHandItemId == slot.ItemId)
            return;

        ClearHandItemProp();

        _currentHandItemInstance = Instantiate(spec.handPrefab);
        _currentHandItemInstance.transform.SetParent(_rightHandItemPropTransform, false);

        _currentHandItemId = slot.ItemId;
    }

    void ClearHandItemProp()
    {
        if (_currentHandItemInstance != null)
            Destroy(_currentHandItemInstance);

        _currentHandItemInstance = null;
        _currentHandItemId = -1;
    }

    void ClearToolVisual()
    {
        if (_currentToolInstance != null)
            Destroy(_currentToolInstance);

        _currentToolInstance = null;
        _currentToolData = null;
    }

    void UpdateHoldingAnimFlag()
    {
        if (_animator == null)
            return;

        bool isHolding = false;

        if (_hand >= 0 && _hand < 10 && _inventory != null)
        {
            var slot = _inventory.GetSlot(_hand);
            isHolding = (slot != null && !slot.IsEmpty);
        }

        _animator.SetBool(IsHoldingHash, isHolding);
    }
}
