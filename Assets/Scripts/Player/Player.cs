using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("외부 참조")]
    [SerializeField] Camera mainCam;

    [Header("내부 참조")]
    [SerializeField] CharacterController characterController;
    [SerializeField] PlayerController controller;
    [SerializeField] PlayerAction action;
    [SerializeField] PlayerInteraction interaction;

    public void Initialize()
    {
        controller.Initialize(characterController);
        action.Initialize(mainCam);
        interaction.Initialize(mainCam);
    }

    private void Start()
    {
        Initialize();
    }
}
