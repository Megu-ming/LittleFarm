using UnityEngine;

public class SeedToolTarget : MonoBehaviour, IToolTarget
{
    public void OnToolAction(ToolActionContext context)
    {
        if(context.toolType == ToolType.Pickaxe)
        {
            var hitTile = context.hitTile;
            if(hitTile.occupant != null)
            {
                Destroy(gameObject);
                hitTile.ClearOccupant();
            }                
        }
    }
}
