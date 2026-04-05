// =============================================================================
// HotbarManager.cs  |  Scripts/UI
// WaifuGarden — Phase 1 Update
// Passes full display data (icon, name, type, description) to HotbarSlotUI
// when equipping items, so the slot can show the icon and tooltip correctly.
// Number-key navigation (1–0) was already implemented; verified here.
// =============================================================================

using UnityEngine;

public class HotbarManager : MonoBehaviour
{
    public static HotbarManager Instance { get; private set; }

    // -------------------------------------------------------------------------
    [Header("Hotbar Slots")]
    [Tooltip("Assign all 10 HotbarSlotUI components in order (slot 0 to slot 9).")]
    public HotbarSlotUI[] Slots = new HotbarSlotUI[10];

    // -------------------------------------------------------------------------
    private int _activeSlotIndex = -1;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        for (int i = 0; i < Slots.Length; i++)
            Slots[i]?.Initialise(i);

        if (PlayerInventory.Instance != null)
            PlayerInventory.Instance.OnInventoryChanged += RefreshAllCountLabels;
    }

    private void OnDestroy()
    {
        if (PlayerInventory.Instance != null)
            PlayerInventory.Instance.OnInventoryChanged -= RefreshAllCountLabels;
    }

    // -------------------------------------------------------------------------
    // Number key input — 1–9 select slots 0–8, 0 selects slot 9.
    // -------------------------------------------------------------------------

    private void Update()
    {
        for (int i = 0; i < 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            { OnSlotClicked(i); return; }
        }
        if (Input.GetKeyDown(KeyCode.Alpha0)) OnSlotClicked(9);
    }

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    /// <summary>Returns the ItemID in the active slot, or null if none.</summary>
    public string GetActiveItemID()
    {
        if (_activeSlotIndex < 0 || _activeSlotIndex >= Slots.Length) return null;
        HotbarSlotUI slot = Slots[_activeSlotIndex];
        return (slot != null && !slot.IsEmpty) ? slot.EquippedItemID : null;
    }

    /// <summary>
    /// Moves an item from inventory to the first empty hotbar slot.
    /// Fetches icon, name, type, and description from DataRegistry to populate the slot.
    /// </summary>
    public bool EquipToFirstEmpty(string itemID)
    {
        if (string.IsNullOrEmpty(itemID)) return false;

        // If already equipped somewhere, just activate that slot.
        for (int i = 0; i < Slots.Length; i++)
        {
            if (Slots[i] != null && Slots[i].EquippedItemID == itemID)
            { SetActiveSlot(i); return true; }
        }

        // Find first empty slot.
        for (int i = 0; i < Slots.Length; i++)
        {
            if (Slots[i] != null && Slots[i].IsEmpty)
            {
                PopulateSlot(Slots[i], itemID);
                SetActiveSlot(i);
                Debug.Log($"[HotbarManager] Equipped '{itemID}' to slot {i}.");
                return true;
            }
        }

        Debug.Log("[HotbarManager] Hotbar is full.");
        return false;
    }

    /// <summary>Left-click on a slot: select it, or deselect if already active.</summary>
    public void OnSlotClicked(int index)
    {
        if (index < 0 || index >= Slots.Length) return;
        HotbarSlotUI slot = Slots[index];
        if (slot == null) return;

        if (!slot.IsEmpty && _activeSlotIndex == index)
            ReturnSlotToInventory(index);
        else
            SetActiveSlot(index);
    }

    /// <summary>Right-click or second-click: clear the slot.</summary>
    public void ReturnSlotToInventory(int index)
    {
        if (index < 0 || index >= Slots.Length) return;
        HotbarSlotUI slot = Slots[index];
        if (slot == null || slot.IsEmpty) return;

        Debug.Log($"[HotbarManager] Cleared slot {index} ('{slot.EquippedItemID}').");
        slot.ClearItem();
        if (_activeSlotIndex == index) _activeSlotIndex = -1;
    }

    /// <summary>Called by GridManager when the last of an item has been consumed.</summary>
    public void ClearSlotIfEmpty(string itemID)
    {
        if (PlayerInventory.Instance != null && PlayerInventory.Instance.HasItem(itemID)) return;
        for (int i = 0; i < Slots.Length; i++)
        {
            if (Slots[i] != null && Slots[i].EquippedItemID == itemID)
            {
                Slots[i].ClearItem();
                if (_activeSlotIndex == i) _activeSlotIndex = -1;
                break;
            }
        }
    }

    // -------------------------------------------------------------------------
    // Internal helpers
    // -------------------------------------------------------------------------

    private void PopulateSlot(HotbarSlotUI slot, string itemID)
    {
        ShopItemData data = DataRegistry.Instance?.GetShopItem(itemID);

        string displayName  = data != null ? data.ItemName    : itemID;
        string description  = data != null ? data.Description : "";
        Sprite icon         = data != null ? data.ItemIcon    : null;
        string typeLabel    = GetTypeLabel(data);

        slot.SetItem(itemID, icon, displayName, typeLabel, description);
    }

    private string GetTypeLabel(ShopItemData data)
    {
        if (data == null) return "Item";
        if (data is ToolData tool)
        {
            return tool.Type switch
            {
                ToolType.Shovel      => "Tool — Shovel",
                ToolType.WateringCan => "Tool — Watering Can",
                ToolType.Fertilizer  => "Tool — Fertilizer",
                _                    => "Tool"
            };
        }
        return data.ItemType switch
        {
            ShopItemType.Seed     => "Seed",
            ShopItemType.FarmPlot => "Farm Plot",
            ShopItemType.Tool     => "Tool",
            _                     => "Item"
        };
    }

    private void SetActiveSlot(int index)
    {
        if (_activeSlotIndex >= 0 && _activeSlotIndex < Slots.Length)
            Slots[_activeSlotIndex]?.SetHighlight(false);

        _activeSlotIndex = index;

        if (_activeSlotIndex >= 0 && _activeSlotIndex < Slots.Length)
            Slots[_activeSlotIndex]?.SetHighlight(true);
    }

    private void RefreshAllCountLabels()
    {
        foreach (HotbarSlotUI slot in Slots) slot?.RefreshCountLabel();
    }
}
