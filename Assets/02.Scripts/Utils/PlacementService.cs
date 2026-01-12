using UnityEngine;

public static class PlacementService
{
    static bool _initialized;
    static Transform _worldRoot;

    public static void Initialize(Transform worldRoot = null)
    {
        _worldRoot = worldRoot;
        _initialized = true;
    }

    public static T PlaceOnTile<T>(
        T prefab,
        FarmTile tile,
        Vector3 offset,
        Quaternion rotation,
        bool replaceExisting = true
    ) where T : Component
    {
        EnsureInitialized();
        if (prefab == null || tile == null) return null;

        if (replaceExisting && tile.occupant != null)
        {
            Object.Destroy(tile.occupant);
            tile.ClearOccupant();
        }

        Vector3 pos = tile.transform.position + offset;
        var inst = Object.Instantiate(prefab, pos, rotation, _worldRoot);

        var placed = EnsurePlacedObject(inst.gameObject);
        placed.SetOwner(tile);

        return inst;
    }

    public static GameObject PlaceOnTile(
        GameObject prefab,
        FarmTile tile,
        Vector3 offset,
        Quaternion rotation,
        bool replaceExisting = true
    )
    {
        EnsureInitialized();
        if (prefab == null || tile == null) return null;

        if (replaceExisting && tile.occupant != null)
        {
            Object.Destroy(tile.occupant);
            tile.ClearOccupant();
        }

        Vector3 pos = tile.transform.position;
        var inst = Object.Instantiate(prefab, pos, rotation, _worldRoot);

        var placed = EnsurePlacedObject(inst);
        placed.SetOwner(tile);

        return inst;
    }

    // 멀티 점유(건물/큰 오브젝트)
    public static T PlaceFootprint<T>(
        T prefab,
        FarmTile originTile,
        FarmTile[] tiles,
        Vector3 offset,
        Quaternion rotation,
        bool replaceExisting = true
    ) where T : Component
    {
        EnsureInitialized();
        if (prefab == null || originTile == null || tiles == null || tiles.Length == 0) return null;

        if (replaceExisting)
        {
            for (int i = 0; i < tiles.Length; i++)
            {
                var t = tiles[i];
                if (t == null) continue;
                if (t.occupant != null)
                {
                    Object.Destroy(t.occupant);
                    t.ClearOccupant();
                }
            }
        }

        Vector3 pos = originTile.transform.position + offset;
        var inst = Object.Instantiate(prefab, pos, rotation, _worldRoot);

        var placed = EnsurePlacedObject(inst.gameObject);
        placed.SetOccupiedTiles(originTile, tiles);

        return inst;
    }

    public static GameObject PlaceFootprint(
    GameObject prefab,
    FarmTile originTile,
    FarmTile[] tiles,
    Vector3 offset,
    Quaternion rotation,
    bool replaceExisting = true
)
    {
        EnsureInitialized();
        if (prefab == null || originTile == null || tiles == null || tiles.Length == 0) return null;

        if (replaceExisting)
        {
            for (int i = 0; i < tiles.Length; i++)
            {
                var t = tiles[i];
                if (t == null) continue;

                if (t.occupant != null)
                {
                    Object.Destroy(t.occupant);
                    t.ClearOccupant();
                }
            }
        }

        Vector3 pos = originTile.transform.position + offset;
        var inst = Object.Instantiate(prefab, pos, rotation, _worldRoot);

        var placed = EnsurePlacedObject(inst);
        placed.SetOccupiedTiles(originTile, tiles);

        return inst;
    }

    static PlacedObject EnsurePlacedObject(GameObject go)
    {
        var placed = go.GetComponent<PlacedObject>();
        if (placed == null)
            placed = go.AddComponent<PlacedObject>();
        return placed;
    }

    static void EnsureInitialized()
    {
        if (_initialized) return;
        Debug.LogError("[PlacementService] Initialize()가 호출되지 않았습니다. GameInitializer에서 먼저 호출하세요.");
        _initialized = true; // 실패 연쇄 방지(임시)
    }
}
