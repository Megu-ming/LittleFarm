using UnityEngine;

public interface IInteractable
{
    /// <summary>
    /// 플레이어가 상호작용 키를 눌렀을 때 호출되는 함수
    /// </summary>
    /// <param name="interactor">상호작용을 시도한 플레이어</param>
    void Interact(PlayerInteraction interactor);
}
