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

public class FarmTile : MonoBehaviour, IInteractable
{
    [Header("그리드 좌표")]
    public int x;
    public int z;

    [Header("타일 타입")]
    public TileType tileType = TileType.Ground;
    [SerializeField] bool _isTilled = false;

    [Header("점유 상태")]
    public bool used = false;
    public GameObject occupant;

    [Header("작물 상태")]
    [SerializeField] bool _hasCrop = false;     // 이 타일에 작물이 있는가
    [SerializeField] int _cropItemId = -1;      // 어떤 씨앗/작물인지 (ItemId)
    [SerializeField] int _growthStage = 0;      // 성장 단계 (0 = 심은 직후)
    [SerializeField] int _maxGrowthStage = 3;   // 최종 단계 (나중에 데이터화 예정)
    [SerializeField] bool _wateredToday = false;// 오늘 물 줬는지

    public bool IsTilled => _isTilled;
    public bool HasCrop => _hasCrop;
    public bool CanBeTilled => !_isTilled && (tileType == TileType.Ground);
    public bool CanPlantSeed => _isTilled && !_hasCrop;
    public bool IsWalkable => tileType != TileType.Block && tileType != TileType.Water;

    // 점유자 설정
    public void SetOccupant(GameObject obj)
    {
        occupant = obj;
        used = (obj != null);
    }
    // 점유자 해제
    public void ClearOccupant()
    {
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

    /// <summary>
    /// 액션 들어왔을 때 호출
    /// 호미면 경작지로 변경/ 곡괭이면 만약 경작지면 원래대로 돌림
    /// 타일 위에 점유자가 있으면 Action양도
    /// </summary>
    /// <param name="context"></param>
    public void OnToolAction(ToolActionContext context)
    {
        if(occupant == null)
        {
            if (!CanBeTilled)
                return;

            if (context.toolType == ToolType.Hoe)
                TillSoil();
            else if (context.toolType == ToolType.Pickaxe)
                ReturnSoil();
        }
        else
        {
            var target = occupant.GetComponent<IToolTarget>();
            target.OnToolAction(context);
        }
    }

    void TillSoil()
    {
        _isTilled = true;
        tileType = TileType.Soil;

        Debug.Log($"[FarmTile]{name}경작 완료");
    }

    void ReturnSoil()
    {
        _isTilled = false;
        tileType = TileType.Ground;

        Debug.Log($"[FarmTile]{name} 되돌리기 완료");
    }

    public bool TryPlantSeed(int itemId, int maxGrowthStage)
    {
        if (!CanPlantSeed) return false;

        _hasCrop = true;
        _cropItemId = itemId;
        _growthStage = 0;
        _maxGrowthStage = maxGrowthStage;

        // TODO: 여기서 "씨앗 프리팹"을 스폰하거나, 타일 비주얼을 '씨앗 심은 모양'으로 변경
        Debug.Log($"[FarmTile] {name} 씨앗 심기 완료 (itemId={itemId})");

        return true;
    }

    public void AdvancedGrowthOneDay()
    {
        if (!_hasCrop) return;
        if (_growthStage >= _maxGrowthStage) return;

        if(_wateredToday)
            _growthStage++;

        // TODO:
        // 작물 성장 이벤트 호출
        Debug.Log($"[FarmTile] {name} 성장 단계 : {_growthStage}/{_maxGrowthStage}");
    }

    public void Interact(PlayerInteraction interactor)
    {
        if(CanPlantSeed && !_hasCrop)
            TryPlantSeed(3001, 3);
    }
}   
