using UnityEngine;

public class GridTileDesigner : MonoBehaviour
{
    [Header("참조")]
    public GridManager grid;

    [Header("타일 머티리얼 목록")]
    public Material[] tileMaterials;

    [Header("현재 선택된 머티리얼 인덱스")]
    public int selectedIndex = 0;

    public Material CurrentMaterial
    {
        get
        {
            if (tileMaterials == null || tileMaterials.Length == 0) return null;
            if (selectedIndex < 0 || selectedIndex >= tileMaterials.Length) return null;
            return tileMaterials[selectedIndex];
        }
    }

    public void PaintTile(int gx, int gz)
    {
        if(grid == null)
        {
            Debug.LogWarning("Grid가 비어 있습니다.");
            return;
        }

        Material mat = CurrentMaterial;
        if(mat == null)
        {
            Debug.LogWarning("선택된 머티리얼이 없습니다. 배열과 인덱스를 확인하세요.");
            return;
        }

        FarmTile tile = grid.GetTile(gx, gz);
        if(tile == null)
        {
            Debug.LogWarning($"타일을 찾을 수 없습니다: ({gx}, {gz})");
            return;
        }

        var renderer = tile.GetComponent<MeshRenderer>();
        if(renderer == null)
        {
            Debug.LogWarning($"타일 ({gx}, {gz})에 MeshRenderer가 없습니다.");
            return;
        }

        renderer.sharedMaterial = mat;
    }
}
