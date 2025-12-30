using UnityEngine;

public class SeedToolTarget : PlacedObject, IToolTarget
{
    public void OnToolAction(ToolActionContext context)
    {
        if(context.toolType == ToolType.Pickaxe)
        {
            var hitTile = context.hitTile;
            if(hitTile.occupant != null)
            {
                Destroy(gameObject);
                // 파괴됐을 때 CropManager가 모르고있음 CropHarvastable에서도 마찬가지로 수정해야함
            }                
        }
    }
}
