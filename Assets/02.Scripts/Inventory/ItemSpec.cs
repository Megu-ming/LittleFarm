using UnityEngine;

[System.Serializable]
public class ItemSpec
{
    public int id;
    public string key;
    public string name;
    public string desc;
    public ItemCategory category;
    public int maxStack;
    public int buyPrice;
    public int sellPrice;
    public string iconKey;
    public string worldKey;
    public string toolKey;
    public string handKey;

    public Sprite iconSprite;
    public GameObject worldPrefab;

    public GameObject handPrefab;

    public ToolData toolData;

    public override string ToString()
    {
        return $"ItemSpec(id={id}, key={key}, name={name}, category={category})";
    }
}
