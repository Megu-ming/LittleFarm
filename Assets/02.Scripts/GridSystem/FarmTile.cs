using UnityEngine;
using UnityEditor.Experimental.GraphView;
using System;


#if UNITY_EDITOR
using UnityEditor;
#endif

public enum TileType
{
    Ground,     // 경작 가능한 땅
    Tilled,     // 경작한 땅
    Watered,    // 젖은 경작한 땅(물을 준)
    Path,       // 경작 불가능한 땅
    Water,      // 물(강, 호수, 바다)
    Block,      // 막힌 곳
}

[Serializable]
public class FarmTile : MonoBehaviour, IInteractable, IToolTarget
{
    [Header("그리드 좌표")]
    [SerializeField] Vector2Int _gridPos;
    public Vector2Int GridPos => _gridPos;
    public void SetGridPos(int x, int z) => _gridPos = new Vector2Int(x, z);

    [Header("타일 타입")]
    [SerializeField] TileType _tileType = TileType.Ground;
    public TileType TileType
    {
        get => _tileType;
        set
        {
            if(_tileType == value) return;
            _tileType = value;
            OnTileTypeChanged?.Invoke(this, value);
        }
    }

    public event System.Action<FarmTile, TileType> OnTileTypeChanged;

    [SerializeField] bool _isTilled = false;

    [Header("점유 상태")]
    public bool used = false;
    public GameObject occupant;

    [SerializeField] bool _wateredToday = false;// 오늘 물 줬는지

    public bool IsTilled => _isTilled;
    public bool CanBeTilled => _tileType == TileType.Ground || _tileType == TileType.Tilled;
    public bool CanPlantSeed => _isTilled && occupant == null;
    public bool WasWateredToday => _wateredToday;
    public bool IsWalkable => _tileType != TileType.Block && _tileType != TileType.Water;

    // 점유자 설정
    public void SetOccupant(GameObject obj)
    {
        occupant = obj;
        used = (obj != null);
    }
    // 점유자 해제
    public void ClearOccupant()
    {
        //if(occupant != null)
        //    DestroyImmediate(occupant);
        used = false;
        occupant = null;
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
        string layerName = "Tile";

        string tagName = _tileType switch
        {
            TileType.Ground => "GroundTile",
            TileType.Tilled => "TilledTile",
            TileType.Watered => "WateredTile",
            TileType.Water => "WaterTile",
            TileType.Block => "BlockTile",
            TileType.Path => "PathTile",
            _ => "GroundTile"
        };

        // 레이어 적용
        int layerIndex = LayerMask.NameToLayer(layerName);
        if(layerIndex == -1)
        {
            Debug.LogWarning($"[FarmTile] Layer '{layerName}'가 정의되어 있지 않습니다. (Tile {_gridPos.x},{_gridPos.y})");
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
            Debug.LogWarning($"[FarmTile] Tag '{tagName}' 가 정의되어 있지 않습니다. (Tile {_gridPos.x} , {_gridPos.y})");
        }
    }

    /// <summary>
    /// 액션 들어왔을 때 호출
    /// 호미면 경작지로 변경/ 곡괭이면 만약 경작지면 원래대로 돌림
    /// 타일 위에 점유자가 있으면 Action양도
    /// </summary>
    /// <param name="context"></param>
    public void OnToolAction(ToolActionContext context)
    {
        // 뭔가 설치되어있지 않을 때
        if(occupant == null)
        {
            if (context.toolType == ToolType.Hoe)
                TillSoil();
            else if (context.toolType == ToolType.Pickaxe)
                ReturnSoil();
            else if (context.toolType == ToolType.WateringCan)
                WaterSoil();
        }
        // 뭔가 설치되어 있음
        else
        {
            if(occupant.TryGetComponent<SeedToolTarget>(out var seed))
            {
                if (context.toolType == ToolType.WateringCan)
                { 
                    WaterSoil();
                    return;
                }
            }
            var target = occupant.GetComponent<IToolTarget>();
            if(target != null)
                target.OnToolAction(context);
        }
    }

    void TillSoil()
    {
        TileType = TileType.Tilled;
        _isTilled = true;
        Debug.Log($"[FarmTile]{name} 경작 완료");
    }

    void ReturnSoil()
    {
        if(occupant != null)
        {
            Debug.Log("곡괭이로 제거 가능한 경우 점유물 제거");
            return;
        }

        _isTilled = false;
        TileType = TileType.Ground;

        Debug.Log($"[FarmTile]{name} 되돌리기 완료");
    }

    void WaterSoil()
    {
        if (_tileType == TileType.Tilled)
        {
            TileType = TileType.Watered;
            _wateredToday = true;
            _isTilled = true;
            Debug.Log($"[FarmTile]{name} 물주기 완료");
        }
    }

    public void Interact(PlayerInteraction interactor)
    {
        //if(CanPlantSeed && !_hasCrop)
        //    TryPlantSeed(3001, 3);
    }

    public void ClearWateredTodayAndResetVisual()
    {
        _wateredToday = false;
        if (TileType == TileType.Watered)
            TileType = TileType.Tilled;
    }

    public void ClearOccupantIf(GameObject expectedOccupant)
    {
        if (occupant == expectedOccupant)
            ClearOccupant();
    }
}   
