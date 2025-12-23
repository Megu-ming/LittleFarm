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

        _player.HandChanged += UpdateHand;
        _inventory.SlotChanged += OnInventorySlotChanged;
        RefreshAll();

        UpdateHand(_player.Hand);
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

    void RefreshSlot(int index)
    {
        if (_inventory == null || _database == null) return;
        if (_quickSlots == null || index < 0 || index >= _quickSlots.Count) return;

        var slotUI = _quickSlots[index];
        if (slotUI == null) return;

        var stack = _inventory.GetSlot(index);
        slotUI.Refresh(stack, _database);
    }

    void OnInventorySlotChanged(int index)
    {
        // Äü½½·Ô ¿µ¿ª(0~9)¸¸ ¹Ý¿µ
        if (index < 0 || index >= slotCount) return;
        RefreshSlot(index);
    }

    public void UpdateHand(int index)
    {
        if(index == -1)
        {
            _handTransform.gameObject.SetActive(false);
        }
        else
        {
            _handTransform.gameObject.SetActive(true);
            _handTransform.position = _quickSlots[index].transform.position;
        }
    }
}
