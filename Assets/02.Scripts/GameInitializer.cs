using UnityEngine;

public class GameInitializer : MonoBehaviour
{
    private static GameInitializer instance;

    [Header("테스트용 임시")]
    [SerializeField] BuildingPlacer _buildingPlacer;

    [Header("주요 객체 참조")]
    [SerializeField] Player _player;
    [SerializeField] Inventory _inventory;
    [SerializeField] GameTimeManager _timeManager;
    [SerializeField] CropManager _cropManager;
    [SerializeField] GridManager _gridManager;
    [SerializeField] GridSelector _gridSelector;
    [SerializeField] TerrainGridManager _tgManager;

    [Header("UI 참조")]
    [SerializeField] ToolChangerUI _toolChangerUI;
    [SerializeField] InventoryUI _inventoryUI;
    [SerializeField] QuickSlotUI _quickSlotUI;
    [SerializeField] TimeUI _timeUI;

    [SerializeField] Transform _worldRoot;

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

    public void Initialize()
    {
        var gm = GameManager.Instance;
        PlacementService.Initialize(_worldRoot);

        _tgManager.Initialize();
        _gridSelector.Initialize(_player);

        _inventory.Initialize(gm.DataManager.ItemDatabase);
        _inventoryUI.Initialize(_inventory, gm.DataManager.ItemDatabase);
        _toolChangerUI.Initialize(_player);
        _quickSlotUI.Initialize(_player, _inventory, gm.DataManager.ItemDatabase);
        _player.Initialize(_inventoryUI, _toolChangerUI, _quickSlotUI, _gridSelector, _cropManager);

        _timeManager.Initialize();
        _timeUI.Initialize(_timeManager);

        _cropManager.Initialize(_timeManager, _gridManager);

        // 테스트용 임시
        _buildingPlacer.Initialize(_gridSelector, _gridManager);
    }
}
