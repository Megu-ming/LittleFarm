using System.Collections.Generic;
using UnityEngine;

public class QuickSlotUI : MonoBehaviour
{
    [SerializeField] Transform _handTransform;
    [SerializeField] List<QuickSlot_SlotUI> _quickSlots = new List<QuickSlot_SlotUI>();
    [SerializeField] QuickSlot_SlotUI _quickSlotPrefab;

    Player _player;
    Inventory _inventory;
    ItemDatabase _database;

    public Transform Hand => _handTransform;

    const int slotCount = 10;

    public void Initialize(Player player, Inventory inventory, ItemDatabase db)
    {
        _player = player;
        _inventory = inventory;
        _database = db;

        for(int i=0;i<slotCount;i++)
        {
            var slot = Instantiate(_quickSlotPrefab, transform);
            slot.name = $"QuickSlot_{i}";
            slot.transform.SetSiblingIndex(i);
            _quickSlots.Add(slot);
            slot.Initialize(this, player, i);
        }

        RefreshAll();
    }

    public void RefreshAll()
    {
        if (_inventory == null || _quickSlots == null)
            return;

        if (_database == null)
            return;

        for (int i = 0; i < _quickSlots.Count; i++)
        {
            var slotUI = _quickSlots[i];
            if (slotUI == null) continue;

            var stack = _inventory.GetSlot(i);

            slotUI.Refresh(stack, _database);
        }
    }
}
