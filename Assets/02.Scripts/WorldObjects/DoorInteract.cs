using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorInteract : MonoBehaviour, IInteractable
{
    [SerializeField] string _targetSceneName;
    [SerializeField] string _targetSpawnId;

    public void Interact(PlayerInteraction interactor)
    {
        if (string.IsNullOrEmpty(_targetSceneName))
        {
            Debug.LogError("[DoorInteract::Interact] 대상 씬 이름이 설정되지 않았습니다.", this);
            return;
        }

        SceneTransitionData.PendingSpawnId = _targetSpawnId;

        SceneManager.LoadScene(_targetSceneName);
    }
}
