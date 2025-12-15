using UnityEngine;

[ExecuteAlways]
public class PlacedObject : MonoBehaviour
{
    public FarmTile ownerTile;

    private void OnEnable()
    {
        // 에디터/플레이 모두에서, 활성화될 때마다
        // 자기 타일에 자신을 occupant로 다시 등록
        if (ownerTile != null)
        {
            ownerTile.SetOccupant(gameObject);
        }
    }

    private void OnDestroy()
    {
        // 삭제될 때는 타일 비우기
        if (ownerTile != null)
        {
            ownerTile.ClearOccupant();
        }
    }
}
