using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 입력 인스펙터 연결 담당
/// 플레이어의 모든 컴포넌트에 Initialize 담당
/// </summary>
public class Player : MonoBehaviour
{
    [Header("외부 참조")]
    [SerializeField] GridSelector _gridSelector;
    [SerializeField] ToolChangerUI _toolChangerUI;

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

    [Header("현재 상태")]
    [SerializeField] PlayerState _currentState = PlayerState.Idle;
    [SerializeField] int _hand = -1;     // 키보드 상단의 1~0 만약 도구를 들고있다면 -1

    // 읽기 전용 프로퍼티
    public PlayerState CurrentState => _currentState;
    public ToolData[] Tools => _tools;
    public ToolData CurrentToolData => _currentToolData;
    public int Hand => _hand;

    public Action<int> HandChanged;

    public void SetState(PlayerState state)
    {
        _currentState = state;
    }

    public void SetHand(int index)
    {
        if(_hand == -1)
        {
            Destroy(_currentToolInstance);
            _currentToolData = null;
        }
        if (index < 0 || index >= 10)
        { 
            _hand = 0; 
            return; 
        }
        _hand = index;
    }

    public void Initialize(InventoryUI inventoryUI, ToolChangerUI toolChangerUI, QuickSlotUI quickSlotUI)
    {
        _controller.Initialize(this, _characterController, _animator, _gridSelector);
        _action.Initialize(this, _gridSelector, _animator);
        _interaction.Initialize(_gridSelector);
        _itemMagnet.Initialize(this);

        _inventoryUI = inventoryUI;
        _quickSlotUI = quickSlotUI;
        _toolChangerUI = toolChangerUI;
    }

    private void Update()
    {
        if(actionHold)
            _action.Action();

        if(interactHold)
            _interaction.OnInteract();
    }

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

    public void EquipTool(ToolData selected)
    {
        _currentToolData = selected;

        if (_currentToolData == null)
        {
            if (_currentToolInstance != null)
            {
                Destroy(_currentToolInstance);
                _currentToolInstance = null;
                _currentToolData = null;
            }
            return;
        }

        if (_currentToolData.ToolPrefab == null)
        {
            Debug.Log("[Player] _currentToolData에 ToolPrefab이 없습니다.");
            return;
        }

        if(_rightHandPropTransform == null)
        {
            Debug.LogWarning("[Player] RightHandPropTransform이 비어 있습니다.");
            return;
        }
        // 도구 오브젝트 생성
        _currentToolInstance = Instantiate(_currentToolData.ToolPrefab, _rightHandPropTransform);
        // 현재 Hand 갱신
        _hand = -1;
        HandChanged?.Invoke(_hand);
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
}
