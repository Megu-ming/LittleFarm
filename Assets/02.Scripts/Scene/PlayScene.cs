using UnityEngine;

/// <summary>
/// Play 씬을 총괄하는 역할
/// </summary>
public class PlayScene : MonoBehaviour
{
    [SerializeField] Player _player;
    [SerializeField] Inventory _inventory;

    private void Start()
    {
        var gm = GameManager.Instance;
    }
}
