using UnityEngine;

public class Tester : MonoBehaviour
{
    [SerializeField] Player player;

    public void AddPotatoButton()
    {
        player.TryPickupItem(3002, 10);
    }
}
