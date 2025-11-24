using UnityEngine;

public class PlayerItemMagnet : MonoBehaviour
{
    [SerializeField] float _pullSpeed = 10f;
    [SerializeField] float _pickupInstance = 0.5f;

    Player _player;

    public void Initialize(Player player)
    {
        _player = player;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_player == null) return;

        var pickup = other.GetComponent<ItemPickup>();
        if (pickup == null) return;

        pickup.BeginAttract(_player, _pullSpeed, _pickupInstance);
    }

    private void OnTriggerExit(Collider other)
    {
        var pickup = other.GetComponent<ItemPickup>();
        if (pickup == null) return;

        pickup.StopAttract();
    }
}
