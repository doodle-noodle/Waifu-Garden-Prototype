// =============================================================================
// PlayerInventory.cs  |  Scripts/Player
// WaifuGarden — Phase 1  (replaces Phase 0 version — adds GetAllItemIDs)
// Runtime container for all player-held items and unsold harvested crops.
// Attach to the GameManager GameObject.
// =============================================================================

using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }

    // -------------------------------------------------------------------------
    // Stackable items — seeds, consumable tool uses, farm plots.
    // Key: ItemID string.  Value: quantity / remaining uses.
    // -------------------------------------------------------------------------
    private readonly Dictionary<string, int> _itemCounts = new Dictionary<string, int>();

    // Unsold harvested crops shown in the Sell tab.
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

    /// <summary>Returns all ItemIDs currently held (count > 0). Used by InventoryPanel.</summary>
    public IEnumerable<string> GetAllItemIDs() => _itemCounts.Keys;

    public void AddItem(string itemID, int quantity = 1)
    {
        if (string.IsNullOrEmpty(itemID) || quantity <= 0) return;
        _itemCounts[itemID] = GetItemCount(itemID) + quantity;
        Debug.Log($"[PlayerInventory] +{quantity}x {itemID}  (total: {_itemCounts[itemID]})");
        OnInventoryChanged?.Invoke();
    }

    /// <summary>Removes quantity of itemID. Returns false if insufficient.</summary>
    public bool RemoveItem(string itemID, int quantity = 1)
    {
        int current = GetItemCount(itemID);
        if (current < quantity)
        {
            Debug.LogWarning($"[PlayerInventory] Cannot remove {quantity}x {itemID} — only {current} held.");
            return false;
        }
        int remaining = current - quantity;
        if (remaining == 0) _itemCounts.Remove(itemID);
        else                _itemCounts[itemID] = remaining;
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
