using System;
using System.Collections.Generic;
using Unity.VisualScripting;
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
        public SeedToolTarget[] _stagePrefabs;
        public CropHarvestable _grownPrefab;
        public SeedToolTarget _plantedPrefab;
        public Vector3 _offset = new Vector3(0, 0.05f, 0);
    }

    class CropState
    {
        public int _seedItemId;
        public int _stage;
        public int _maxStage;
    }

    private readonly Dictionary<FarmTile, CropState> _crops = new();

    [Header("심기 프리팹")]
    [SerializeField] List<CropVisual> _cropVisuals = new List<CropVisual>();
    [SerializeField] SeedToolTarget _defaultPlantedPrefab;

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
        if (_crops.Count == 0) return;

        var snapshot = new List<KeyValuePair<FarmTile, CropState>>(_crops);

        foreach (var pair in snapshot)
        {
            var tile = pair.Key;
            var state = pair.Value;

            if (tile == null)
            {
                _crops.Remove(tile);
                continue;
            }

            // 수확/파괴 등으로 점유물이 없어졌으면 작물도 제거
            if (tile.occupant == null)
            {
                _crops.Remove(tile);
                continue;
            }

            // 오늘 물 안 줬으면 성장 X
            if (!tile.WasWateredToday)
                continue;

            state._stage++;
            tile.ClearWateredTodayAndResetVisual();

            if (state._stage >= state._maxStage)
            {
                ConvertToGrownCrop(tile, state._seedItemId);
                _crops.Remove(tile);
            }
            else
            {
                SpawnVisualForStage(tile, state._stage, state._seedItemId);
            }
        }
    }

    public bool PlantSeed(FarmTile tile, int seedItemId, int maxGrouthDays = 3)
    {
        if(tile == null) return false;
        if (!tile.CanPlantSeed) return false;

        if (_crops.ContainsKey(tile)) return false;

        _crops[tile] = new CropState
        {
            _seedItemId = seedItemId,
            _stage = 0,
            _maxStage = maxGrouthDays
        };

        SpawnVisualForStage(tile, stage: 0, seedItemId);
        return true;
    }

    private void SpawnVisualForStage(FarmTile tile, int stage, int seedItemId)
    {
        if (tile.occupant != null)
            Destroy(tile.occupant);

        SeedToolTarget prefab = _defaultPlantedPrefab;
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

        //var obj = Instantiate(prefab, tile.transform.position + offset, Quaternion.identity);
        //obj.SetOwner(tile);
        PlacementService.PlaceOnTile(prefab, tile, offset, Quaternion.identity, replaceExisting: true);
    }

    private void ConvertToGrownCrop(FarmTile tile, int seedItemId)
    {
        PlacedObject prefab = null;
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

        var obj = PlacementService.PlaceOnTile(prefab, tile, offset, Quaternion.identity, replaceExisting: true);

        var ch = obj.GetOrAddComponent<CropHarvestable>();
        // TODO: 여기서 CropHarvestable 연결해주고 작물 아이디 넘겨주면서 외형 + 어떤 작물인지 결정
        var spec = GameManager.Instance.DataManager.ItemDatabase.GetById(seedItemId + 1);
        ch.Initialize(tile, spec.key);
    }

}
