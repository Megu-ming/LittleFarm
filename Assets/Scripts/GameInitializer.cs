using UnityEngine;

public class GameInitializer : MonoBehaviour
{
    private static GameInitializer instance;

    [Header("주요 객체 참조")]
    [SerializeField] Player _player;
    [SerializeField] ItemDatabase _database;
    [SerializeField] Inventory _inventory;
    [SerializeField] GameTimeManager _timeManager;
    [SerializeField] CropManager _cropManager;
    [SerializeField] GridManager _gridManager;

    [Header("UI 참조")]
    [SerializeField] ToolChangerUI _toolChangerUI;
    [SerializeField] InventoryUI _inventoryUI;
    [SerializeField] QuickSlotUI _quickSlotUI;
    [SerializeField] TimeUI _timeUI;

    public ItemDatabase Database => _database;

    public static GameInitializer Instance
    {
        get
        {
            if(instance == null)
            {
                return null;
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
            Destroy(this.gameObject);
    }

    private void Start()
    {
        instance = this;

        _database.Initialize();
        _inventory.Initialize(_database);
        _inventoryUI.Initialize(_inventory, _database);
        _toolChangerUI.Initialize(_player);
        _quickSlotUI.Initialize(_player, _inventory, _database);
        _player.Initialize(_inventoryUI, _toolChangerUI, _quickSlotUI);

        _timeManager.Initialize();
        _timeUI.Initialize(_timeManager);

        _cropManager.Initialize(_timeManager, _gridManager);
    }
}
