using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAction : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] Camera _mainCam;

    [Header("도구 상태")]
    [SerializeField] private ToolType _currentTool = ToolType.None;

    [Header("설정")]
    [SerializeField] float _actionMaxDistance = 2f;
    [SerializeField] LayerMask _actionMask = ~0;

    public void OnAction(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        if (_currentTool == ToolType.None)
            return;

        TryToolAction();
    }

    void TryToolAction()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = _mainCam.ScreenPointToRay(mousePos);

        if (!Physics.Raycast(ray, out RaycastHit hit, 100f, _actionMask))
            return;

        Vector3 hitPos = hit.point;
        float dist = Vector3.Distance(transform.position, hitPos);

        if(dist > _actionMaxDistance)
            return;

        IToolTarget target = hit.collider.GetComponentInParent<IToolTarget>();
        if(target == null)
            return;

        var ctx = new ToolActionContext
        {
            user = this,
            toolType = _currentTool,
            hitPoint = hit.point,
            hitNormal = hit.normal
        };

        target.OnToolAction(ctx);
    }


}
