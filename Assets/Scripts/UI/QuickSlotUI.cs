using UnityEngine;
using UnityEngine.EventSystems;

public class QuickSlotUI : MonoBehaviour
{
    [SerializeField] Transform handTransform;
    [SerializeField] InventorySlotUI[] quickSlots;

    public void SetHandPosition()
    {

        var slotRect = quickSlots[index].GetComponent<RectTransform>();
        if(slotRect!=null)
        {
            handTransform.transform.position = slotRect.position;
        }
    }
}
