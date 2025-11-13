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

    [Header("테스트용 오브젝트")]
    public GameObject testPrefab;
    public int testX = 3;
    public int testZ = 5;

    // 생성한 타일들 저장
    FarmTile[,] tiles;

    private void OnEnable()
    {
        GenerateGrid();
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) == true)
            BuildCube();
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


    public FarmTile GetTile(int x, int z)
    {
        if (x < 0 || x >= width || z < 0 || z >= height)
            return null;

        return tiles[x, z];
    }

    // 테스트용
    void BuildCube()
    {
        if (testPrefab == null) return;
        testX = Random.Range(0, width);
        testZ = Random.Range(0, height);
        FarmTile tile = GetTile(testX, testZ);
        if (tile == null) return;
        if (tile.used) return;

        Vector3 pos = tile.transform.position;
        pos.y += 0.5f; // 타일 위로 살짝 올리기 (큐브 반 높이)

        Instantiate(testPrefab, pos, Quaternion.identity);
        tile.used = true;
    }
}
