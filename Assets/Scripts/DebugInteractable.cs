using UnityEngine;

public class DebugInteractable : MonoBehaviour, IInteractable
{
    public string debugName = "상호작용 오브젝트";

    public void Interact(PlayerInteraction interactor)
    {
        Debug.Log($"[DebugInteractable] '{debugName}' 이(가) 상호작용 되었습니다. by {interactor.name}");
    }
}
