using UnityEngine;
using UnityEngine.InputSystem;

public class ToolChangerUI : MonoBehaviour
{
    [SerializeField] Animator animator;
    bool _isOpen = false;
    public bool IsOpen
    {
        get { return _isOpen; }
        set { _isOpen = value; animator.SetBool("isOpen", value); }
    }

    public void OnToolChange(InputAction.CallbackContext context)
    {
        if (context.started)
            Open();
        else if (context.canceled)
            Close();
    }

    void Open()
    {
        IsOpen = !IsOpen;
    }

    void Close()
    {
        IsOpen = !IsOpen;
    }
}
