using System.Collections;
using UnityEngine;

/// <summary>
/// 월드에 드랍되는 아이템
/// </summary>
public class ItemPickup : MonoBehaviour
{
    [Header("아이템 정보")]
    [SerializeField] int _itemId;
    [SerializeField] int _amount = 1;

    [Header("드랍 연출 설정")]
    [SerializeField] float _dropEffectDuration = 1f;

    float _moveSpeed;
    float _pickupDistance;

    Player _targetPlayer;

    // flags
    bool _dropEffectPlaying = false;
    bool _magnetRequested   = false;
    bool _canBeAttracted    = false;
    bool _isAttracting      = false;

    public void Setup(int itemId, int amount)
    {
        _itemId = itemId;
        _amount = amount;
    }

    public void PlayDropEffect()
    {
        _dropEffectPlaying  = true;
        _canBeAttracted     = false;

        // 애니메이션 or 파이클 재생

        if (_dropEffectDuration > 0)
        {
            StopAllCoroutines();
            StartCoroutine(DropEffectTimer());
        }
        else
            OnDropEffectFinished();
    }

    IEnumerator DropEffectTimer()
    {
        yield return new WaitForSeconds(_dropEffectDuration);
        OnDropEffectFinished();
    }

    public void OnDropEffectFinished()
    {
        _dropEffectPlaying = false;
        _canBeAttracted    = true;

        TryStartAttract();
    }

    public void BeginAttract(Player player, float pullSpeed, float pickupDistance)
    {
        _targetPlayer = player;
        _moveSpeed = pullSpeed;
        _pickupDistance = pickupDistance;
        _magnetRequested = true;

        TryStartAttract();
    }

    public void StopAttract()
    {
        _isAttracting = false;
    }

    private void TryStartAttract()
    {
        if (_dropEffectPlaying)
            return;
        if (_isAttracting)
            return;
        if (!_magnetRequested)
            return;
        if (!_canBeAttracted)
            return;
        if (_targetPlayer == null)
            return;
        _isAttracting = true;
    }

    private void Update()
    {
        if (!_isAttracting || _targetPlayer == null) return;

        Vector3 targetPos = _targetPlayer.transform.position + Vector3.up * 0.5f;

        transform.position = Vector3.MoveTowards(transform.position, targetPos, _moveSpeed * Time.deltaTime);

        float sqrDist = (transform.position - targetPos).sqrMagnitude;
        if(sqrDist <= _pickupDistance*_pickupDistance)
        {
            bool picked = _targetPlayer.TryPickupItem(_itemId, _amount);
            if (picked)
                Destroy(gameObject);
            else
                _isAttracting = false;
        }
    }
}
