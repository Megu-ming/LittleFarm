using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAction : MonoBehaviour
{
    [Header("도구 상태")]
    [SerializeField] ToolData _currentTool;

    GridSelector _gridSelector;
    Animator _animator;
    FarmTile _cachedActionTile;

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

    public void SetActionInput(bool isPressed)
    {
        if (isPressed == false || _currentTool == null || _currentTool.ToolType == ToolType.None) return;

        _cachedActionTile = _gridSelector != null ? _gridSelector.CurrentTile : null;
        _animator.SetTrigger("Action");
    }

    /// <summary>
    /// 행동 애니메이션 클립에서 호출됨
    /// </summary>
    /// <returns></returns>
    public bool TryDoToolAction()
    {
        FarmTile tile = _cachedActionTile;
        _cachedActionTile = null;

        if (tile == null)
            return false;

        IToolTarget target = null;
        Vector3 hitPoint = tile.transform.position;
        Vector3 hitNormal = Vector3.up;

        if(tile.occupant != null)
        {
            target = tile.occupant.GetComponentInChildren<IToolTarget>();
            if (target != null)
                hitPoint = tile.occupant.transform.position;
        }

        if (target == null)
            return false;

        var ctx = new ToolActionContext
        {
            user = this,
            toolType = _currentTool.ToolType,
            power = _currentTool.Power,
            hitPoint = hitPoint,
            hitNormal = hitNormal
        };
        
        target.OnToolAction(ctx);
        return true;
    }
}
