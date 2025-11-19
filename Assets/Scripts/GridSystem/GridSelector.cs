using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class GridSelector : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] Camera cam;                // 사용할 카메라
    [SerializeField] GridManager grid;          // 그리드 정보
    [SerializeField] Transform playerTransform; // 플레이어 위치

    [Header("레이어 마스크")]
    [SerializeField] LayerMask tileMask;        // FarmTile이 붙어있는 타일 레이어
    [SerializeField] LayerMask actionMask;      // 행동 레이어
    [SerializeField] LayerMask interactMask;    // 상호작용 레이어

    [Header("타일 하이라이트")]
    [SerializeField] GameObject tileHighlightPrefab;
    [SerializeField] float highlightYPos = 0.05f;

    [Header("타일 범위")]
    [SerializeField] float maxRangeInTiles = 1f;

    FarmTile _currentTile;
    GameObject tileHighlightInstance;
    bool _isTileInRange;

    public FarmTile CurrentTile => _currentTile;
    public bool IsTileInRange => _isTileInRange;

    private void Awake()
    {
        if (cam == null)
            cam = Camera.main;

        if(tileHighlightPrefab!=null)
        {
            tileHighlightInstance = Instantiate(tileHighlightPrefab);
            tileHighlightInstance.SetActive(false);
        }
    }

    private void Update()
    {
        UpdateCurrentTileAndHighlight();
    }

    private void UpdateCurrentTileAndHighlight()
    {
        _currentTile = null;
        _isTileInRange = false;

        if(Mouse.current == null || cam == null || grid == null || playerTransform == null)
        {
            SetHighlightActive(false);
            return;
        }

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = cam.ScreenPointToRay(mousePos);

        if(Physics.Raycast(ray, out RaycastHit hit, 100f, tileMask))
        {
            FarmTile tile = hit.collider.GetComponent<FarmTile>();
            if(tile!=null)
            {
                _currentTile = tile;

                if(grid.WorldToGrid(playerTransform.position, out int px, out int pz))
                {
                    int dx = Mathf.Abs(tile.x - px);
                    int dz = Mathf.Abs(tile.z - pz);
                    int maxDelta = Mathf.Max(dx, dz);

                    _isTileInRange = (maxDelta <= maxRangeInTiles);
                }
                else
                {
                    _isTileInRange = false;
                }

                if(tileHighlightInstance != null)
                {
                    tileHighlightInstance.transform.position =
                        tile.transform.position + Vector3.up * highlightYPos;
                    SetHighlightActive(_isTileInRange);
                }

                return;
            }
        }
    }

    private void SetHighlightActive(bool isActive)
    {
        if (tileHighlightInstance != null)
        {
            tileHighlightInstance.SetActive(isActive);
        }
    }

    /// <summary>
    /// 플레이어 액션용. 선택된 타일이 범위 안에 있으면 그 타일 반환
    /// </summary>
    /// <param name="tile"></param>
    /// <returns></returns>
    public bool TryGetTileForAction(out FarmTile tile)
    {
        tile = (_isTileInRange ? _currentTile : null);
        return tile != null;
    }

    /// <summary>
    /// 도구 사용 대상 찾기
    /// 타일이 범위 안에 있을 것
    /// 타일 occupant 또는 overlapSphere로 IToolTarget탐색
    /// </summary>
    /// <param name="playerPos"></param>
    /// <param name="toolData"></param>
    /// <param name="target"></param>
    /// <param name="hitPoint"></param>
    /// <param name="hitNormal"></param>
    /// <returns></returns>
    public bool TryGetToolTarget(Vector3 playerPos, ToolData toolData, out IToolTarget target, out Vector3 hitPoint, out Vector3 hitNormal)
    {
        target = null;
        hitPoint = default;
        hitNormal = Vector3.up;

        if (!TryGetTileForAction(out FarmTile tile)) return false;

        if(tile.occupant !=null)
        {
            target = tile.occupant.GetComponentInChildren<IToolTarget>();
            if(target!=null)
            {
                hitPoint = tile.occupant.transform.position;
                return true;
            }
        }

        float radius = (grid != null) ? grid.cellSize * 0.5f : 0.5f;
        Vector3 center = tile.transform.position + Vector3.up * 0.5f;

        Collider[] cols = Physics.OverlapSphere(center, radius, actionMask);
        foreach (var col in cols)
        {
            target = col.GetComponentInParent<IToolTarget>();
            if (target != null)
            {
                hitPoint = col.ClosestPoint(center);
                hitNormal = Vector3.up;
                return true;
            }
        }

        return false;
    }

    public bool TryGetInteractTarget(Vector3 playerPos, out IInteractable interactable)
    {
        interactable = null;

        if (!TryGetTileForAction(out FarmTile tile)) return false;

        float radius = (grid != null) ? grid.cellSize * 0.5f : 0.5f;
        Vector3 center = tile.transform.position + Vector3.up * 0.5f;

        Collider[] cols = Physics.OverlapSphere(center, radius, interactMask);
        foreach (var col in cols)
        {
            interactable = col.GetComponentInParent<IInteractable>();
            if (interactable != null)
                return true;
        }

        return false;
    }
}
