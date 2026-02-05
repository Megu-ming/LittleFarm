using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemDatabase : MonoBehaviour
{
    [Header("CSV 데이터")]
    [SerializeField] TextAsset _itemCsv;

    [Header("리소스 폴더 이름")]
    [SerializeField] string _iconResourceFolder = "ItemIcons";  // Assets/Resources/ItemIcons
    [SerializeField] string _worldResourceFolder = "ItemDrops";  // Assets/Resources/ItemDrops
    [SerializeField] string _handResourceFolder = "ItemHandProps";  // Assets/Resources/ItemHandProps

    Dictionary<int, ItemSpec> _itemsById = new Dictionary<int, ItemSpec>();
    Dictionary<string, ItemSpec> _itemsByKey = new Dictionary<string, ItemSpec>();

    public void Initialize()
    {
        _itemCsv = Resources.Load<TextAsset>("Data/ItemList");
        LoadFromCsv(_itemCsv.text);

        LinkIconSprites();                   // 아이콘 스프라이트 연결
        LinkWorldPrefabs();                  // 월드 드랍 프리팹 연결
        LinkHandPrefabs();                   // 아이템 핸드 프리팹 연결
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
                toolKey = GetString(cols, "toolKey", ""),
                handKey = GetString(cols, "handKey", "")
            };

            _itemsById[spec.id] = spec;
            _itemsByKey[spec.key] = spec;
        }

        Debug.Log($"[ItemDatabase] 로드 완료 : 총 {_itemsById.Count}개 아이템");
    }

    /// <summary>
    /// Resources/ItemIcons 에서 Sprite들을 읽어와서
    /// ItemSpec.iconKey(or key)와 이름을 매칭해 iconSprite에 넣어줌.
    /// </summary>
    void LinkIconSprites()
    {
        Sprite[] sprites = Resources.LoadAll<Sprite>(_iconResourceFolder);
        if (sprites == null || sprites.Length == 0)
        {
            Debug.LogWarning($"[ItemDatabase] Resources/{_iconResourceFolder} 에서 스프라이트를 찾지 못했습니다.");
            return;
        }

        var spriteByName = new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase);
        foreach (var sp in sprites)
        {
            if (sp == null) continue;
            spriteByName[sp.name] = sp;
        }

        int linked = 0;

        foreach (var kv in _itemsById)
        {
            ItemSpec spec = kv.Value;
            if (spec == null) continue;

            // 우선 iconKey 사용, 없으면 key 사용
            string iconKey = !string.IsNullOrEmpty(spec.iconKey) ? spec.iconKey : spec.key;
            if (string.IsNullOrEmpty(iconKey)) continue;

            if (spriteByName.TryGetValue(iconKey, out var sp))
            {
                spec.iconSprite = sp;
                linked++;
            }
            else
            {
                // 필요하면 iconKey + "_icon" 같은 규칙도 추가 가능
                string alt = iconKey + "_icon";
                if (spriteByName.TryGetValue(alt, out sp))
                {
                    spec.iconSprite = sp;
                    linked++;
                }
                // 못 찾으면 그냥 null 유지 (슬롯 UI에서 자동으로 비활성화됨) :contentReference[oaicite:4]{index=4}
            }
        }

        Debug.Log($"[ItemDatabase] 아이콘 연결 완료: {linked}개");
    }

    /// <summary>
    /// Resources/ItemDrops 에서 프리팹을 읽어와
    /// ItemSpec.worldKey(or key)와 이름을 매칭해 worldPrefab에 넣어줌.
    /// </summary>
    void LinkWorldPrefabs()
    {
        GameObject[] prefabs = Resources.LoadAll<GameObject>(_worldResourceFolder);
        if (prefabs == null || prefabs.Length == 0)
        {
            Debug.LogWarning($"[ItemDatabase] Resources/{_worldResourceFolder} 에서 프리팹을 찾지 못했습니다.");
            return;
        }

        var prefabByName = new Dictionary<string, GameObject>(StringComparer.OrdinalIgnoreCase);
        foreach (var go in prefabs)
        {
            if (go == null) continue;
            prefabByName[go.name] = go;
        }

        int linked = 0;

        foreach (var kv in _itemsById)
        {
            ItemSpec spec = kv.Value;
            if (spec == null) continue;

            string worldKey = !string.IsNullOrEmpty(spec.worldKey) ? spec.worldKey : spec.key;
            if (string.IsNullOrEmpty(worldKey)) continue;

            if (prefabByName.TryGetValue(worldKey, out var go))
            {
                spec.worldPrefab = go;
                linked++;
            }
            else
            {
                string alt = worldKey + "_prefab";
                if (prefabByName.TryGetValue(alt, out go))
                {
                    spec.worldPrefab = go;
                    linked++;
                }
            }
        }

        Debug.Log($"[ItemDatabase] 월드 프리팹 연결 완료: {linked}개");
    }

    void LinkHandPrefabs()
    {
        GameObject[] prefabs = Resources.LoadAll<GameObject>(_handResourceFolder);
        if (prefabs == null || prefabs.Length == 0)
        {
            Debug.LogWarning($"[ItemDatabase] Resources/{_handResourceFolder} 에서 프리팹을 찾지 못했습니다.");
            return;
        }

        var prefabByName = new Dictionary<string, GameObject>(StringComparer.OrdinalIgnoreCase);
        foreach (var go in prefabs)
        {
            if (go == null) continue;
            prefabByName[go.name] = go;
        }

        int linked = 0;

        foreach (var kv in _itemsById)
        {
            ItemSpec spec = kv.Value;
            if (spec == null) continue;

            string handKey = !string.IsNullOrEmpty(spec.handKey) ? spec.handKey : spec.key;
            if (string.IsNullOrEmpty(handKey)) continue;

            if (prefabByName.TryGetValue(handKey, out var go))
            {
                spec.handPrefab = go;
                linked++;
            }
            else
            {
                string alt = handKey + "_prefab";
                if (prefabByName.TryGetValue(alt, out go))
                {
                    spec.handPrefab = go;
                    linked++;
                }
            }
        }

        Debug.Log($"[ItemDatabase] 핸드 프리팹 연결 완료: {linked}개");
    }
}
