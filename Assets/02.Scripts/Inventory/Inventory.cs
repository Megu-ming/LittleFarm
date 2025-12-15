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

    ItemDatabase _database;

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

    /// <summary>
    /// 인덱스로 슬롯 참조 가져오기
    /// </summary>
    /// <returns></returns>
    public ItemStack GetSlot(int index)
    {
        if (index < 0 || index >= _slots.Length)
            return null;
        return _slots[index];
    }

    /// <summary>
    /// 특정 아이템의 총 수량을 구하는 함수
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// itemId 아이템을 amount개 추가 시도
    /// 모두 들어가면 true, 일부라도 못들어가면 false
    /// </summary>
    /// <returns></returns>
    public bool TryAddItem(int itemId, int amount, out int remainder)
    {
        remainder = amount;
        if (amount <= 0) return true;

        if(_database == null)
        {
            Debug.LogWarning("[Inventory] ItemDatabase가 없습니다");
            return false;
        }

        var spec = _database.GetById(itemId);
        if (spec == null)
        {
            Debug.LogWarning($"[Inventory] Unknown item id : {itemId}");
            return false;
        }

        int maxStack = Mathf.Max(1, spec.maxStack);

        // 같은 아이템이 있는 슬롯에 먼저 채우기
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
        }

        // 남은 수량을 열린 슬롯 중 빈 슬롯에 넣기
        for(int i =0;i<_unlockedSlots && remainder > 0;i++)
        {
            var slot = _slots[i];
            if (!slot.IsEmpty) continue;

            int toAdd = Mathf.Min(maxStack, remainder);
            slot.Set(itemId, toAdd);
            remainder -= toAdd;
        }

        return remainder == 0;
    }

    /// <summary>
    /// itemId 아이템을 amount개 제거 시도
    /// 모두 제거하면 true, 부족하면 false
    /// </summary>
    /// <returns></returns>
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
        }

        return true;
    }

    /// <summary>
    /// itemId를 최소 amount개 이상 가지고 있는지 여부
    /// </summary>
    /// <returns></returns>
    public bool HasEnough(int itemId, int amount)
    {
        return GetTotalCount(itemId) >= amount;
    }
}
