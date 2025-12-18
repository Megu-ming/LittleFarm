using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 농작물 & 자랄 수 있는 나무들을 관리하는 작물 매니저
/// </summary>
public class CropManager : MonoBehaviour
{
    [Serializable]
    public class SeedVisual
    {
        public int _seedItemId;
        public GameObject _plantedPrefab;
        public Vector3 _offset = new Vector3(0, 0.05f, 0);
    }

    [Header("심기 프리팹")]
    [SerializeField] List<SeedVisual> _seedVisuals = new List<SeedVisual>();
    [SerializeField] GameObject _defaultPlantedPrefab;

    GameTimeManager _timeManager;
    readonly List<FarmTile> _tiles = new List<FarmTile>();
    Dictionary<int, SeedVisual> _seedVisualById;

    public void Initialize(GameTimeManager timeManager, GridManager gridManager)
    {
        _timeManager = timeManager;

        _timeManager.OnDateChanged += HandleNewDay;

        _tiles.Clear();

        var tiles = gridManager.Tiles;
        if(tiles!=null)
        {
            int width  = tiles.GetLength(0);
            int height = tiles.GetLength(1);
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    _tiles.Add(tiles[x, y]);
        }

        _seedVisualById = new Dictionary<int, SeedVisual>();
        foreach(var v in _seedVisuals)
        {
            if (v == null) continue;
            if (!_seedVisualById.ContainsKey(v._seedItemId))
                _seedVisualById[v._seedItemId] = v;
        }
    }

    void HandleNewDay(int year, Season season, int day)
    {
        foreach(var tile in _tiles)
        {
            if (tile != null)
                tile.AdvancedGrowthOneDay();
        }
    }

    public bool PlantSeed(FarmTile tile, int seedItemId, int maxGrouthDays = 3)
    {
        if(tile == null) return false;
        if (!tile.TryPlantSeed(seedItemId, maxGrouthDays)) return false;

        if (tile.occupant != null) Destroy(tile.occupant);

        GameObject prefab = _defaultPlantedPrefab;

        if (_seedVisualById != null && _seedVisualById.TryGetValue(seedItemId, out var visual))
        {
            if (visual._plantedPrefab != null)
                prefab = visual._plantedPrefab;
        }

        if (prefab == null)
        {
            Debug.LogWarning("[CropManager] planted prefab이 없습니다. (default도 null)");
            return true;
        }

        Vector3 spawnPos = tile.transform.position;
        Vector3 offset = Vector3.zero;

        if (_seedVisualById != null && _seedVisualById.TryGetValue(seedItemId, out var v2))
            offset = v2._offset;

        var obj = Instantiate(prefab, spawnPos + offset, Quaternion.identity);
        tile.SetOccupant(obj); // used/occupant 동기화

        return true;
    }
}
