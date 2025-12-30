using UnityEngine;

[ExecuteAlways]
public class PlacedObject : MonoBehaviour
{
    [SerializeField] FarmTile _ownerTile;
    [SerializeField] FarmTile[] _occupiedTiles;

    public FarmTile OwnerTile => _ownerTile;
    public FarmTile[] OccupiedTiles => _occupiedTiles;

    public void SetOwner(FarmTile ownerTile)
    {
        _ownerTile = ownerTile;
        RegisterOccupancy();
    }

    public void SetOccupiedTiles(FarmTile origin, FarmTile[] tiles)
    {
        _ownerTile = origin;
        _occupiedTiles = tiles;

        RegisterOccupancy();
    }

    private void OnEnable()
    {
        RegisterOccupancy();
    }

    private void RegisterOccupancy()
    {
        // 멀티가 있으면 멀티 우선
        if (_occupiedTiles != null && _occupiedTiles.Length > 0)
        {
            for (int i = 0; i < _occupiedTiles.Length; i++)
                TryRegisterToTile(_occupiedTiles[i]);

            return;
        }

        // 멀티가 없으면 단일로 동작(기존 호환)
        TryRegisterToTile(_ownerTile);
    }

    private void TryRegisterToTile(FarmTile tile)
    {
        if (tile == null) return;

        // 이미 다른 점유자가 있으면 덮어쓰지 않음
        if (tile.occupant != null && tile.occupant != gameObject)
            return;

        tile.SetOccupant(gameObject);
    }

    private void OnDestroy()
    {
        // 멀티가 있으면 멀티 해제
        if (_occupiedTiles != null && _occupiedTiles.Length > 0)
        {
            for (int i = 0; i < _occupiedTiles.Length; i++)
            {
                var tile = _occupiedTiles[i];
                if (tile != null)
                    tile.ClearOccupantIf(gameObject);
            }

            return;
        }

        // 단일 해제(기존 호환)
        if (_ownerTile != null)
            _ownerTile.ClearOccupantIf(gameObject);
    }
}
