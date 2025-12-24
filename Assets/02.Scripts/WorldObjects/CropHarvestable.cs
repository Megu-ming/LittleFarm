using UnityEngine;

public class CropHarvestable : MonoBehaviour, IToolTarget, IInteractable
{
    string _cropItemKey;
    Vector3 _dropOffset = new Vector3(0f, 0.5f, 0f);

    public void Initialize(string cropItemKey)
    {
        _cropItemKey = cropItemKey;
    }

    public void Interact(PlayerInteraction interactor)
    {
        GameInitializer.Instance.DropItems(_cropItemKey, _dropOffset);

        Destroy(gameObject);
    }

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
