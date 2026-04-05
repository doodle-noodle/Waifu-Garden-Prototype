// =============================================================================
// InventorySlotUI.cs  |  Scripts/UI
// WaifuGarden — Phase 1 Update
// Represents one item stack in the inventory panel.
// - Name label removed (shown in tooltip instead)
// - Hover shows TooltipSystem with item name, type, and description
// - Clicking moves item to hotbar
// =============================================================================

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class InventorySlotUI : MonoBehaviour,
    IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    // -------------------------------------------------------------------------
    [Header("Visual References")]
    // -------------------------------------------------------------------------

    [Tooltip("Displays the item icon. Disabled when slot is empty.")]
    public Image ItemIcon;

    [Tooltip("Shows stack count (e.g. '×3'). Hidden when count is 1 or slot is empty.")]
    public TextMeshProUGUI CountLabel;

    // NameLabel intentionally removed — name is shown in the tooltip on hover.

    // -------------------------------------------------------------------------
    // State
    // -------------------------------------------------------------------------

    public string ItemID    { get; private set; }
    public bool   IsEmpty   => string.IsNullOrEmpty(ItemID);

    private string _itemName;
    private string _itemType;
    private string _itemDescription;

    // -------------------------------------------------------------------------

    /// <summary>Populates this slot. Called by InventoryPanel on refresh.</summary>
    public void SetItem(string itemID, Sprite icon, string displayName,
                        string itemType, string description, int count)
    {
        ItemID           = itemID;
        _itemName        = displayName;
        _itemType        = itemType;
        _itemDescription = description;

        if (ItemIcon != null)
        {
            ItemIcon.sprite  = icon;
            ItemIcon.enabled = icon != null;
        }

        RefreshCount(count);
    }

    /// <summary>Marks this slot as empty (shows background only).</summary>
    public void SetEmpty()
    {
        ItemID           = null;
        _itemName        = "";
        _itemType        = "";
        _itemDescription = "";

        if (ItemIcon  != null) { ItemIcon.sprite = null; ItemIcon.enabled = false; }
        if (CountLabel != null) CountLabel.enabled = false;
    }

    public void RefreshCount(int count)
    {
        if (CountLabel == null) return;
        if (IsEmpty || count <= 1) { CountLabel.enabled = false; return; }
        CountLabel.text    = $"×{count}";
        CountLabel.enabled = true;
    }

    // -------------------------------------------------------------------------
    // Pointer events
    // -------------------------------------------------------------------------

    public void OnPointerClick(PointerEventData eventData)
    {
        if (IsEmpty) return;
        HotbarManager.Instance?.EquipToFirstEmpty(ItemID);
        AudioManager.Instance?.PlaySFX("ui_open");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (IsEmpty) return;
        TooltipSystem.Instance?.Show(_itemName, _itemType, _itemDescription);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipSystem.Instance?.Hide();
    }
}
