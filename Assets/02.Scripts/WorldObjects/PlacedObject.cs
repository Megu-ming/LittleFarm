using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class PlacedObject : MonoBehaviour
{
    public enum AnchorMode
    {
        BottomLeft, // transform.position이 footprint의 좌하단 타일 중심이라고 가정
        Center      // transform.position이 footprint의 중심(가운데 타일)이라고 가정
    }

    [SerializeField] FarmTile _ownerTile;
    [SerializeField] FarmTile[] _occupiedTiles;

    [Header("Auto Bind (Editor)")]
    [SerializeField] bool _autoBindInEditor = true;
    [SerializeField] GridManager _gridOverride;
    [SerializeField] Vector2Int _footprintSize = Vector2Int.one; // 집이면 (예: 4,3)
    [SerializeField] AnchorMode _anchor = AnchorMode.Center;

#if UNITY_EDITOR
    bool _rebuildScheduled;
#endif

    public Vector2Int Size => _footprintSize;
    public FarmTile OwnerTile => _ownerTile;
    public FarmTile[] OccupiedTiles => _occupiedTiles;

    public void SetOwner(FarmTile ownerTile)
    {
        _ownerTile = ownerTile;
        _occupiedTiles = null;
        RegisterOccupancy();
    }

    public void SetOccupiedTiles(FarmTile origin, FarmTile[] tiles)
    {
        _ownerTile = origin;
        _occupiedTiles = tiles;
        RegisterOccupancy();
    }

    void OnEnable()
    {
        if (Application.isPlaying)
        {
            RegisterOccupancy();
            return;
        }

        if (_autoBindInEditor)
            ScheduleRebuildFromTransform();
        else
            RegisterOccupancy();
    }

    void OnValidate()
    {
        if (Application.isPlaying) return;
        if (_autoBindInEditor)
            ScheduleRebuildFromTransform();
    }

    void Update()
    {
        if (Application.isPlaying) return;
        if (!_autoBindInEditor) return;

        // 에디터에서 이동/회전/스케일 변경 감지
        if (transform.hasChanged)
        {
            transform.hasChanged = false;
            ScheduleRebuildFromTransform();
        }
    }

    void ScheduleRebuildFromTransform()
    {
#if UNITY_EDITOR
        if (_rebuildScheduled) return;
        _rebuildScheduled = true;

        UnityEditor.EditorApplication.delayCall += () =>
        {
            _rebuildScheduled = false;
            if (this == null) return;
            RebuildOccupiedTilesFromTransform();
        };
#else
        RebuildOccupiedTilesFromTransform();
#endif
    }

    void RebuildOccupiedTilesFromTransform()
    {
        GridManager grid = _gridOverride != null ? _gridOverride : FindAnyObjectByType<GridManager>();
        if (grid == null) return;

        // 그리드 좌표(앵커 타일) 계산
        if (!grid.WorldToGrid(transform.position, out int ax, out int az))
        {
            // 그리드 밖이면 기존 점유만 해제
            ClearPreviousOccupancy();
            _ownerTile = null;
            _occupiedTiles = null;
            return;
        }

        int w = Mathf.Max(1, _footprintSize.x);
        int h = Mathf.Max(1, _footprintSize.y);

        int ox = ax;
        int oz = az;

        if (_anchor == AnchorMode.Center)
        {
            // 중심 기준 -> 좌하단(origin) 환산
            ox = ax - (w / 2);
            oz = az - (h / 2);
        }

        // 기존 점유 해제
        ClearPreviousOccupancy();

        // 새 footprint 구성
        var list = new List<FarmTile>(w * h);
        for (int z = 0; z < h; z++)
        {
            for (int x = 0; x < w; x++)
            {
                var t = grid.GetTile(ox + x, oz + z);
                if (t == null)
                {
                    // 범위 밖이면 등록 포기(부분 점유 방지)
                    _ownerTile = null;
                    _occupiedTiles = null;
                    return;
                }
                list.Add(t);
            }
        }

        _occupiedTiles = list.ToArray();
        _ownerTile = (_anchor == AnchorMode.Center) ? grid.GetTile(ax, az) : grid.GetTile(ox, oz);

        RegisterOccupancy();
    }

    void RegisterOccupancy()
    {
        // 멀티가 있으면 멀티 우선
        if (_occupiedTiles != null && _occupiedTiles.Length > 0)
        {
            for (int i = 0; i < _occupiedTiles.Length; i++)
                TryRegisterToTile(_occupiedTiles[i]);
            return;
        }

        // 멀티가 없으면 단일
        TryRegisterToTile(_ownerTile);
    }

    void TryRegisterToTile(FarmTile tile)
    {
        if (tile == null) return;

        // 이미 다른 점유자가 있으면 덮어쓰지 않음
        if (tile.occupant != null && tile.occupant != gameObject)
            return;

        tile.SetOccupant(gameObject);
    }

    void ClearPreviousOccupancy()
    {
        if (_occupiedTiles != null && _occupiedTiles.Length > 0)
        {
            for (int i = 0; i < _occupiedTiles.Length; i++)
            {
                var t = _occupiedTiles[i];
                if (t != null)
                    t.ClearOccupantIf(gameObject);
            }
        }
        else if (_ownerTile != null)
        {
            _ownerTile.ClearOccupantIf(gameObject);
        }
    }

    void OnDestroy()
    {
        ClearPreviousOccupancy();
    }
}
