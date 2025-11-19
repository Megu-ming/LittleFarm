using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAction : MonoBehaviour
{
    [Header("도구 상태")]
    [SerializeField] ToolData _currentTool;

    GridSelector _gridSelector;

    public ToolData CurrentTool => _currentTool;
    public void Initialize(GridSelector gridSelector)
    {
        _gridSelector = gridSelector;
    }

    public void EquipTool(ToolData tooldata)
    {
        _currentTool = tooldata;
    }

    public void OnAction()
    {
        if (_currentTool == null || _currentTool.ToolType == ToolType.None)
            return;

        if(_gridSelector == null)
        {
            Debug.LogWarning("[PlayerAction] GridSelector 참조가 없습니다.");
            return;
        }

        if (!_gridSelector.TryGetToolTarget(transform.position, _currentTool, out IToolTarget target, out Vector3 hitPoint, out Vector3 hitNormal))
            return;

        var ctx = new ToolActionContext
        {
            user = this,
            toolType = _currentTool.ToolType,
            power = _currentTool.Power,
            hitPoint = hitPoint,
            hitNormal = hitNormal
        };

        target.OnToolAction(ctx);
    }
}
