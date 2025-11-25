using Unity.Mathematics;
using UnityEngine;

[ExecuteAlways]
public class GridManager : MonoBehaviour
{
    [Header("타일 프리팹")]
    public GameObject tilePrefab;   // Tile_Cell 프리팹

    [Header("그리드 크기")]
    public int width = 10;          // X 방향 칸 수
    public int height = 10;         // Z 방향 칸 수

    [Header("칸 간격")]
    public float cellSize = 1f;     // 칸 하나의 크기 (유닛)

    // 생성한 타일들 저장
    FarmTile[,] tiles;

    private void OnEnable()
    {
        var existingTiles = GetComponentsInChildren<FarmTile>();

        if (Application.isPlaying)
            RefreshTilesFromScene(existingTiles);
        else
        {
            if(existingTiles == null || existingTiles.Length == 0)
                GenerateGrid();
            else
                RefreshTilesFromScene(existingTiles);
        }    

    }

    public void GenerateGrid()
    {
        if (tilePrefab == null)
            return;

        // 1) 기존 타일 정리 (FarmTile 붙은 자식만 삭제)
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            if (child.GetComponent<FarmTile>() != null)
            {
                if (Application.isPlaying)
                    Destroy(child.gameObject);
                else
                    DestroyImmediate(child.gameObject);
            }
        }

        // 2) 배열 새로 만들기
        tiles = new FarmTile[width, height];

        float offsetX = -(width - 1) * 0.5f * cellSize;
        float offsetZ = -(height - 1) * 0.5f * cellSize;

        for (int z = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector3 pos = new Vector3(
                    transform.position.x + offsetX + x * cellSize,
                    transform.position.y + 0.01f,
                    transform.position.z + offsetZ + z * cellSize
                );

                GameObject tileObj = (GameObject)Instantiate(
                    tilePrefab, pos, tilePrefab.transform.rotation, transform
                );
                tileObj.name = $"Tile_{x}_{z}";

                FarmTile farmTile = tileObj.GetComponent<FarmTile>();
                if (farmTile != null)
                {
                    farmTile.x = x;
                    farmTile.z = z;
                    tiles[x, z] = farmTile;
                }
            }
        }
    }

    private void RefreshTilesFromScene(FarmTile[] foundTiles = null)
    {
        if(foundTiles == null || foundTiles.Length == 0)
            foundTiles = GetComponentsInChildren<FarmTile>();

        tiles = new FarmTile[width, height];

        foreach(FarmTile tile in foundTiles)
        {
            int x = tile.x;
            int z = tile.z;

            if (x < 0 || x >= width || z < 0 || z >= height)
                continue;

            tiles[x, z] = tile;
        }
    }

    public FarmTile GetTile(int x, int z)
    {
        if (x < 0 || x >= width || z < 0 || z >= height)
            return null;

        return tiles[x, z];
    }

    public bool WorldToGrid(Vector3 worldPos, out int x, out int z)
    {
        // GenerateGrid에서 사용한 것과 같은 offset 계산
        float offsetX = -(width - 1) * 0.5f * cellSize;
        float offsetZ = -(height - 1) * 0.5f * cellSize;

        // 그리드 중심(Ground.position)을 기준으로 로컬 좌표로 변환
        float localX = worldPos.x - transform.position.x;
        float localZ = worldPos.z - transform.position.z;

        float fx = (localX - offsetX) / cellSize;
        float fz = (localZ - offsetZ) / cellSize;

        x = Mathf.RoundToInt(fx);
        z = Mathf.RoundToInt(fz);

        if (x < 0 || x >= width || z < 0 || z >= height)
            return false;

        return true;
    }
}
