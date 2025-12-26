using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 농작물 & 자랄 수 있는 나무들을 관리하는 작물 매니저
/// </summary>
public class CropManager : MonoBehaviour
{
    [Serializable]
    public class CropVisual
    {
        public int _seedItemId;
        public GameObject[] _stagePrefabs;
        public GameObject _grownPrefab;
        public GameObject _plantedPrefab;
        public Vector3 _offset = new Vector3(0, 0.05f, 0);
    }

    [Header("심기 프리팹")]
    [SerializeField] List<CropVisual> _cropVisuals = new List<CropVisual>();
    [SerializeField] GameObject _defaultPlantedPrefab;

    GameTimeManager _timeManager;
    private readonly HashSet<FarmTile> _plantedTiles = new HashSet<FarmTile>();
    Dictionary<int, CropVisual> _cropVisualById;

    public void Initialize(GameTimeManager timeManager, GridManager gridManager)
    {
        _timeManager = timeManager;

        _timeManager.OnDateChanged += HandleNewDay;

        _cropVisualById = new Dictionary<int, CropVisual>();
        foreach(var v in _cropVisuals)
        {
            if (v == null) continue;
            if (!_cropVisualById.ContainsKey(v._seedItemId))
                _cropVisualById[v._seedItemId] = v;
        }
    }

    void HandleNewDay(int year, Season season, int day)
    {
        if (_plantedTiles.Count == 0) return;

        var snapshot = new List<FarmTile>(_plantedTiles);

        foreach (var tile in snapshot)
        {
            if (tile == null) { _plantedTiles.Remove(tile); continue; }
            if (!tile.HasCrop) { _plantedTiles.Remove(tile); continue; }

            bool grew = tile.TryAdvancedGrowthOneDay();
            if (!grew) continue; // 물 안 줬으면 외형도 그대로

            // 성장 단계에 따른 외형 교체
            if (tile.GrowthStage >= tile.MaxGrowthStage)
            {
                ConvertToGrownCrop(tile);
                // 지금은 완성 후 성장 더 안 하니까 목록에서 제거(재성장 작물 넣을 때는 유지)
                _plantedTiles.Remove(tile);
            }
            else
            {
                SpawnVisualForStage(tile, tile.GrowthStage, tile.CropItemId);
            }
        }
    }

    public bool PlantSeed(FarmTile tile, int seedItemId, int maxGrouthDays = 3)
    {
        if(tile == null) return false;
        if (!tile.TryPlantSeed(seedItemId, maxGrouthDays)) return false;

        _plantedTiles.Add(tile); // ★ 심어진 타일만 관리

        SpawnVisualForStage(tile, stage: 0, seedItemId);
        return true;
    }

    private void SpawnVisualForStage(FarmTile tile, int stage, int seedItemId)
    {
        if (tile.occupant != null)
            Destroy(tile.occupant);

        GameObject prefab = _defaultPlantedPrefab;
        Vector3 offset = Vector3.zero;

        if (_cropVisualById != null && _cropVisualById.TryGetValue(seedItemId, out var v))
        {
            offset = v._offset;

            // 1) stage 프리팹 우선
            if (v._stagePrefabs != null && stage >= 0 && stage < v._stagePrefabs.Length && v._stagePrefabs[stage] != null)
                prefab = v._stagePrefabs[stage];
            // 2) 없으면 기존 plantedPrefab
            else if (v._plantedPrefab != null)
                prefab = v._plantedPrefab;
        }

        if (prefab == null)
        {
            tile.ClearOccupant();
            return;
        }

        var obj = Instantiate(prefab, tile.transform.position + offset, Quaternion.identity);
        tile.SetOccupant(obj);
    }

    private void ConvertToGrownCrop(FarmTile tile)
    {
        int seedItemId = tile.CropItemId;

        GameObject prefab = null;
        Vector3 offset = Vector3.zero;

        if (_cropVisualById != null && _cropVisualById.TryGetValue(seedItemId, out var v))
        {
            offset = v._offset;
            prefab = v._grownPrefab;
            // grownPrefab이 없으면 마지막 stage 프리팹을 대체로 사용 가능
            if (prefab == null && v._stagePrefabs != null && v._stagePrefabs.Length > 0)
                prefab = v._stagePrefabs[^1];
        }

        if (tile.occupant != null)
        {
            Destroy(tile.occupant);
            tile.ClearOccupant();
        }
        
        if (prefab == null)
        {
            Debug.Log("[CropManager] No GrownPrefab Exsist");
            tile.ClearOccupant();
            return;
        }

        var obj = Instantiate(prefab, tile.transform.position + offset, Quaternion.identity);
        tile.SetOccupant(obj);

        var ch = obj.AddComponent<CropHarvestable>();
        // TODO: 여기서 CropHarvestable 연결해주고 작물 아이디 넘겨주면서 외형 + 어떤 작물인지 결정
        var spec = GameInitializer.Instance.Database.GetById(seedItemId + 1);
        ch.Initialize(tile, spec.key);
    }

}
