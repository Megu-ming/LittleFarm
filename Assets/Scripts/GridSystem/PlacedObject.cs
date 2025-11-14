using UnityEngine;

[ExecuteAlways]
public class PlacedObject : MonoBehaviour
{
    public FarmTile ownerTile;

    private void OnDestroy()
    {
        if(ownerTile != null)
        {
            ownerTile.ClearOccupant();
        }
    }
}
