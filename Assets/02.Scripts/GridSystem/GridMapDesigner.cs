using UnityEngine;

public class GridMapDesigner : MonoBehaviour
{
    [Header("참조")]
    public GridManager grid;       // Ground에 붙어있는 GridManager

    [Header("배치할 프리팹")]
    public GameObject[] prefabs;      // 나무, 바위, 작물 등

    [Header("배치할 프리팹 인덱스")]
    public int selectedIndex = 0;

    [Header("배치할 타일 좌표")]
    public int x;
    public int z;

    public float yOffset = 0.5f;   // 타일 위로 얼마나 띄울지

    public GameObject CurrentPrefab
    {
        get 
        {
            if(prefabs == null || prefabs.Length == 0) return null;
            if (selectedIndex < 0 || selectedIndex >= prefabs.Length) return null;
            return prefabs[selectedIndex];
        }
    }

    // 에디터에서 버튼으로 호출할 함수
    public void PlacePrefabOnGrid()
    {
        PlacePrefabOnGridAt(x, z);
    }

    public void PlacePrefabOnGridAt(int gx, int gz)
    {
        if (grid == null)
        {
            Debug.LogWarning("Grid가 비어 있습니다.");
            return;
        }

        GameObject prefab = CurrentPrefab;
        if(prefab == null)
        {
            Debug.LogWarning("선택된 프리팹이 없습니다. 배열과 인덱스를 확인하세요.");
            return;
        }

        FarmTile tile = grid.GetTile(gx, gz);
        if (tile == null)
        {
            Debug.LogWarning($"({gx}, {gz}) 위치에 타일이 없습니다.");
            return;
        }

        if(tile.used)
        {
            Debug.LogWarning($"({gx}, {gz}) 위치의 타일은 이미 사용 중입니다.");
            return;
        }

        Vector3 pos = tile.transform.position;
        pos.y += yOffset;

        // 에디터/플레이 둘 다에서 동작하도록 일반 Instantiate 사용
        GameObject obj = Instantiate(prefab, pos, Quaternion.identity, grid.transform);
        obj.name = $"{prefab.name}_({gx},{gz})";
        
        PlacedObject marker = obj.GetComponent<PlacedObject>();
        if(marker == null)
            marker = obj.AddComponent<PlacedObject>();

        marker.ownerTile = tile;
        tile.SetOccupant(obj);
    }

    public void EraseAt(int gx, int gz)
    {
        if (grid == null)
        {
            Debug.LogWarning("Grid가 비어 있습니다.");
            return;
        }
        FarmTile tile = grid.GetTile(gx, gz);
        if (tile == null)
        {
            Debug.LogWarning($"({gx}, {gz}) 위치에 타일이 없습니다.");
            return;
        }
        if (!tile.used)
        {
            Debug.LogWarning($"({gx}, {gz}) 위치의 타일은 비어 있습니다.");
            return;
        }
        if(tile.occupant == null)
        {
            Debug.Log($"타일 ({gx}, {gz}에는 오브젝트가 없습니다.)");
            tile.ClearOccupant();
            return;
        }

        GameObject obj = tile.occupant;
        tile.ClearOccupant();

        if(Application.isPlaying)
            Destroy(obj);
        else
            DestroyImmediate(obj);

        Debug.Log($"타일 ({gx}, {gz})의 오브젝트 삭제");
    }
}
