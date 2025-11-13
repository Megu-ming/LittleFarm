using UnityEngine;

public class GridMapDesigner : MonoBehaviour
{
    [Header("참조")]
    public GridManager grid;       // Ground에 붙어있는 GridManager

    [Header("배치할 프리팹")]
    public GameObject prefab;      // 나무, 바위, 작물 등

    [Header("배치할 타일 좌표")]
    public int x;
    public int z;

    public float yOffset = 0.5f;   // 타일 위로 얼마나 띄울지

    // 에디터에서 버튼으로 호출할 함수
    public void PlacePrefabOnGrid()
    {
        if (grid == null || prefab == null)
        {
            Debug.LogWarning("Grid 또는 Prefab이 비어 있습니다.");
            return;
        }

        FarmTile tile = grid.GetTile(x, z);
        if (tile == null)
        {
            Debug.LogWarning($"({x}, {z}) 위치에 타일이 없습니다.");
            return;
        }

        Vector3 pos = tile.transform.position;
        pos.y += yOffset;

        if(tile.used == false)
        {
            // 에디터/플레이 둘 다에서 동작하도록 일반 Instantiate 사용
            GameObject obj = Instantiate(prefab, pos, Quaternion.identity, grid.transform);
            obj.name = $"{prefab.name}_({x},{z})";
            tile.used = true;

            Debug.Log($"프리팹 배치: {obj.name} at ({x}, {z})");
        }
        else
        {
            // 그냥 두기
        }
        
    }
}
