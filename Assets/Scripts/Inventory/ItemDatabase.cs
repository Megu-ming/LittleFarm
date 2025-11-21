using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemDatabase : MonoBehaviour
{
    [Header("CSV 데이터")]
    [SerializeField] TextAsset _itemCsv;

    Dictionary<int, ItemSpec> _itemsById = new Dictionary<int, ItemSpec>();
    Dictionary<string, ItemSpec> _itemsByKey = new Dictionary<string, ItemSpec>();

    public void Initialize()
    {
        LoadFromCsv(_itemCsv.text);
    }

    public ItemSpec GetById(int id)
    {
        _itemsById.TryGetValue(id, out var spec);
        return spec;
    }

    public ItemSpec GetByKey(string key)
    {
        _itemsByKey.TryGetValue(key, out var spec);
        return spec;
    }

    private void LoadFromCsv(string csvText)
    {
        _itemsById.Clear();
        _itemsByKey.Clear();

        if (string.IsNullOrEmpty(csvText))
        {
            Debug.LogWarning("[ItemDatabase] CSV 내용이 비어 있습니다.");
            return;
        }

        string[] lines = csvText.Split('\n');
        if(lines.Length<=1)
        {
            Debug.LogWarning("[ItemDatabase] CSV에 데이터가 없습니다.");
            return;
        }

        string headerLine = lines[0].TrimEnd('\r');
        string[] headerCols = headerLine.Split(',');

        Dictionary<string, int> colIndex = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for(int i=0;i<headerCols.Length;i++)
        {
            string colName = headerCols[i].Trim();
            if (!string.IsNullOrEmpty(colName) && !colIndex.ContainsKey(colName)) 
                colIndex[colName] = i;
        }

        // 헬퍼 함수들
        string GetString(string[] cols, string name, string defaultValue = "")
        {
            if (!colIndex.TryGetValue(name, out int idx)) return defaultValue;
            if (idx < 0 || idx >= cols.Length) return defaultValue;
            return cols[idx].Trim();
        }

        int GetInt(string[] cols, string name, int defaultValue = 0)
        {
            string s = GetString(cols, name, "");
            if (int.TryParse(s, out int v)) return v;
            return defaultValue;
        }

        ItemCategory GetCategory(string[] cols, string name)
        {
            string s = GetString(cols, name, "Etc");
            if (Enum.TryParse<ItemCategory>(s, out var cat))
                return cat;
            return ItemCategory.Etc;
        }

        for(int lineIndex = 1;lineIndex<lines.Length; lineIndex++)
        {
            string line = lines[lineIndex].Trim();
            if(string.IsNullOrWhiteSpace(line)) continue;

            string[] cols = line.Split(',');

            int id = GetInt(cols, "id", -1);
            if (id < 0) continue;

            string key = GetString(cols, "key", null);
            if(string.IsNullOrEmpty(key)) continue;

            ItemSpec spec = new ItemSpec
            {
                id = id,
                key = key,
                name = GetString(cols, "name", key),
                desc = GetString(cols, "desc", ""),
                category = GetCategory(cols, "category"),
                maxStack = GetInt(cols, "maxStack", 1),
                buyPrice = GetInt(cols, "buyPrice", 0),
                sellPrice = GetInt(cols, "sellPrice", 0),
                iconKey = GetString(cols, "iconKey", ""),
                worldKey = GetString(cols, "worldKey", ""),
                toolKey = GetString(cols, "toolKey", "")
            };

            _itemsById[spec.id] = spec;
            _itemsByKey[spec.key] = spec;
        }

        Debug.Log($"[ItemDatabase] 로드 완료 : 총 {_itemsById.Count}개 아이템");
    }
}
