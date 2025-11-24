using UnityEngine;

public class GameInitializer : MonoBehaviour
{
    public static GameInitializer Instance;

    [Header("주요 객체 참조")]
    [SerializeField] Player _player;
    [SerializeField] ItemDatabase _database;
    [SerializeField] Inventory _inventory;

    [Header("UI 참조")]
    [SerializeField] ToolChangerUI _toolChangerUI;
    [SerializeField] InventoryUI _inventoryUI;

    public ItemDatabase Database => _database;

    private void Start()
    {
        Instance = this;

        _database.Initialize();
        _inventory.Initialize(_database);
        _inventoryUI.Initialize(_inventory, _database);
        _toolChangerUI.Initialize(_player);
        _player.Initialize(_inventoryUI, _toolChangerUI);
    }
}
