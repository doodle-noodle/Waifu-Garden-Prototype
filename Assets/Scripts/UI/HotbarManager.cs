// =============================================================================
// HotbarManager.cs  |  Scripts/UI
// WaifuGarden — Pre-Phase 2 Fixes
// Switched from legacy Input.GetKeyDown to UnityEngine.InputSystem.Keyboard.
// Requires: Input System package installed, Active Input Handling set to
// "Input System Package (New)" in Project Settings → Player.
// =============================================================================

using UnityEngine;
using UnityEngine.InputSystem;

public class HotbarManager : MonoBehaviour
{
    public static HotbarManager Instance { get; private set; }

    [Header("Hotbar Slots")]
    [Tooltip("Assign all 10 HotbarSlotUI components in order (slot 0 to slot 9).")]
    public HotbarSlotUI[] Slots = new HotbarSlotUI[10];

    private int _activeSlotIndex = -1;

    // -------------------------------------------------------------------------

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
    // New Input System — number keys 1–9 select slots 0–8, 0 selects slot 9.
    // -------------------------------------------------------------------------

    private void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.digit1Key.wasPressedThisFrame) { OnSlotClicked(0); return; }
        if (kb.digit2Key.wasPressedThisFrame) { OnSlotClicked(1); return; }
        if (kb.digit3Key.wasPressedThisFrame) { OnSlotClicked(2); return; }
        if (kb.digit4Key.wasPressedThisFrame) { OnSlotClicked(3); return; }
        if (kb.digit5Key.wasPressedThisFrame) { OnSlotClicked(4); return; }
        if (kb.digit6Key.wasPressedThisFrame) { OnSlotClicked(5); return; }
        if (kb.digit7Key.wasPressedThisFrame) { OnSlotClicked(6); return; }
        if (kb.digit8Key.wasPressedThisFrame) { OnSlotClicked(7); return; }
        if (kb.digit9Key.wasPressedThisFrame) { OnSlotClicked(8); return; }
        if (kb.digit0Key.wasPressedThisFrame) { OnSlotClicked(9); return; }
    }

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    public string GetActiveItemID()
    {
        if (_activeSlotIndex < 0 || _activeSlotIndex >= Slots.Length) return null;
        HotbarSlotUI slot = Slots[_activeSlotIndex];
        return (slot != null && !slot.IsEmpty) ? slot.EquippedItemID : null;
    }

    public bool EquipToFirstEmpty(string itemID)
    {
        if (string.IsNullOrEmpty(itemID)) return false;

        for (int i = 0; i < Slots.Length; i++)
            if (Slots[i] != null && Slots[i].EquippedItemID == itemID)
            { SetActiveSlot(i); return true; }

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

    public void ReturnSlotToInventory(int index)
    {
        if (index < 0 || index >= Slots.Length) return;
        HotbarSlotUI slot = Slots[index];
        if (slot == null || slot.IsEmpty) return;
        Debug.Log($"[HotbarManager] Cleared slot {index} ('{slot.EquippedItemID}').");
        slot.ClearItem();
        if (_activeSlotIndex == index) _activeSlotIndex = -1;
    }

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

    private void PopulateSlot(HotbarSlotUI slot, string itemID)
    {
        ShopItemData data = DataRegistry.Instance?.GetShopItem(itemID);
        slot.SetItem(itemID, data?.ItemIcon, data?.ItemName ?? itemID,
                     GetTypeLabel(data), data?.Description ?? "");
    }

    private string GetTypeLabel(ShopItemData data)
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
