// =============================================================================
// InventoryPanel.cs  |  Scripts/UI
// WaifuGarden — Phase 3
// Updated: shows harvested crops alongside seeds/tools in the inventory grid.
// Crops with identical PlantID + modifier set are displayed as a single stacked
// entry with a ×N count. Per-instance CropData is preserved in PlayerInventory.
// Crop slots are visually marked and are NOT equippable to the hotbar.
// =============================================================================

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventoryPanel : MonoBehaviour
{
    [Header("References")]
    public RectTransform ContentContainer;
    public GameObject    InventorySlotPrefab;

    [Tooltip("Total slots always visible. Empty ones show as blank backgrounds.")]
    public int TotalVisibleSlots = 24;

    // -------------------------------------------------------------------------
    private readonly List<InventorySlotUI> _slots = new List<InventorySlotUI>();
    private bool _initialized = false;

    // -------------------------------------------------------------------------

    private void OnDestroy()
    {
        if (PlayerInventory.Instance != null)
            PlayerInventory.Instance.OnInventoryChanged -= Refresh;
    }

    private void Initialize()
    {
        _initialized = true;
        if (PlayerInventory.Instance != null)
            PlayerInventory.Instance.OnInventoryChanged += Refresh;
        BuildSlots();
        Refresh();
    }

    // -------------------------------------------------------------------------

    private void BuildSlots()
    {
        if (ContentContainer == null || InventorySlotPrefab == null) return;
        foreach (Transform child in ContentContainer) Destroy(child.gameObject);
        _slots.Clear();

        for (int i = 0; i < TotalVisibleSlots; i++)
        {
            GameObject      go   = Instantiate(InventorySlotPrefab, ContentContainer);
            go.name              = $"InvSlot_{i:D2}";
            InventorySlotUI slot = go.GetComponent<InventorySlotUI>();
            if (slot != null) { slot.SetEmpty(); _slots.Add(slot); }
        }
    }

    private void Refresh()
    {
        if (PlayerInventory.Instance == null) return;

        // ---- 1. Regular stackable items (seeds, tools, farm plots) ----------
        var entries = new List<SlotEntry>();

        foreach (string itemID in PlayerInventory.Instance.GetAllItemIDs())
        {
            int count = PlayerInventory.Instance.GetItemCount(itemID);
            if (count <= 0) continue;

            ShopItemData data = DataRegistry.Instance?.GetShopItem(itemID);
            entries.Add(new SlotEntry
            {
                ItemID      = itemID,
                Icon        = data?.ItemIcon,
                DisplayName = data?.ItemName    ?? itemID,
                TypeLabel   = GetTypeLabel(data),
                Description = data?.Description ?? "",
                Count       = count,
                IsCrop      = false
            });
        }

        // ---- 2. Crops — group by PlantID + sorted modifier set --------------
        var cropGroups = new Dictionary<string, List<CropData>>();
        foreach (CropData crop in PlayerInventory.Instance.HarvestedCrops)
        {
            string key = BuildCropKey(crop);
            if (!cropGroups.ContainsKey(key)) cropGroups[key] = new List<CropData>();
            cropGroups[key].Add(crop);
        }

        foreach (var group in cropGroups.Values)
        {
            CropData representative = group[0];
            PlantData plantData = DataRegistry.Instance?.GetPlant(representative.PlantID);

            string modifierText = representative.AppliedModifierIDs != null
                                  && representative.AppliedModifierIDs.Count > 0
                ? string.Join(", ", representative.AppliedModifierIDs)
                : "No modifiers";

            entries.Add(new SlotEntry
            {
                ItemID      = representative.PlantID,
                Icon        = plantData?.MatureSprite,
                DisplayName = representative.PlantName,
                TypeLabel   = "Harvested Crop",
                Description = $"{modifierText}\n¥{representative.FinalSellValue:F0} per unit",
                Count       = group.Count,
                IsCrop      = true
            });
        }

        // ---- 3. Fill slots ---------------------------------------------------
        for (int i = 0; i < _slots.Count; i++)
        {
            if (i < entries.Count)
            {
                SlotEntry e = entries[i];
                _slots[i].SetItem(e.ItemID, e.Icon, e.DisplayName,
                                  e.TypeLabel, e.Description, e.Count);
                _slots[i].SetIsCrop(e.IsCrop);
            }
            else
            {
                _slots[i].SetEmpty();
                _slots[i].SetIsCrop(false);
            }
        }
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Generates a unique key for a crop group.
    /// Two crops are "the same" if they have the same PlantID and identical
    /// sorted modifier sets.
    /// </summary>
    private static string BuildCropKey(CropData crop)
    {
        if (crop.AppliedModifierIDs == null || crop.AppliedModifierIDs.Count == 0)
            return crop.PlantID;

        var sorted = new List<string>(crop.AppliedModifierIDs);
        sorted.Sort();
        return crop.PlantID + "|" + string.Join(",", sorted);
    }

    private static string GetTypeLabel(ShopItemData data)
    {
        if (data == null) return "Item";
        if (data is ToolData tool)
            return tool.Type switch
            {
                ToolType.Shovel      => "Tool — Shovel",
                ToolType.WateringCan => "Tool — Watering Can",
                ToolType.Fertilizer  => "Tool — Fertilizer",
                _                    => "Tool"
            };
        return data.ItemType switch
        {
            ShopItemType.Seed     => "Seed",
            ShopItemType.FarmPlot => "Farm Plot",
            ShopItemType.Tool     => "Tool",
            _                     => "Item"
        };
    }

    // -------------------------------------------------------------------------
    // Open / Close — wire Toggle() to the button in Inspector
    // -------------------------------------------------------------------------

    private bool _isOpen = false;

    public void Toggle()
    {
        if (!_initialized) Initialize();
        _isOpen = !_isOpen;
        gameObject.SetActive(_isOpen);
        if (_isOpen)  { Refresh(); AudioManager.Instance?.PlaySFX("ui_open"); }
        else          { TooltipSystem.Instance?.Hide(); AudioManager.Instance?.PlaySFX("ui_close"); }
    }

    public void Open()  { if (!_isOpen) Toggle(); }
    public void Close() { if (_isOpen)  Toggle(); }

    // -------------------------------------------------------------------------
    // Internal data container
    // -------------------------------------------------------------------------

    private class SlotEntry
    {
        public string ItemID;
        public UnityEngine.Sprite Icon;
        public string DisplayName;
        public string TypeLabel;
        public string Description;
        public int    Count;
        public bool   IsCrop;
    }
}
