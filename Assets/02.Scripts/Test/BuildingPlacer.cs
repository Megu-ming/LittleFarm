using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BuildingPlacer : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private GridSelector _gridSelector;
    [SerializeField] private GridManager _grid;

    [Header("Preview")]
    [SerializeField] private GameObject _footprintHighlightPrefab;
    [SerializeField] private float _highlightY = 0.11f;

    [SerializeField] private Color _validTint = new Color(0.2f, 1f, 0.2f, 0.45f);
    [SerializeField] private Color _invalidTint = new Color(1f, 0.2f, 0.2f, 0.45f);

    [Header("Selected Building")]
    [SerializeField] private BuildingData _selected;

    bool _buildMode;

    GameObject _ghost;
    GameObject _ghostPrefabRef;
    Renderer[] _ghostRenderers;
    MaterialPropertyBlock _mpb;

    readonly List<GameObject> _highlightPool = new();
    FarmTile[] _lastFootprint;
    bool _lastCanPlace;

    public void Initialize(GridSelector gridSelector, GridManager grid)
    {
        _gridSelector = gridSelector;
        _grid = grid;
        _mpb = new MaterialPropertyBlock();
    }

    // 예: B키로 건설 모드 토글
    public void OnToggleBuildMode(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        SetBuildMode(!_buildMode);
    }

    // 예: ESC로 취소
    public void OnCancelBuildMode(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        SetBuildMode(false);
    }

    // 예: 좌클릭으로 설치
    public void OnPlaceBuilding(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (!_buildMode) return;

        TryPlaceSelectedOnCurrentTile();
    }

    void Update()
    {
        if (!_buildMode)
            return;

        UpdatePreview();
    }

    void SetBuildMode(bool on)
    {
        _buildMode = on;

        if (_gridSelector != null)
            _gridSelector.SetFocus(on);

        if (!on)
        {
            SetGhostActive(false);
            SetHighlightsActive(false);
            _lastFootprint = null;
        }
    }

    void UpdatePreview()
    {
        if (_selected == null || _gridSelector == null || _grid == null)
        {
            SetGhostActive(false);
            SetHighlightsActive(false);
            return;
        }

        FarmTile origin = _gridSelector.CurrentTile;
        if (origin == null)
        {
            SetGhostActive(false);
            SetHighlightsActive(false);
            return;
        }

        int ox = origin.GridPos.x;
        int oz = origin.GridPos.y;

        if (!TryGetFootprintTiles(ox, oz, _selected.Width, _selected.Height, out var footprint))
        {
            SetGhostActive(false);
            SetHighlightsActive(false);
            return;
        }

        bool canPlace = CanPlaceFootprint(footprint);

        EnsureGhost(_selected.Prefab);
        PositionGhost(origin, _selected.Width, _selected.Height, _selected.ExtraOffset);
        TintGhost(canPlace ? _validTint : _invalidTint);
        SetGhostActive(true);

        UpdateFootprintHighlights(footprint, canPlace);

        _lastFootprint = footprint;
        _lastCanPlace = canPlace;
    }

    bool TryPlaceSelectedOnCurrentTile()
    {
        if (_selected == null || _gridSelector == null || _grid == null)
            return false;

        FarmTile origin = _gridSelector.CurrentTile;
        if (origin == null)
            return false;

        int ox = origin.GridPos.x;
        int oz = origin.GridPos.y;

        if (!TryGetFootprintTiles(ox, oz, _selected.Width, _selected.Height, out var footprint))
            return false;

        if (!CanPlaceFootprint(footprint))
            return false;

        // 중심 오프셋 (cellSize 반영)
        Vector3 centerOffset = new Vector3((_selected.Width - 1) * 0.5f * _grid.cellSize, 0f,
                                           (_selected.Height - 1) * 0.5f * _grid.cellSize);
        Vector3 offset = centerOffset + _selected.ExtraOffset;

        PlacementService.PlaceFootprint(
            _selected.Prefab,
            origin,
            footprint,
            offset,
            Quaternion.identity,
            replaceExisting: false
        );

        return true;
    }

    bool TryGetFootprintTiles(int originX, int originZ, int w, int h, out FarmTile[] result)
    {
        result = null;

        if (originX < 0 || originZ < 0) return false;
        if (originX + w - 1 >= _grid.width) return false;
        if (originZ + h - 1 >= _grid.height) return false;

        var list = new List<FarmTile>(w * h);
        for (int z = 0; z < h; z++)
        {
            for (int x = 0; x < w; x++)
            {
                var t = _grid.GetTile(originX + x, originZ + z);
                if (t == null)
                    return false;

                list.Add(t);
            }
        }

        result = list.ToArray();
        return true;
    }

    bool CanPlaceFootprint(FarmTile[] tiles)
    {
        for (int i = 0; i < tiles.Length; i++)
        {
            var t = tiles[i];
            if (t == null) return false;

            if (t.occupant != null) return false;
            if (t.TileType == TileType.Water || t.TileType == TileType.Block) return false;
        }
        return true;
    }

    void EnsureGhost(GameObject prefab)
    {
        if (prefab == null)
            return;

        if (_ghost != null && _ghostPrefabRef == prefab)
            return;

        if (_ghost != null)
            Destroy(_ghost);

        _ghostPrefabRef = prefab;
        _ghost = Instantiate(prefab);
        _ghost.name = $"[Ghost]{prefab.name}";

        // 고스트는 게임 로직/점유/충돌 영향 없게
        DisableAllBehaviours(_ghost);
        DisableAllColliders(_ghost);

        _ghostRenderers = _ghost.GetComponentsInChildren<Renderer>(true);
        if (_mpb == null) _mpb = new MaterialPropertyBlock();
    }

    void PositionGhost(FarmTile origin, int w, int h, Vector3 extraOffset)
    {
        // origin은 "좌하단 타일" 기준 (GridManager 타일이 x+0.5, z+0.5로 생성됨)
        Vector3 centerOffset = new Vector3((w - 1) * 0.5f * _grid.cellSize, 0f,
                                           (h - 1) * 0.5f * _grid.cellSize);
        _ghost.transform.position = origin.transform.position + centerOffset + extraOffset;
        _ghost.transform.rotation = Quaternion.identity;
    }

    void TintGhost(Color tint)
    {
        if (_ghostRenderers == null) return;

        for (int i = 0; i < _ghostRenderers.Length; i++)
        {
            var r = _ghostRenderers[i];
            if (r == null) continue;

            r.GetPropertyBlock(_mpb);

            // URP Lit: _BaseColor, Legacy: _Color 둘 다 세팅
            _mpb.SetColor("_BaseColor", tint);
            _mpb.SetColor("_Color", tint);

            r.SetPropertyBlock(_mpb);
        }
    }

    void UpdateFootprintHighlights(FarmTile[] tiles, bool canPlace)
    {
        if (_footprintHighlightPrefab == null)
            return;

        EnsureHighlightPool(tiles.Length);

        Color tint = canPlace ? _validTint : _invalidTint;

        for (int i = 0; i < _highlightPool.Count; i++)
        {
            bool active = i < tiles.Length;
            var go = _highlightPool[i];
            if (go == null) continue;

            go.SetActive(active);

            if (!active) continue;

            var tile = tiles[i];
            go.transform.position = tile.transform.position + Vector3.up * _highlightY;

            // 하이라이트도 tint 적용(머티리얼이 Color/_BaseColor를 지원하면 반영됨)
            var rend = go.GetComponentInChildren<Renderer>();
            if (rend != null)
            {
                rend.GetPropertyBlock(_mpb);
                _mpb.SetColor("_BaseColor", tint);
                _mpb.SetColor("_Color", tint);
                rend.SetPropertyBlock(_mpb);
            }
        }
    }

    void EnsureHighlightPool(int count)
    {
        while (_highlightPool.Count < count)
        {
            var inst = Instantiate(_footprintHighlightPrefab);
            inst.SetActive(false);
            _highlightPool.Add(inst);
        }
    }

    void SetGhostActive(bool on)
    {
        if (_ghost != null)
            _ghost.SetActive(on);
    }

    void SetHighlightsActive(bool on)
    {
        for (int i = 0; i < _highlightPool.Count; i++)
        {
            if (_highlightPool[i] != null)
                _highlightPool[i].SetActive(on);
        }
    }

    static void DisableAllBehaviours(GameObject root)
    {
        // preview에서는 모든 MonoBehaviour 비활성(PlacedObject 포함)
        var behaviours = root.GetComponentsInChildren<MonoBehaviour>(true);
        for (int i = 0; i < behaviours.Length; i++)
        {
            // BuildingPlacer 자신은 제외(ghost에 붙어있을 가능성은 거의 없지만 안전)
            if (behaviours[i] == null) continue;
            behaviours[i].enabled = false;
        }
    }

    static void DisableAllColliders(GameObject root)
    {
        var cols = root.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < cols.Length; i++)
            cols[i].enabled = false;
    }
}
