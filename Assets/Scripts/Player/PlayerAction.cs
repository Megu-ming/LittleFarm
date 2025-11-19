using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAction : MonoBehaviour
{
    [Header("도구 상태")]
    [SerializeField] ToolData _currentTool;

    GridSelector _gridSelector;
    Animator _animator;

    public ToolData CurrentTool => _currentTool;
    public void Initialize(GridSelector gridSelector, Animator animator)
    {
        _gridSelector = gridSelector;
        _animator = animator;
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

        _animator.SetTrigger("Action");

        if (!_gridSelector.TryGetToolTargetFromMouseDirection(
                transform.position,
                _currentTool,
                out IToolTarget target,
                out Vector3 hitPoint,
                out Vector3 hitNormal))
        {
            // 범위 밖이거나 타겟이 없음
            return;
        }

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
