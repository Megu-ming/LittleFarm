using UnityEditor;
using UnityEngine;

[System.Serializable]
public class TilePaintPreset
{
    public string name;
    public Material material;
    public TileType tileType;
}

[System.Serializable]
public class AutoTileRule
{
    public string name;       // 에디터에서 보기용

    public TileType tileType; // 어떤 타입에 적용할지 (Soil/Path 등)
    public int mask;          // autoMask 값 (Up=1, Right=2, Down=4, Left=8 비트 합)

    public Material material; // 이 조건일 때 쓸 머티리얼
}

public class GridTileDesigner : MonoBehaviour
{
    [Header("참조")]
    public GridManager grid;

    [Header("타일 브러시 프리셋 목록")]
    public TilePaintPreset[] presets;

    [Header("현재 선택된 머티리얼 인덱스")]
    public int selectedIndex = 0;

    [Header("오토타일 룰 목록")]
    public AutoTileRule[] autoTileRules;

    public TilePaintPreset CurrentPreset
    {
        get
        {
            if (presets == null || presets.Length == 0) return null;
            if (selectedIndex < 0 || selectedIndex >= presets.Length) return null;
            return presets[selectedIndex];
        }
    }

    public void PaintTile(int gx, int gz)
    {
        if(grid == null)
        {
            Debug.LogWarning("Grid가 비어 있습니다.");
            return;
        }

        TilePaintPreset preset = CurrentPreset;
        if(preset == null)
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

        // 머티리얼 교체
        var renderer = tile.GetComponent<MeshRenderer>();
        if(renderer == null)
        {
            Debug.LogWarning($"타일 ({gx}, {gz})에 MeshRenderer가 없습니다.");
            return;
        }
        else
        {
            renderer.sharedMaterial = preset.material;
        }

        // 타일 타입 변경
        tile.tileType = preset.tileType;

        // 레이어/태그 동기화
        tile.SyncUnityMeta();

        // 오토타일 데이터 갱신
        grid.UpdateAutoTileAround(gx, gz);

        // 오토타일 비주얼 갱신
        UpdateAutoTileVisualAround(gx, gz);
    }

    private AutoTileRule FindRule(TileType type, int mask)
    {
        if (autoTileRules == null) return null;

        for(int i=0;i<autoTileRules.Length;i++)
        {
            var rule = autoTileRules[i];
            if(rule == null) continue;

            if(rule.tileType == type && rule.mask == mask)
                return rule;
        }

        return null;
    }

    private void ApplyAutoTileVisual(FarmTile tile)
    {
        if (tile == null) return;

        AutoTileRule rule = FindRule(tile.tileType, tile.autoMask);

        if (rule == null || rule.material == null) return;

        var renderer = tile.GetComponent<MeshRenderer>();
        if (renderer == null) return;

        renderer.sharedMaterial = rule.material;
    }

    private void UpdateAutoTileVisualAround(int gx, int gz)
    {
        var center = grid.GetTile(gx, gz);
        ApplyAutoTileVisual(center);

        var up = grid.GetTile(gx, gz + 1);
        ApplyAutoTileVisual(up);

        var down = grid.GetTile(gx, gz - 1);
        ApplyAutoTileVisual(down);

        var left = grid.GetTile(gx - 1, gz);
        ApplyAutoTileVisual(left);

        var right = grid.GetTile(gx + 1, gz);
        ApplyAutoTileVisual(right);
    }
}
