using UnityEngine;

/// <summary>
/// 전역에서(게임 전체에서) 하나만 존재하도록 보장되는 게임 매니저 역할.
/// 게임 전체에 필요한 각 매니저들을 관리한다.
/// </summary>
public class GameManager : Singleton<GameManager>
{
    DataManager _dataManager;
    ObjectManager _objectManager;

    public DataManager DataManager => _dataManager;
    public ObjectManager ObjectManager => _objectManager;

    protected override void Awake()
    {
        base.Awake();

        _dataManager = gameObject.GetOrAddComponent<DataManager>();
        _objectManager = gameObject.GetOrAddComponent<ObjectManager>();
    }

    /// <summary>
    /// GameTimeManager의 ForceNextDay에서 데이터 저장
    /// </summary>
    public void Save()
    {
        _dataManager.Save();
    }

    /// <summary>
    /// GameInitializer에서 Initialize할 때 데이터 불러오기
    /// </summary>
    public void Load()
    {
        _dataManager.Load();
    }
}
