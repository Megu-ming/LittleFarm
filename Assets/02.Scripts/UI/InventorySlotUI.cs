using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [Header("참조")]
    [SerializeField] Image _background;
    [SerializeField] Image _icon;
    [SerializeField] TMP_Text _countText;

    [Header("색상")]
    [SerializeField] Color _normalColor = Color.white;
    [SerializeField] Color _lockedColor = new Color(0.2f, 0.2f, 0.2f, 1f);

    [SerializeField] int _slotIndex;
    public int SlotIndex => _slotIndex;

    InventoryUI _owner;

    public void SetOwner(InventoryUI owner)
    {
        _owner = owner;
    }

    public void SetIndex(int index)
    {
        _slotIndex = index;
        name = $"Slot_{index}";
    }

    public void Refresh(ItemStack stack, bool unlocked, ItemDatabase db)
    {
        if(_background != null)
        {
            _background.color = unlocked ? _normalColor : _lockedColor;
        }

        if(!unlocked)
        {
            if (_icon != null) _icon.enabled = false;
            if(_countText !=null) _countText.text = "";
            return;
        }

        if(stack == null || stack.IsEmpty)
        {
            if (_icon != null) _icon.enabled = false;
            if (_countText != null) _countText.text = "";
            return;
        }

        var spec = db.GetById(stack.ItemId);
        if(spec == null)
        {
            if (_icon != null) _icon.enabled = false;
            if (_countText != null) _countText.text = "";
            return;
        }

        if(_icon !=null)
        {
            if(spec.iconSprite !=null)
            {
                _icon.sprite = spec.iconSprite;
                _icon.enabled = true;
            }
            else
                _icon.enabled = false;
        }

        if(_countText!=null)
        {
            _countText.text = (stack.Count > 1) ? stack.Count.ToString() : "";
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _owner?.OnSlotBeginDrag(_slotIndex, eventData);
        Debug.Log($"{_slotIndex}_Slot BeginDrag");
    }

    public void OnDrag(PointerEventData eventData)
    {
        _owner?.OnSlotDrag(eventData);
        Debug.Log($"{_slotIndex}_Slot OnDrag");
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _owner?.OnSlotEndDrag(eventData);
        Debug.Log($"{_slotIndex}_Slot EndDrag");
    }

    public void OnDrop(PointerEventData eventData)
    {
        _owner?.OnSlotDrop(_slotIndex, eventData);
        Debug.Log($"{_slotIndex}_Slot Drop");
    }
}
