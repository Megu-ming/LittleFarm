using UnityEngine;
using UnityEngine.InputSystem;

public class GridSelector : MonoBehaviour
{
    [SerializeField] Camera cam;       // 사용할 카메라
    [SerializeField] LayerMask tileMask; // 타일 레이어만 맞추기

    FarmTile currentTile;

    private void Awake()
    {
        if (cam == null)
            cam = Camera.main;
    }

    private void Update()
    {
        // 마우스가 없는 환경(패드 전용 등) 대비
        if (Mouse.current == null) return;

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = cam.ScreenPointToRay(mousePos);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, tileMask))
        {
            FarmTile tile = hit.collider.GetComponent<FarmTile>();
            if (tile != null)
            {
                // 마우스가 새 타일 위로 옮겨졌을 때만 로그 출력
                if (tile != currentTile)
                {
                    currentTile = tile;
                    Debug.Log($"마우스가 타일 ({tile.x}, {tile.z}) 위에 있습니다.");
                }

                // 클릭했을 때
                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    Debug.Log($"타일 클릭: ({tile.x}, {tile.z})");
                    // 나중에 여기서 작물 심기 같은 로직 호출하면 됨
                }
            }
        }
        else
        {
            currentTile = null;
        }
    }
}
