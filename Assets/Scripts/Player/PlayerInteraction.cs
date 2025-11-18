using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerInteraction : MonoBehaviour
{
    [Header("설정")]
    [SerializeField, Tooltip("상호작용 가능 반경")] float interactRadius = 2f;
    [SerializeField, Tooltip("상호작용 가능 반경")] LayerMask interactableMask = ~0;
    
    Camera _mainCam;

    public void Initialize(Camera cam)
    {
        _mainCam = cam;
    }

    public void OnInteract()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = _mainCam.ScreenPointToRay(mousePos);

        if (!Physics.Raycast(ray, out RaycastHit hit, 100f, interactableMask))
            return;

        IInteractable interactable = hit.collider.GetComponentInChildren<IInteractable>();
        if (interactable == null)
            return;

        Vector3 closest = hit.collider.ClosestPoint(transform.position);
        float dist = Vector3.Distance(transform.position, closest);

        if (dist > interactRadius)
            return;

        interactable.Interact(this);
    }
}
