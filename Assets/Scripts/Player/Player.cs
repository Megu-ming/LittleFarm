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

    public void Initialize()
    {
        controller.Initialize(characterController);
        action.Initialize(gridSelector);
        interaction.Initialize(gridSelector);
    }

    private void Start()
    {
        Initialize();
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
        if (!context.performed)
            return;

        action.OnAction();
    }
}
