using UnityEngine;

public class GameInitializer : MonoBehaviour
{
    [Header("주요 객체 참조")]
    [SerializeField] Player _player;

    [Header("UI 참조")]
    [SerializeField] ToolChangerUI _toolChangerUI;

    private void Start()
    {
        _player.Initialize();
        _toolChangerUI.Initialize(_player);
    }
}
