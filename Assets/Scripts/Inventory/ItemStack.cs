using UnityEngine;

[System.Serializable]
public class ItemStack
{
    [SerializeField] int _itemId = -1;
    [SerializeField] int _count = 0;

    public int ItemId => _itemId;
    public int Count => _count;

    public bool IsEmpty => _itemId < 0 || _count <= 0;

    public void Clear()
    {
        _itemId = -1;
        _count = 0;
    }

    public void Set(int itemId, int count)
    {
        _itemId = itemId;
        _count = count;
    }

    public void Add(int amount)
    {
        _count += amount;
    }

    public void Remove(int amount)
    {
        _count -= amount;
        if (_count < 0)
            Clear();
    }
}
