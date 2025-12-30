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
    [SerializeField] TerrainGridManager _tgManager;

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

        _tgManager.Initialize();

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

    // 필드에 아이템 드랍해주는 함수 -> 헬퍼 클래스 만들어서 옮길 예정
    public void DropItems(string itemKey, Vector3 spawnPosition, Vector3 dropOffset, int dropMin = 1, int dropMax = 1)
    {
        var db = _database;
        if (db == null)
        {
            Debug.LogWarning("[GameInitializer::DropItems] ItemDatabase가 없습니다.");
            return;
        }

        if (string.IsNullOrEmpty(itemKey))
        {
            Debug.LogWarning("[GameInitializer::DropItems] 드랍 아이템 Key가 없습니다.");
            return;
        }

        ItemSpec spec = db.GetByKey(itemKey);
        if (spec == null)
        {
            Debug.LogWarning($"[GameInitializer::DropItems] 드랍 아이템을 찾을 수 없습니다. key = {itemKey}");
            return;
        }

        int itemId = spec.id;

        int dropCount = Random.Range(dropMin, dropMax + 1);
        if (dropCount <= 0) return;

        GameObject prefab = Resources.Load<GameObject>($"ItemDrops/{spec.worldKey}");
        if (prefab == null)
        {
            Debug.LogWarning($"[GameInitializer::DropItems] 프리팹을 찾을 수 없습니다: Resources/ItemDrops/{spec.worldKey}");
            return;
        }

        for (int i = 0; i < dropCount; i++)
        {
            Vector2 rand = Random.insideUnitCircle * 0.3f;
            Vector3 pos = spawnPosition + dropOffset + new Vector3(rand.x, 0, rand.y);

            GameObject go = Instantiate(prefab, pos, Quaternion.identity);

            var pickup = go.GetComponent<ItemPickup>();
            if (pickup != null)
            {
                pickup.Setup(itemId, 1);
                pickup.PlayDropEffect();
            }
        }
    }
}
