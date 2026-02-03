using UnityEngine;

[System.Serializable]
public class GameSaveData
{
    [SerializeField] PlayerSaveData _playerSaveData = new PlayerSaveData();
    [SerializeField] InventorySaveData _inventorySaveData = new InventorySaveData();

    public PlayerSaveData PlayerSaveData => _playerSaveData;
    public InventorySaveData InventorySaveData => _inventorySaveData;

    public void SetPlayerData(PlayerSaveData playerData)
    {
        _playerSaveData = playerData;
    }

    public void SetInventorySaveData(InventorySaveData inventorySaveData)
    {
        _inventorySaveData = inventorySaveData;
    }
}
