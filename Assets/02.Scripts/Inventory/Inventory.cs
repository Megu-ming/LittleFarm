using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [Header("슬롯 설정")]
    [SerializeField] int _maxSlotCount = 30;
    [SerializeField] int _initialUnlocked = 10;
    [SerializeField] ItemStack[] _slots;

    [Header("디버그 확인용")]
    [SerializeField] int _unlockedSlots;

    public int SlotCount => _maxSlotCount;
    public IReadOnlyList<ItemStack> Slots => _slots;
    public int UnlockedSlots => _unlockedSlots;

    private ItemDatabase _database;
    public ItemDatabase Database => _database;

    /// <summary>슬롯 내용이 바뀌면 호출됨(index)/// </summary>
    public event Action<int> SlotChanged;

    public void Initialize(ItemDatabase database)
    {
        _database = database;

        if(_slots == null || _slots.Length != _maxSlotCount)
            _slots = new ItemStack[_maxSlotCount];

        for(int i=0;i<_slots.Length;i++)
        {
            if(_slots[i] == null)
                _slots[i] = new ItemStack();
        }

        _unlockedSlots = Mathf.Clamp(_initialUnlocked, 0, _maxSlotCount);
    }

    public ItemStack GetSlot(int index)
    {
        if (index < 0 || index >= _slots.Length)
            return null;
        return _slots[index];
    }

    public int GetTotalCount(int itemId)
    {
        int total = 0;
        foreach(var slot in _slots)
        {
            if (slot.ItemId == itemId)
                total += slot.Count;
        }
        return total;
    }

    public bool TryAddItem(int itemId, int amount, out int remainder)
    {
        remainder = amount;
        if (amount <= 0) return true;

        if(_database == null)
        {
            Debug.LogWarning("[Inventory] ItemDatabase�� �����ϴ�");
            return false;
        }

        var spec = _database.GetById(itemId);
        if (spec == null)
        {
            Debug.LogWarning($"[Inventory] Unknown item id : {itemId}");
            return false;
        }

        int maxStack = Mathf.Max(1, spec.maxStack);

        for(int i=0;i<_unlockedSlots && remainder>0;i++)
        {
            var slot = _slots[i];
            if (slot.IsEmpty) continue;
            if (slot.ItemId != itemId) continue;

            int canAdd = maxStack - slot.Count;
            if (canAdd <= 0) continue;

            int toAdd = Mathf.Min(canAdd, remainder);
            slot.Add(toAdd);
            remainder -= toAdd;

            SlotChanged?.Invoke(i);
        }

        for(int i =0;i<_unlockedSlots && remainder > 0;i++)
        {
            var slot = _slots[i];
            if (!slot.IsEmpty) continue;

            int toAdd = Mathf.Min(maxStack, remainder);
            slot.Set(itemId, toAdd);
            remainder -= toAdd;

            SlotChanged?.Invoke(i);
        }

        return remainder == 0;
    }

    public bool TryRemoveItem(int itemId, int amount)
    {
        if (amount <= 0) return true;

        int total = GetTotalCount(itemId);
        if (total < amount) return false;

        int remaining = amount;

        for (int i = 0; i < _slots.Length && remaining > 0; i++)
        {
            var slot = _slots[i];
            if(slot.ItemId != itemId) continue;

            if(slot.Count <= remaining)
            {
                remaining -= slot.Count;
                slot.Clear();
            }
            else
            {
                slot.Remove(remaining);
                remaining = 0;
            }

            SlotChanged?.Invoke(i);
        }

        return true;
    }

    public bool TryConsumeFromSlot(int slotIndex, int amount)
    {
        if (amount <= 0) return true;
        if (slotIndex < 0 || slotIndex >= _slots.Length) return false;

        var slot = _slots[slotIndex];
        if (slot == null || slot.IsEmpty) return false;
        if (slot.Count < amount) return false;

        if (slot.Count == amount) slot.Clear();
        else slot.Remove(amount);

        SlotChanged?.Invoke(slotIndex);
        return true;
    }

    public bool HasEnough(int itemId, int amount)
    {
        return GetTotalCount(itemId) >= amount;
    }
}
