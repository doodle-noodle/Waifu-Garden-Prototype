// =============================================================================
// PlayerInventory.cs  |  Scripts/Player
// WaifuGarden — Phase 0
// Runtime container for all player-held items and unsold harvested crops.
// Fully implemented here (no Phase 1 changes needed beyond wiring references).
// Attach to the GameManager GameObject.
// =============================================================================

using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }

    // -------------------------------------------------------------------------
    // Stackable items: seeds, consumable tools (WateringCan uses, Fertilizer),
    // permanent tools (Shovel), and farm plot items.
    // Key: ItemID string.  Value: quantity / remaining uses.
    // -------------------------------------------------------------------------
    private readonly Dictionary<string, int> _itemCounts = new Dictionary<string, int>();

    // -------------------------------------------------------------------------
    // Unsold harvested crops, displayed in the shop Sell tab.
    // -------------------------------------------------------------------------
    public List<CropData> HarvestedCrops { get; private set; } = new List<CropData>();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // -------------------------------------------------------------------------
    // Item API
    // -------------------------------------------------------------------------

    public int  GetItemCount(string itemID) { _itemCounts.TryGetValue(itemID, out int n); return n; }
    public bool HasItem(string itemID)      => GetItemCount(itemID) > 0;

    public void AddItem(string itemID, int quantity = 1)
    {
        if (string.IsNullOrEmpty(itemID) || quantity <= 0) return;
        _itemCounts[itemID] = GetItemCount(itemID) + quantity;
        Debug.Log($"[PlayerInventory] +{quantity}x {itemID}  (total: {_itemCounts[itemID]})");
        OnInventoryChanged?.Invoke();
    }

    /// <summary>Removes quantity of itemID. Returns false if not enough held.</summary>
    public bool RemoveItem(string itemID, int quantity = 1)
    {
        int current = GetItemCount(itemID);
        if (current < quantity)
        {
            Debug.LogWarning($"[PlayerInventory] Cannot remove {quantity}x {itemID} — only have {current}.");
            return false;
        }
        _itemCounts[itemID] = current - quantity;
        if (_itemCounts[itemID] <= 0) _itemCounts.Remove(itemID);
        OnInventoryChanged?.Invoke();
        return true;
    }

    // -------------------------------------------------------------------------
    // Crop API
    // -------------------------------------------------------------------------

    public void AddCrop(CropData crop)
    {
        if (crop == null) return;
        HarvestedCrops.Add(crop);
        OnInventoryChanged?.Invoke();
    }

    public bool RemoveCrop(CropData crop)
    {
        bool removed = HarvestedCrops.Remove(crop);
        if (removed) OnInventoryChanged?.Invoke();
        return removed;
    }

    // -------------------------------------------------------------------------
    // Events
    // -------------------------------------------------------------------------

    /// <summary>Fired whenever inventory contents change. All inventory UI subscribes.</summary>
    public event System.Action OnInventoryChanged;
}
