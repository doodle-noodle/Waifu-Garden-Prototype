// =============================================================================
// ShopBuyTabUI.cs  |  Scripts/UI
// WaifuGarden — Phase 3
// Full implementation of the Buy tab.
// - Lists all items where TotalCurrencyEarned >= UnlockAtTotalCurrency
// - Shows one teaser (???) entry for the next locked item
// - Refreshes automatically when currency changes (new unlocks appear instantly)
// - Fires UnlockNotification when a new item becomes available mid-session
// =============================================================================

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ShopBuyTabUI : MonoBehaviour
{
    [Header("References")]
    public RectTransform ContentContainer;
    public GameObject    ShopItemSlotPrefab;

    // -------------------------------------------------------------------------
    private readonly List<ShopItemSlotUI> _activeSlots = new List<ShopItemSlotUI>();

    // Tracks which item IDs were unlocked last refresh so we can detect new ones.
    private readonly HashSet<string> _previouslyUnlocked = new HashSet<string>();

    private bool _firstBuild = true;

    // -------------------------------------------------------------------------

    private void OnEnable()
    {
        if (PlayerStats.Instance != null)
            PlayerStats.Instance.OnCurrencyChanged += OnCurrencyChanged;

        Refresh();
    }

    private void OnDisable()
    {
        if (PlayerStats.Instance != null)
            PlayerStats.Instance.OnCurrencyChanged -= OnCurrencyChanged;
    }

    // -------------------------------------------------------------------------

    private void OnCurrencyChanged()
    {
        Refresh();
    }

    // -------------------------------------------------------------------------

    public void Refresh()
    {
        if (ContentContainer == null || ShopItemSlotPrefab == null) return;

        // Collect all purchasable items from DataRegistry.
        List<ShopItemData> allItems = BuildSortedItemList();

        // Split into unlocked and locked.
        float totalEarned = PlayerStats.Instance?.TotalCurrencyEarned ?? 0f;

        List<ShopItemData> unlocked = allItems
            .Where(i => i.UnlockAtTotalCurrency <= (long)totalEarned)
            .ToList();

        ShopItemData nextLocked = allItems
            .Where(i => i.UnlockAtTotalCurrency > (long)totalEarned)
            .OrderBy(i => i.UnlockAtTotalCurrency)
            .FirstOrDefault();

        // Detect newly unlocked items (skip on first build — everything is "new" then).
        if (!_firstBuild)
        {
            foreach (ShopItemData item in unlocked)
            {
                if (!_previouslyUnlocked.Contains(item.ItemID))
                {
                    UnlockNotification.Instance?.Show(item);
                    Debug.Log($"[ShopBuyTabUI] New unlock: {item.ItemName}");
                }
            }
        }

        _previouslyUnlocked.Clear();
        foreach (ShopItemData item in unlocked)
            _previouslyUnlocked.Add(item.ItemID);

        _firstBuild = false;

        // Rebuild the slot list.
        RebuildSlots(unlocked, nextLocked);
    }

    // -------------------------------------------------------------------------

    private void RebuildSlots(List<ShopItemData> unlocked, ShopItemData teaser)
    {
        foreach (Transform child in ContentContainer)
            Destroy(child.gameObject);
        _activeSlots.Clear();

        foreach (ShopItemData item in unlocked)
        {
            GameObject      go   = Instantiate(ShopItemSlotPrefab, ContentContainer);
            ShopItemSlotUI  slot = go.GetComponent<ShopItemSlotUI>();
            if (slot != null)
            {
                slot.SetItem(item);
                _activeSlots.Add(slot);
            }
        }

        // Teaser entry at the bottom.
        if (teaser != null)
        {
            GameObject      go   = Instantiate(ShopItemSlotPrefab, ContentContainer);
            ShopItemSlotUI  slot = go.GetComponent<ShopItemSlotUI>();
            slot?.SetTeaser(teaser);
        }
    }

    // -------------------------------------------------------------------------
    // Builds a combined, sorted list of all purchasable items.
    // Order: by UnlockAtTotalCurrency ascending, then BuyCost ascending.
    // -------------------------------------------------------------------------

    private List<ShopItemData> BuildSortedItemList()
    {
        var all = new List<ShopItemData>();

        if (DataRegistry.Instance == null) return all;

        // Gather tools
        foreach (ToolData tool in DataRegistry.Instance.AllTools)
            if (tool != null) all.Add(tool);

        // Gather non-tool shop items
        foreach (ShopItemData item in DataRegistry.Instance.AllShopItems)
            if (item != null) all.Add(item);

        return all
            .OrderBy(i => i.UnlockAtTotalCurrency)
            .ThenBy(i => i.BuyCost)
            .ToList();
    }
}
