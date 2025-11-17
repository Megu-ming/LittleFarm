using UnityEngine;

public class TreeToolTarget : MonoBehaviour, IToolTarget
{
    public void OnToolAction(ToolActionContext context)
    {
        if(context.toolType == ToolType.Axe)
        {
            Debug.Log($"{name} Hit!");
        }
    }
}
