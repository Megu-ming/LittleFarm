using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] Inventory _inventory;
    [SerializeField] Transform _slotParent;
    [SerializeField] InventorySlotUI _slotPrefab;

    [SerializeField] InventorySlotUI[] _slotUIs;

    ItemDatabase _database;

    bool _isOpen = false;

    public void Initialize(Inventory inventory, ItemDatabase database)
    {
        _inventory = inventory;
        _database = database;

        CreateSlots();
    }

    private void CreateSlots()
    {
        if(_slotUIs != null)
        {
            for (int i = 0; i < _slotUIs.Length; i++)
                Destroy(_slotUIs[i].gameObject);
        }

        int count = _inventory.SlotCount;
        _slotUIs = new InventorySlotUI[count];

        for(int i=0;i<count;i++)
        {
            InventorySlotUI slot = Instantiate(_slotPrefab, _slotParent);

            slot.SetIndex(i);

            _slotUIs[i] = slot;
        }

        RefreshAll();
    }

    public void Toggle()
    {
        SetOpen(!_isOpen);
    }

    public void SetOpen(bool open)
    {
        _isOpen = open;
        gameObject.SetActive(open);

        if(_isOpen)
        {
            RefreshAll();
            // 인벤토리 오픈 시 시간 정지
            Time.timeScale = 0f;
        }
        else
            Time.timeScale = 1f;
    }

    public void RefreshAll()
    {
        if (_inventory == null || _slotUIs == null)
            return;

        if (_database == null)
            return;

        int unlocked = _inventory.UnlockedSlots;

        for(int i=0;i<_slotUIs.Length;i++)
        {
            var slotUI = _slotUIs[i];
            if(slotUI == null) continue;

            var stack = _inventory.GetSlot(i);
            bool isUnlocked = i < unlocked;

            slotUI.Refresh(stack, isUnlocked, _database);
        }
    }
}
