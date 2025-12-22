using UnityEngine;

public struct ToolActionContext
{
    public PlayerAction user;
    public ToolType toolType;
    public float power;
    public FarmTile hitTile;
    public Vector3 hitNormal;
}

public interface IToolTarget
{
    void OnToolAction(ToolActionContext context);
}
