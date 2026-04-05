// =============================================================================
// PlayerInventory.cs  |  Scripts/Player
// WaifuGarden — Pre-Phase 2 Fixes
// Fix: GetAllItemIDs() now returns a snapshot copy of the key collection,
// preventing InvalidOperationException if inventory changes during enumeration.
// =============================================================================

using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }

    private readonly Dictionary<string, int> _itemCounts = new Dictionary<string, int>();
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

    /// <summary>
    /// Returns a snapshot copy of all held ItemIDs.
    /// Safe to iterate even if inventory changes during the loop.
    /// </summary>
    public IEnumerable<string> GetAllItemIDs() => new List<string>(_itemCounts.Keys);

    public void AddItem(string itemID, int quantity = 1)
    {
        if (string.IsNullOrEmpty(itemID) || quantity <= 0) return;
        _itemCounts[itemID] = GetItemCount(itemID) + quantity;
        Debug.Log($"[PlayerInventory] +{quantity}x {itemID}  (total: {_itemCounts[itemID]})");
        OnInventoryChanged?.Invoke();
    }

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

    public event System.Action OnInventoryChanged;
}
