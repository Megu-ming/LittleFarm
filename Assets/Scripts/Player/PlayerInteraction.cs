using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerInteraction : MonoBehaviour
{
    GridSelector _gridSelector;

    public void Initialize(GridSelector gridSelector)
    {
        _gridSelector = gridSelector;
    }

    public void OnInteract()
    {
        if(_gridSelector == null)
        {
            Debug.LogWarning("[PlayerInteraction] GridSelector 참조가 없습니다.");
            return;
        }

        if (!_gridSelector.TryGetInteractTarget(transform.position, out IInteractable interactable))
            return;

        interactable.Interact(this);
    }
}
