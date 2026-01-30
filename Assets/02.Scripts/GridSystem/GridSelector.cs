using Unity.Collections;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class GridSelector : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] Camera cam;                // 사용할 카메라
    [SerializeField] GridManager grid;          // 그리드 정보
    [SerializeField] Transform _player; // 플레이어 위치

    [Header("레이어 마스크")]
    [SerializeField] LayerMask tileMask;        // FarmTile이 붙어있는 타일 레이어
    [SerializeField] LayerMask actionMask;      // 행동 레이어
    [SerializeField] LayerMask interactMask;    // 상호작용 레이어

    [Header("타일 하이라이트")]
    [SerializeField] GameObject tileHighlightPrefab;
    [SerializeField] float highlightYPos = 0.11f;

    [Header("타일 범위")]
    [SerializeField] int maxRangeInTiles = 2;

    [SerializeField, ReadOnly]FarmTile _currentTile;
    GameObject _tileHighlightInstance;
    bool isFocus;
    
    public FarmTile CurrentTile => _currentTile;

    public void Initialize(Player player)
    {
        if (cam == null)
            cam = Camera.main;
        _player = player.transform;

        if(tileHighlightPrefab!=null)
        {
            _tileHighlightInstance = Instantiate(tileHighlightPrefab);
            _tileHighlightInstance.SetActive(false);
        }
    }

    private void Update()
    {
        UpdateCurrentTileFromMouseDirection();

        if (_tileHighlightInstance != null && _currentTile != null && isFocus)
        {
            _tileHighlightInstance.transform.position =
                _currentTile.transform.position + Vector3.up * highlightYPos;
            SetHighlightActive(true);
        }
        else
            SetHighlightActive(false);
    }

    private void UpdateCurrentTileFromMouseDirection()
    {
        _currentTile = null;

        if (!TryGetTileFromMouseDirection(_player.position, out FarmTile tile))
        {
            SetHighlightActive(false);
            return;
        }
        _currentTile = tile;
    }

    private void SetHighlightActive(bool isActive)
    {
        if (_tileHighlightInstance != null)
        {
            _tileHighlightInstance.SetActive(isActive);
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if (Mouse.current != null)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Ray ray = cam.ScreenPointToRay(mousePos);
            Gizmos.DrawRay(ray);
        }
    }

    public void SetFocus(bool isFocus) => this.isFocus = isFocus;

    /// <summary>
    /// 마우스 방향의 플레이어 기준 범위 내의 타일 반환
    /// </summary>
    /// <param name="playerPos"></param>
    /// <param name="tile"></param>
    /// <returns></returns>
    public bool TryGetTileFromMouseDirection(Vector3 playerPos, out FarmTile tile)
    {
        tile = null;

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = cam.ScreenPointToRay(mousePos);

        // 1) 바닥/타일에 레이 쏴서 마우스 방향의 월드지점 얻기
        if (!Physics.Raycast(ray, out RaycastHit hit, 1000f, tileMask))
        {
            Debug.Log("Raycast Failed!");
            return false;
        }
        
        Vector3 aimWorld = hit.point;

        if (!grid.WorldToGrid(playerPos, out int px, out int pz))
            return false;
        if (!grid.WorldToGrid(aimWorld, out int ax, out int az))
            return false;

        int dx = Mathf.Clamp(ax - px, -maxRangeInTiles, maxRangeInTiles);
        int dz = Mathf.Clamp(az - pz, -maxRangeInTiles, maxRangeInTiles);

        int tx = px + dx;
        int tz = pz + dz;

        // 4) 범위 체크 + 타일 가져오기
        FarmTile t = grid.GetTile(tx, tz);
        if (t == null)
            return false;

        tile = t;
        return true;
    }

    /// <summary>
    /// 도구 액션용:
    /// 현재 "플레이어→마우스 방향"으로 선택된 타일에서 IToolTarget 찾기
    /// </summary>
    public bool TryGetToolTargetFromMouseDirection(
        Vector3 playerPos,
        ToolData tool,
        out IToolTarget target,
        out Vector3 hitPoint,
        out Vector3 hitNormal)
    {
        target = null;
        hitPoint = default;
        hitNormal = Vector3.up;

        if (!TryGetTileFromMouseDirection(playerPos, out FarmTile tile))
            return false;

        // 타일 중심 주변에서 도구 타겟 찾기
        float radius = (grid != null) ? grid.cellSize * 0.5f : 0.5f;
        Vector3 center = tile.transform.position + Vector3.up * 0.5f;

        Collider[] cols = Physics.OverlapSphere(center, radius, actionMask);
        foreach (var col in cols)
        {
            var t = col.GetComponentInParent<IToolTarget>();
            if (t != null)
            {
                target = t;
                hitPoint = col.ClosestPoint(center);
                // hitNormal은 필요하면 레이 방향이나 Up으로 세팅
                hitNormal = Vector3.up;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 상호작용용:
    /// 플레이어 주변 1타일(월드 거리 기준) 안에 있고,
    /// 마우스가 가리키는 오브젝트에서 IInteractable 찾기
    /// </summary>
    public bool TryGetInteractTarget(
        Vector3 playerPos,
        out IInteractable interactable)
    {
        interactable = null;

        if (Mouse.current == null || cam == null)
            return false;

        // 1) 마우스 위치로 레이 쏘기
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = cam.ScreenPointToRay(mousePos);

        // interactMask에 맞는 오브젝트만
        if (!Physics.Raycast(ray, out RaycastHit hit, 100f, interactMask))
            return false;

        if (hit.collider.TryGetComponent<IInteractable>(out var target))
        {
            Vector2 targetPos = new Vector2(hit.collider.transform.position.x, hit.collider.transform.position.z);
            Vector2 playerPosxz = new Vector2(playerPos.x, playerPos.z);
            var dist = Vector2.Distance(playerPosxz, targetPos);
            if (dist > maxRangeInTiles)
                return false;
        }
        else return false;

        // 4) 여기까지 왔으면 유효한 상호작용 대상
        interactable = target;
        return true;
    }
}
