using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAction : MonoBehaviour
{
    [Header("도구 상태")]
    [SerializeField] ToolData _currentTool;

    [Header("설정")]
    [SerializeField] float _actionMaxDistance = 2f;
    [SerializeField] LayerMask _actionMask = ~0;

    Camera _mainCam;

    public void Initialize(Camera cam)
    {
        _mainCam = cam;
    }

    public void OnAction()
    {
        if (_currentTool == null || _currentTool.ToolType == ToolType.None)
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
            toolType = _currentTool.ToolType,
            power = _currentTool.Power,
            hitPoint = hit.point,
            hitNormal = hit.normal
        };
        target.OnToolAction(ctx);
    }
}
