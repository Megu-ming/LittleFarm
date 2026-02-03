using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InventorySaveData
{
    [SerializeField] int[] _itemIds;
    [SerializeField] int[] _itemAmounts;

    public IReadOnlyList<int> ItemIds => _itemIds;
    public IReadOnlyList<int> ItemAmounts => _itemAmounts;
    public InventorySaveData()
    {
        _itemIds = new int[Inventory._maxSlotCount];
        _itemAmounts = new int[Inventory._maxSlotCount];
    }
    public InventorySaveData(int[] itemIds, int[] itemAmounts)
    {
        SetItems(itemIds, itemAmounts);
    }

    public void SetItems(int[] itemIds, int[] itemAmounts)
    {
        _itemIds = itemIds;
        _itemAmounts = itemAmounts;
    }
}
