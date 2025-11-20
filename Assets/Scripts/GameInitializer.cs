using UnityEngine;

public class GameInitializer : MonoBehaviour
{
    [SerializeField] Player _player;
    [SerializeField] ToolChangerUI _toolChangerUI;

    private void Start()
    {
        _player.Initialize();
        _toolChangerUI.Initialize(_player);
    }
}
