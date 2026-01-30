using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GlobalGameState
{
    // 1. 위치, 스폰 정보
    [SerializeField] string _targetSpawnId;

    // 2. 시간 정보
    [SerializeField] DateTime _currentTime;

    // 3. 플레이어 상태
    [SerializeField] List<string> _inventoryItemIds; // (임시) 아이템 ID 목록 또는 실제 데이터 구조
    [SerializeField] int _currentHandIndex;          // 현재 들고 있는 퀵슬롯 번호

    public string TargetSpawnId => _targetSpawnId;
    public DateTime CurrentTime => _currentTime;
    public List<string> InventoryItemIds => _inventoryItemIds;
    public int CurrentHandIndex => _currentHandIndex;

    public GlobalGameState()
    {
        _currentTime = new DateTime(1, 1, 1, 6, 0, 0); // 1년 1월 1일 06:00
        _inventoryItemIds = new List<string>();
        _targetSpawnId = "DefaultSpawn"; // 기본 스폰 위치
    }
}
