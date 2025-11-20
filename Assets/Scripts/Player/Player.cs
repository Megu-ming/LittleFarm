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

    [Header("내부 참조")]
    [SerializeField] CharacterController characterController;
    [SerializeField] PlayerController controller;
    [SerializeField] PlayerAction action;
    [SerializeField] PlayerInteraction interaction;
    [SerializeField] Animator animator;

    [Header("도구")]
    [SerializeField] ToolData[] _tools = new ToolData[6];
    [SerializeField] Transform rightHandPropTransform;

    [Header("현재 상태")]
    [SerializeField] PlayerState currentState = PlayerState.Idle;

    // 읽기 전용 프로퍼티
    public PlayerState CurrentState => currentState;
    public ToolData[] Tools => _tools;

    public void SetState(PlayerState state)
    {
        currentState = state;
    }

    public void Initialize()
    {
        controller.Initialize(this, characterController, animator);
        action.Initialize(gridSelector, animator);
        interaction.Initialize(gridSelector);
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
        action.SetActionInput(isPressed);
    }

    public void EquipTool(ToolData selected)
    {
        action.EquipTool(selected);
    }
}
