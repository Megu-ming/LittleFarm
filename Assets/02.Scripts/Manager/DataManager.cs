using System.IO;
using UnityEditor.Overlays;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    [SerializeField] GameSaveData _gameSaveData;
    [SerializeField] ItemDatabase _itemDatabase;

    Player _player;
    Inventory _inventory;

    public PlayerSaveData PlayerSaveData => _gameSaveData.PlayerSaveData;
    public InventorySaveData InventorySaveData => _gameSaveData.InventorySaveData;
    public ItemDatabase ItemDatabase => _itemDatabase;

    public void SetPlayer(Player player)
    {
        _player = player;
    }

    public void SetInventory(Inventory inventory)
    {
        _inventory = inventory;
    }

    private void Awake()
    {
        _itemDatabase = gameObject.GetOrAddComponent<ItemDatabase>();
        _itemDatabase.Initialize();
    }

    public void Save()
    {
        if (_player != null && _inventory != null)
        {
            PlayerSaveData playerSaveData = _player.GetPlayerSaveData();
            InventorySaveData inventorySaveData = _inventory.GetSaveData();
            _gameSaveData.SetPlayerData(playerSaveData);
            _gameSaveData.SetInventorySaveData(inventorySaveData);
        }

        string json = JsonUtility.ToJson(_gameSaveData);
        Debug.Log($"세이브 데이터\n{json}");

        string path = $"{Application.persistentDataPath}/GameSaveData.json";
        Debug.Log(path);
        System.Diagnostics.Process.Start(Application.persistentDataPath);
        File.WriteAllText(path, json);
    }

    public void Load()
    {
        if (File.Exists($"{Application.persistentDataPath}/GameSaveData.json") == false)
        {
            _gameSaveData = new GameSaveData();
        }
        else
        {
            string json = File.ReadAllText($"{Application.persistentDataPath}/GameSaveData.json");
            Debug.Log($"로드 데이터\n{json}");
            _gameSaveData = JsonUtility.FromJson<GameSaveData>(json);
        }
    }
}
