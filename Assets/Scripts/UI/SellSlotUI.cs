// =============================================================================
// SellSlotUI.cs  |  Scripts/UI
// WaifuGarden — Phase 2
// Represents one unsold crop entry in the Shop Sell tab.
// Shows: crop name, applied modifier list, final sell value, Sell button.
// Dynamically created by ShopSellTabUI — do not add to scene manually.
// =============================================================================

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SellSlotUI : MonoBehaviour
{
    [Header("Text Fields")]
    public TextMeshProUGUI CropNameLabel;
    public TextMeshProUGUI ModifierListLabel;
    public TextMeshProUGUI SellValueLabel;

    [Header("Button")]
    public Button SellButton;

    // -------------------------------------------------------------------------
    private CropData _crop;

    // -------------------------------------------------------------------------

    public void SetCrop(CropData crop)
    {
        _crop = crop;

        if (CropNameLabel    != null) CropNameLabel.text    = crop.PlantName;
        if (SellValueLabel   != null) SellValueLabel.text   = $"¥{crop.FinalSellValue:F0}";

        if (ModifierListLabel != null)
        {
            if (crop.AppliedModifierIDs != null && crop.AppliedModifierIDs.Count > 0)
                ModifierListLabel.text = string.Join(", ", crop.AppliedModifierIDs);
            else
                ModifierListLabel.text = "No modifiers";
        }

        if (SellButton != null)
        {
            SellButton.onClick.RemoveAllListeners();
            SellButton.onClick.AddListener(SellThis);
        }
    }

    private void SellThis()
    {
        if (_crop == null) return;

        PlayerStats.Instance?.EarnCurrency(_crop.FinalSellValue);
        PlayerInventory.Instance?.RemoveCrop(_crop);
        AudioManager.Instance?.PlaySFX("item_sold");

        Debug.Log($"[SellSlotUI] Sold {_crop.PlantName} for ¥{_crop.FinalSellValue:F0}.");

        // ShopSellTabUI will rebuild the list on the next OnInventoryChanged event.
        Destroy(gameObject);
    }
}
