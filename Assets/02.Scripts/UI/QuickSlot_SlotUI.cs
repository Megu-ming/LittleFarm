using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuickSlot_SlotUI : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] Toggle _toggle;
    [SerializeField] Image _icon;
    [SerializeField] TMP_Text _countText;

    QuickSlotUI _owner;
    Player _player;
    int _index;

    public void Initialize(QuickSlotUI ui, Player player, int index)
    {
        _owner = ui;
        _player = player;
        _index = index;

        _toggle.group = ui.GetComponent<ToggleGroup>();
        _toggle.onValueChanged.AddListener(SetHandPosition);
    }

    public void SetHandPosition(bool value)
    {
        if (!value) return;
        _player.SetHand(_index);
    }

    public void Refresh(ItemStack stack, ItemDatabase db)
    {
        if (stack == null || stack.IsEmpty)
        {
            if (_icon != null) _icon.enabled = false;
            if (_countText != null) _countText.text = "";
            return;
        }

        var spec = db.GetById(stack.ItemId);
        if (spec == null)
        {
            if (_icon != null) _icon.enabled = false;
            if (_countText != null) _countText.text = "";
            return;
        }

        if (_icon != null)
        {
            if (spec.iconSprite != null)
            {
                _icon.sprite = spec.iconSprite;
                _icon.enabled = true;
            }
            else
                _icon.enabled = false;
        }

        if (_countText != null)
        {
            _countText.text = (stack.Count > 1) ? stack.Count.ToString() : "";
        }
    }
}
