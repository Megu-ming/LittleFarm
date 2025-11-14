using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum TileType
{
    Ground,
    Water,
    Block,
    Soil,
    Path,
}

public class FarmTile : MonoBehaviour
{
    [Header("그리드 좌표")]
    public int x;
    public int z;

    [Header("타일 타입")]
    public TileType tileType = TileType.Ground;

    [Header("오토타일용 이웃 마스크(디버그용)")]
    [Tooltip("Up=1, Right=2, Down=4, Left=8 비트 마스크")]
    public int autoMask;

    [Header("점유 상태")]
    public bool used = false;
    public GameObject occupant;

    // 점유자 설정
    public void SetOccupant(GameObject obj)
    {
        occupant = obj;
        used = (obj != null);
    }
    // 점유자 해제
    public void ClearOccupant()
    {
        Debug.Log($"({x}, {z}) 타일 프리팹 삭제");
        used = false;
        occupant = null;
    }

    public bool IsWalkable
    {
        get
        {
            switch(tileType)
            {
                case TileType.Ground:
                case TileType.Soil:
                case TileType.Path:
                    return true;
                case TileType.Water:
                case TileType.Block:
                default:
                    return false;
            }
        }
    }

    public bool IsPlantable
    {
        get
        {
            return tileType == TileType.Soil;
        }
    }

    private void OnValidate()
    {
#if UNITY_EDITOR
        // 값이 바뀔 때 자동으로 레이어/태그 동기화
        if(!Application.isPlaying)
        {
            EditorApplication.delayCall += () =>
            {
                if (this == null) return;
                SyncUnityMeta();
            };
        }
#endif
    }

    public void SyncUnityMeta()
    {
        string layerName = tileType switch
        {
            TileType.Ground => "Tile_Ground",
            TileType.Water => "Tile_Water",
            TileType.Block => "Tile_Block",
            TileType.Soil => "Tile_Soil",
            TileType.Path => "Tile_Path",
            _ => "Tile_Ground",
        };

        // 2) 타일 타입 → 태그 이름 매핑
        string tagName = tileType switch
        {
            TileType.Ground => "GroundTile",
            TileType.Water => "WaterTile",
            TileType.Block => "BlockTile",
            TileType.Soil => "SoilTile",
            TileType.Path => "PathTile",
            _ => "GroundTile"
        };

        // 레이어 적용
        int layerIndex = LayerMask.NameToLayer(layerName);
        if(layerIndex == -1)
        {
            Debug.LogWarning($"[FarmTile] Layer '{layerName}'가 정의되어 있지 않습니다. (Tile {x},{z})");
        }
        else
        {
            gameObject.layer = layerIndex;
        }

        try
        {
            gameObject.tag = tagName;
        }
        catch
        {
            Debug.LogWarning($"[FarmTile] Tag '{tagName}' 가 정의되어 있지 않습니다. (Tile {x},{z})");
        }
    }
}   
