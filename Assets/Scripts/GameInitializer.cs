using UnityEngine;

public class GameInitializer : MonoBehaviour
{
    [Header("주요 객체 참조")]
    [SerializeField] Player _player;
    [SerializeField] ItemDatabase _database;
    [SerializeField] Inventory _inventory;

    [Header("UI 참조")]
    [SerializeField] ToolChangerUI _toolChangerUI;
    [SerializeField] InventoryUI _inventoryUI;

    private void Start()
    {
        _database.Initialize();
        _inventory.Initialize(_database);
        _inventoryUI.Initialize(_inventory, _database);
        _player.Initialize(_inventoryUI);
        _toolChangerUI.Initialize(_player);
    }
}
