// =============================================================================
// InventorySlotUI.cs  |  Scripts/UI
// WaifuGarden — Phase 3
// Updated: SetIsCrop() applies a visual tint to crop slots and prevents them
// from being equipped to the hotbar (clicking a crop slot shows tooltip only).
// =============================================================================

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class InventorySlotUI : MonoBehaviour,
    IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Visual References")]
    public Image           ItemIcon;
    public TextMeshProUGUI CountLabel;

    [Header("Crop Visual")]
    [Tooltip("Tint applied to the slot background when this slot holds a crop. " +
             "Gives crops a subtle visual distinction from equippable items.")]
    public Color CropTint     = new Color(0.85f, 1f, 0.85f, 1f); // soft green
    public Color DefaultTint  = Color.white;

    // -------------------------------------------------------------------------
    public string ItemID  { get; private set; }
    public bool   IsEmpty => string.IsNullOrEmpty(ItemID);

    private string _itemName;
    private string _itemType;
    private string _itemDescription;
    private bool   _isCrop;

    private Image _backgroundImage; // the slot's own root Image

    // -------------------------------------------------------------------------

    private void Awake()
    {
        _backgroundImage = GetComponent<Image>();
    }

    // -------------------------------------------------------------------------

    public void SetItem(string itemID, Sprite icon, string displayName,
                        string itemType, string description, int count)
    {
        ItemID           = itemID;
        _itemName        = displayName;
        _itemType        = itemType;
        _itemDescription = description;

        if (ItemIcon != null) { ItemIcon.sprite = icon; ItemIcon.enabled = icon != null; }
        RefreshCount(count);
    }

    public void SetEmpty()
    {
        ItemID = null;
        _itemName = _itemType = _itemDescription = "";
        if (ItemIcon   != null) { ItemIcon.sprite = null; ItemIcon.enabled = false; }
        if (CountLabel != null) CountLabel.enabled = false;
        SetIsCrop(false);
    }

    /// <summary>
    /// Marks this slot as a crop slot.
    /// Crops are NOT equippable to the hotbar — clicking shows tooltip only.
    /// A tint is applied to distinguish them visually.
    /// </summary>
    public void SetIsCrop(bool isCrop)
    {
        _isCrop = isCrop;
        if (_backgroundImage != null)
            _backgroundImage.color = isCrop ? CropTint : DefaultTint;
    }

    public void RefreshCount(int count)
    {
        if (CountLabel == null) return;
        CountLabel.text    = count > 1 ? $"×{count}" : "";
        CountLabel.enabled = count > 1;
    }

    // -------------------------------------------------------------------------
    // Pointer events
    // -------------------------------------------------------------------------

    public void OnPointerClick(PointerEventData eventData)
    {
        if (IsEmpty) return;

        if (_isCrop)
        {
            // Crops are not equippable — tooltip on hover is sufficient.
            // Future phase: clicking a crop could show detail view.
            return;
        }

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
