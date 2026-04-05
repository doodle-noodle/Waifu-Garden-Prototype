// =============================================================================
// HotbarSlotUI.cs  |  Scripts/UI
// WaifuGarden — Phase 1 Update
// - Added SlotNumberLabel shown permanently in the top-left of each slot
// - Added tooltip on hover (shows item name, type, description)
// - Fixed icon display: icon is always shown when an item is equipped
// =============================================================================

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class HotbarSlotUI : MonoBehaviour,
    IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    // -------------------------------------------------------------------------
    [Header("Visual References")]
    // -------------------------------------------------------------------------

    [Tooltip("Image showing the equipped item icon. Disabled when empty.")]
    public Image ItemIcon;

    [Tooltip("Image overlay shown when this slot is the active (selected) slot.")]
    public Image HighlightFrame;

    [Tooltip("Stack count label (e.g. '×3'). Hidden when count is 1 or slot is empty.")]
    public TextMeshProUGUI CountLabel;

    [Tooltip("Permanent number label in the top-left corner. Shows 1–9 then 0 for slot 9.")]
    public TextMeshProUGUI SlotNumberLabel;

    // -------------------------------------------------------------------------
    // State
    // -------------------------------------------------------------------------

    public string EquippedItemID { get; private set; } = "";
    public int    SlotIndex      { get; private set; }
    public bool   IsEmpty        => string.IsNullOrEmpty(EquippedItemID);

    private string _itemName;
    private string _itemType;
    private string _itemDescription;

    // -------------------------------------------------------------------------

    public void Initialise(int index)
    {
        SlotIndex = index;

        // Set the permanent number label: slots 0–8 → "1"–"9", slot 9 → "0"
        if (SlotNumberLabel != null)
        {
            SlotNumberLabel.text    = index < 9 ? (index + 1).ToString() : "0";
            SlotNumberLabel.enabled = true;
        }

        ClearItem();
    }

    // -------------------------------------------------------------------------
    // Item management
    // -------------------------------------------------------------------------

    /// <summary>Equips an item into this slot with full display data.</summary>
    public void SetItem(string itemID, Sprite icon, string displayName,
                        string itemType, string description)
    {
        EquippedItemID   = itemID;
        _itemName        = displayName;
        _itemType        = itemType;
        _itemDescription = description;

        if (ItemIcon != null)
        {
            ItemIcon.sprite  = icon;
            // Show a white box if no sprite exists, so player knows slot is filled.
            ItemIcon.enabled = true;
        }

        RefreshCountLabel();
    }

    /// <summary>Clears this slot completely.</summary>
    public void ClearItem()
    {
        EquippedItemID   = "";
        _itemName        = "";
        _itemType        = "";
        _itemDescription = "";

        if (ItemIcon   != null) { ItemIcon.sprite = null; ItemIcon.enabled = false; }
        if (CountLabel != null) CountLabel.enabled = false;
        SetHighlight(false);
    }

    /// <summary>Reads current count from PlayerInventory and refreshes the label.</summary>
    public void RefreshCountLabel()
    {
        if (CountLabel == null || IsEmpty) { if (CountLabel != null) CountLabel.enabled = false; return; }
        int count = PlayerInventory.Instance != null
            ? PlayerInventory.Instance.GetItemCount(EquippedItemID) : 0;
        CountLabel.text    = count > 1 ? $"×{count}" : "";
        CountLabel.enabled = count > 1;
    }

    // -------------------------------------------------------------------------
    // Highlight
    // -------------------------------------------------------------------------

    public void SetHighlight(bool active)
    {
        if (HighlightFrame != null) HighlightFrame.enabled = active;
    }

    // -------------------------------------------------------------------------
    // Pointer events
    // -------------------------------------------------------------------------

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
            HotbarManager.Instance?.ReturnSlotToInventory(SlotIndex);
        else
            HotbarManager.Instance?.OnSlotClicked(SlotIndex);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!IsEmpty)
            TooltipSystem.Instance?.Show(_itemName, _itemType, _itemDescription);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipSystem.Instance?.Hide();
    }
}
