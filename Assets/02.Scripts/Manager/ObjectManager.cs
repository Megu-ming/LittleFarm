using UnityEngine;

/// <summary>
/// 오브젝트 생성, 관리하는 역할
/// </summary>
public class ObjectManager : MonoBehaviour
{
    // 필드에 아이템 드랍해주는 함수
    public void DropItems(string itemKey, Vector3 spawnPosition, Vector3 dropOffset, int dropMin = 1, int dropMax = 1)
    {
        var db = GameManager.Instance.DataManager.ItemDatabase;
        if (db == null)
        {
            Debug.LogWarning("[ObjectManager::DropItems] ItemDatabase가 없습니다.");
            return;
        }

        if (string.IsNullOrEmpty(itemKey))
        {
            Debug.LogWarning("[ObjectManager::DropItems] 드랍 아이템 Key가 없습니다.");
            return;
        }

        ItemSpec spec = db.GetByKey(itemKey);
        if (spec == null)
        {
            Debug.LogWarning($"[ObjectManager::DropItems] 드랍 아이템을 찾을 수 없습니다. key = {itemKey}");
            return;
        }

        int itemId = spec.id;

        int dropCount = Random.Range(dropMin, dropMax + 1);
        if (dropCount <= 0) return;

        GameObject prefab = Resources.Load<GameObject>($"ItemDrops/{spec.worldKey}");
        if (prefab == null)
        {
            Debug.LogWarning($"[ObjectManager::DropItems] 프리팹을 찾을 수 없습니다: Resources/ItemDrops/{spec.worldKey}");
            return;
        }

        for (int i = 0; i < dropCount; i++)
        {
            Vector2 rand = Random.insideUnitCircle * 0.3f;
            Vector3 pos = spawnPosition + dropOffset + new Vector3(rand.x, 0, rand.y);

            GameObject go = Instantiate(prefab, pos, Quaternion.identity);

            var pickup = go.GetComponent<ItemPickup>();
            if (pickup != null)
            {
                pickup.Setup(itemId, 1);
                pickup.PlayDropEffect();
            }
        }
    }
}
