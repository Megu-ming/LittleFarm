using UnityEngine;

public class GameSession : MonoBehaviour
{
    public static GameSession Instance { get; private set; }

    [SerializeField] GlobalGameState _globalState;

    public void Initialize()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if(_globalState == null)
            _globalState = new GlobalGameState();

        Debug.Log("GameSession Initialized.");
    }

    // 씬 넘어가기 직전 호출: 현재 상태를 GlobalState에 백업
    public void CaptureState(PlayerController player, GameTimeManager timeMgr)
    {
        // 1. 시간 저장
        //_globalState.CurrentTime = timeMgr.;

        // 2. 인벤토리 저장 (구조에 따라 다를 수 있음)
        // GlobalState.InventoryItemIds = invMgr.GetItemIds(); 

        // 3. 플레이어 상태
        // GlobalState.CurrentHandIndex = player.CurrentHandIndex;
    }

    // 새 씬 로드 후 호출: GlobalState의 데이터를 게임에 적용
    public void RestoreState(PlayerController player, GameTimeManager timeMgr)
    {
        // 1. 시간 복원
        //timeMgr.SetTime(GlobalState.CurrentTime);

        // 2. 인벤토리 복원
        // invMgr.LoadItems(GlobalState.InventoryItemIds);

        // 3. 위치 복원 (별도 SpawnPoint 로직으로 처리)
        // player.WarpTo(GlobalState.TargetSpawnPointID);
    }
}
