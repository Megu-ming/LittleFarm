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

    public void Action()
    {
        if (_player.CurrentState == PlayerState.Acting) return;

        _cachedActionTile = _gridSelector != null ? _gridSelector.CurrentTile : null;
        if(_cachedActionTile == null) return;

        FaceActionTarget();

        var tool = CurrentTool;
        if (tool != null && tool.ToolType != ToolType.None)
        {
            if (!string.IsNullOrEmpty(tool.TriggerName))
                _animator.SetTrigger(tool.TriggerName);
        }

        _player.TryUseItem(_cachedActionTile);
    }

    /// <summary>
    /// 캐싱된 타일(또는 현재 타일) 방향을 바라보게 회전
    /// </summary>
    private void FaceActionTarget()
    {
        // 우선 캐싱된 타일을 쓰고, 없으면 현재 타일 사용
        FarmTile tile = _cachedActionTile;
        if (tile == null && _gridSelector != null)
            tile = _gridSelector.CurrentTile;

        if (tile == null)
            return;

        Vector3 targetPos = tile.transform.position;
        Vector3 dir = targetPos - transform.position;
        dir.y = 0f; // Y축 무시하고 수평 회전만

        if (dir.sqrMagnitude < 0.0001f)
            return;

        transform.rotation = Quaternion.LookRotation(dir.normalized);
    }

    /// <summary>
    /// 각 도구 행동 애니메이션 클립에서 타이밍에 맞게 호출됨
    /// </summary>
    /// <returns></returns>
    public bool TryDoToolAction()
    {
        var tool = CurrentTool;
        if (tool == null || tool.ToolType == ToolType.None)
            return false;

        FarmTile target = _cachedActionTile;
        _cachedActionTile = null;

        if (target == null)
            return false;

        Vector3 hitPoint = target.transform.position;
        Vector3 hitNormal = Vector3.up;

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
