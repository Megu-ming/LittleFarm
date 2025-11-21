using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 입력 인스펙터 연결 담당
/// 플레이어의 모든 컴포넌트에 Initialize 담당
/// </summary>
public class Player : MonoBehaviour
{
    [Header("외부 참조")]
    [SerializeField] GridSelector gridSelector;
    [SerializeField] InventoryUI inventoryUI;

    [Header("내부 참조")]
    [SerializeField] CharacterController characterController;
    [SerializeField] PlayerController controller;
    [SerializeField] PlayerAction action;
    [SerializeField] PlayerInteraction interaction;
    [SerializeField] Animator animator;

    [Header("도구")]
    [SerializeField] ToolData[] _tools = new ToolData[6];
    [Tooltip("도구를 들고 있는 오른손 트랜스폼")]
    [SerializeField] Transform _rightHandPropTransform;
    [Tooltip("현재 들고 있는 도구")]
    [SerializeField] GameObject _currentToolInstance;
    [SerializeField] ToolData _currentToolData;

    [Header("현재 상태")]
    [SerializeField] PlayerState currentState = PlayerState.Idle;

    // 읽기 전용 프로퍼티
    public PlayerState CurrentState => currentState;
    public ToolData[] Tools => _tools;
    public ToolData CurrentToolData => _currentToolData;

    public void SetState(PlayerState state)
    {
        currentState = state;
    }

    public void Initialize(InventoryUI inventoryUI)
    {
        controller.Initialize(this, characterController, animator, gridSelector);
        action.Initialize(this, gridSelector, animator);
        interaction.Initialize(gridSelector);

        this.inventoryUI = inventoryUI;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        controller.Move(context.ReadValue<Vector2>());
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        controller.OnSprint(context);
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        interaction.OnInteract();
    }

    public void OnAction(InputAction.CallbackContext context)
    {
        float value = context.ReadValue<float>();
        bool isPressed = value > 0.5f;
        action.Action(isPressed);
    }

    public void OnInfo(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (inventoryUI != null)
            inventoryUI.Toggle();
    }

    public void EquipTool(ToolData selected)
    {
        if (_currentToolInstance != null)
        {
            Destroy(_currentToolInstance);
            _currentToolInstance = null;
        }

        _currentToolData = selected;

        if (_currentToolData == null)
            return;

        if (_currentToolData.ToolPrefab == null)
            return;

        if(_rightHandPropTransform == null)
        {
            Debug.LogWarning("[Player] RightHandPropTransform이 비어 있습니다.");
            return;
        }

        _currentToolInstance = Instantiate(_currentToolData.ToolPrefab, _rightHandPropTransform);
    }
}
