using UnityEngine;

[CreateAssetMenu(fileName = "ToolData", menuName = "Game/Tool Data")]
public class ToolData : ScriptableObject
{
    [Header("기본 정보")]
    [SerializeField] string _id;
    [SerializeField] string _displayName;
    [SerializeField] Sprite _icon;

    [Header("도구 타입")]
    [SerializeField] GameObject _toolPrefab;

    [Header("도구 타입")]
    [SerializeField] ToolType _toolType = ToolType.None;

    [Header("기본 파워")]
    [SerializeField] float _power = 1f;

    [Header("행동 범위")]
    [SerializeField] Vector2Int _areaSize = Vector2Int.zero;

    // 읽기 전용 프로퍼티
    public string Id => _id;
    public string DisplayName => _displayName;
    public Sprite Icon => _icon;

    public GameObject ToolPrefab => _toolPrefab;

    public ToolType ToolType => _toolType;
    public float Power => _power;
    public Vector2Int AreaSize => _areaSize;
}
