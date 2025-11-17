using UnityEngine;

public struct ToolActionContext
{
    public PlayerAction user;
    public ToolType toolType;
    public Vector3 hitPoint;
    public Vector3 hitNormal;
}

public interface IToolTarget
{
    void OnToolAction(ToolActionContext context);
}
