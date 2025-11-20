using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAction : MonoBehaviour
{
    Player _player;
    GridSelector _gridSelector;
    Animator _animator;
    FarmTile _cachedActionTile;

    public ToolData CurrentTool => _player != null ? _player.CurrentToolData : null;

    public void Initialize(Player player, GridSelector gridSelector, Animator animator)
    {
        _player = player;
        _gridSelector = gridSelector;
        _animator = animator;
    }

    public void SetActionInput(bool isPressed)
    {
        var tool = CurrentTool;
        if (isPressed == false || tool == null || tool.ToolType == ToolType.None) return;

        _cachedActionTile = _gridSelector != null ? _gridSelector.CurrentTile : null;
        _animator.SetTrigger("Action");
    }

    /// <summary>
    /// 행동 애니메이션 클립에서 호출됨
    /// </summary>
    /// <returns></returns>
    public bool TryDoToolAction()
    {
        var tool = CurrentTool;
        if (tool == null || tool.ToolType == ToolType.None)
            return false;

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
            toolType = tool.ToolType,
            power = tool.Power,
            hitPoint = hitPoint,
            hitNormal = hitNormal
        };
        
        target.OnToolAction(ctx);
        return true;
    }
}
