// =============================================================================
// InventoryPanel.cs  |  Scripts/UI
// WaifuGarden — Phase 1 Update 5
// Uses lazy initialization: slots are built the first time Toggle() is called,
// not during Start(). Panel never activates itself during scene startup,
// so there is no state mismatch causing the 3-click bug.
// =============================================================================

using System.Collections.Generic;
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
    private bool _isOpen       = false;
    private bool _initialized  = false;

    // -------------------------------------------------------------------------

    // No Start() needed. The panel starts inactive and stays that way
    // until the player clicks the Inventory button for the first time.

    private void OnDestroy()
    {
        if (PlayerInventory.Instance != null)
            PlayerInventory.Instance.OnInventoryChanged -= Refresh;
    }

    // -------------------------------------------------------------------------
    // Called once, the first time Toggle() is called.
    // -------------------------------------------------------------------------

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

        foreach (Transform child in ContentContainer)
            Destroy(child.gameObject);
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

        var items = new List<(string id, int count)>();
        foreach (string id in PlayerInventory.Instance.GetAllItemIDs())
        {
            int n = PlayerInventory.Instance.GetItemCount(id);
            if (n > 0) items.Add((id, n));
        }

        for (int i = 0; i < _slots.Count; i++)
        {
            if (i < items.Count)
            {
                string       itemID = items[i].id;
                ShopItemData data   = DataRegistry.Instance?.GetShopItem(itemID);
                _slots[i].SetItem(
                    itemID,
                    data?.ItemIcon,
                    data?.ItemName    ?? itemID,
                    GetTypeLabel(data),
                    data?.Description ?? "",
                    items[i].count);
            }
            else { _slots[i].SetEmpty(); }
        }
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
    // Wire this to the Inventory button AND the close button in the Inspector.
    // -------------------------------------------------------------------------
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
}
