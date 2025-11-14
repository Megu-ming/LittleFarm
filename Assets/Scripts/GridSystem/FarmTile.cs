using UnityEngine;

public class FarmTile : MonoBehaviour
{
    public int x;
    public int z;

    public bool used = false;
    public GameObject occupant;

    public void SetOccupant(GameObject obj)
    {
        occupant = obj;
        used = (obj != null);
    }

    public void ClearOccupant()
    {
        Debug.Log($"({x}, {z}) 타일 프리팹 삭제");
        used = false;
        occupant = null;
    }
}   
