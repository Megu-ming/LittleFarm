using UnityEngine;

public class TerrainGridManager : MonoBehaviour
{
    [SerializeField] Terrain _terrain;
    [SerializeField] GridManager _gridManager;

    [Header("타일 타입 ↔ Terrain 텍스처 인덱스 매핑")]
    [SerializeField] int _groundTextureIndex = 0;
    [SerializeField] int _tilledTextureIndex = 1;
    [SerializeField] int _wateredTextureIndex = 2;
    [SerializeField] int _pathTextureIndex = 3;
    [SerializeField] int _waterTextureIndex = 4;
    [SerializeField] int _blockedTextureIndex = 5;

    TerrainData _terrainData;
    int _alphaWidth;
    int _alphaHeight;
    int _layersCount;

    public void Initialize()
    {
        _terrainData = _terrain.terrainData;
        _alphaWidth = _terrainData.alphamapWidth;
        _alphaHeight = _terrainData.alphamapHeight;
        _layersCount = _terrainData.alphamapLayers;

        PaintAllFromGrid();

        SubscribeAllTiles();
    }

    private void PaintAllFromGrid()
    {
        var tiles = _gridManager.Tiles;
        int w = tiles.GetLength(0);
        int h = tiles.GetLength(1);

        if (w != _alphaWidth || h != _alphaHeight)
        {
            Debug.LogWarning($"[TerrainGridPainter] Grid({w}x{h})와 Alphamap({_alphaWidth}x{_alphaHeight}) 크기가 다릅니다. 1:1 매핑이 아닙니다.");
        }

        float[,,] alphas = new float[_alphaWidth, _alphaHeight, _layersCount];

        for (int x = 0; x < w; x++)
        {
            for (int z = 0; z < h; z++)
            {
                var tile = tiles[x, z];
                int texIndex = GetTextureIndexFor(tile);

                // 모든 레이어 0으로
                for (int l = 0; l < _layersCount; l++)
                    alphas[z, x, l] = 0f; // [y, x, layer] 순서 주의

                if (texIndex >= 0 && texIndex < _layersCount)
                    alphas[z, x, texIndex] = 1f;
            }
        }
        _terrainData.SetAlphamaps(0, 0, alphas);
    }

    void SubscribeAllTiles()
    {
        var tiles = _gridManager.Tiles;
        int w = tiles.GetLength(0);
        int h = tiles.GetLength(1);

        for (int x = 0; x < w; x++)
        {
            for (int z = 0; z < h; z++)
            {
                var tile = tiles[x, z];
                if (tile == null) continue;

                tile.OnTileTypeChanged += HandleTileTypeChanged;
            }
        }
    }

    void HandleTileTypeChanged(FarmTile tile, TileType newType)
    {
        // 해당 타일 하나만 알파맵 갱신
        var pos = tile.GridPos;
        int gx = pos.x;
        int gz = pos.y;

        int texIndex = GetTextureIndexFor(tile);
        if (gx < 0 || gz < 0 || gx >= _alphaWidth || gz >= _alphaHeight)
            return;

        float[,,] alpha = _terrainData.GetAlphamaps(gx, gz, 1, 1); // 해당 픽셀만 가져오기
        for (int l = 0; l < _layersCount; l++)
            alpha[0, 0, l] = 0f;
        if (texIndex >= 0 && texIndex < _layersCount)
            alpha[0, 0, texIndex] = 1f;

        _terrainData.SetAlphamaps(gx, gz, alpha);
    }

    int GetTextureIndexFor(FarmTile tile)
    {
        if (tile == null) return _groundTextureIndex;

        switch (tile.TileType)
        {
            case TileType.Ground: return _groundTextureIndex;
            case TileType.Tilled: return _tilledTextureIndex;
            case TileType.Watered: return _wateredTextureIndex;
            case TileType.Path: return _pathTextureIndex;
            case TileType.Water: return _waterTextureIndex;
            case TileType.Block: return _blockedTextureIndex;
        }
        return _groundTextureIndex;
    }

    int WorldToAlphaX(Vector3 worldPos)
    {
        var data = _terrain.terrainData;

        // Terrain 로컬 좌표
        Vector3 local = worldPos - _terrain.transform.position;

        // 0~1 정규화
        float nx = Mathf.Clamp01(local.x / data.size.x);

        // 알파맵 인덱스 (0 ~ alphamapWidth-1)
        int ax = Mathf.RoundToInt(nx * (data.alphamapWidth - 1));
        return ax;
    }

    int WorldToAlphaZ(Vector3 worldPos)
    {
        var data = _terrain.terrainData;

        Vector3 local = worldPos - _terrain.transform.position;
        float nz = Mathf.Clamp01(local.z / data.size.z);

        int az = Mathf.RoundToInt(nz * (data.alphamapHeight - 1));
        return az;
    }

}
