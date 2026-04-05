// =============================================================================
// GridManager.cs  |  Scripts/Grid
// WaifuGarden — Phase 1 Update 4  (fresh start)
//
// APPROACH CHANGE: GridManager no longer spawns slot prefabs at runtime.
// Instead, you build the grid visually in the Editor (drag SlotPrefab children
// into GridContainer), save it as GridPrefab, and GridManager simply reads the
// SlotController components that already exist in the scene.
//
// Benefits:
//   - You can see and adjust the grid in Edit mode with no code changes
//   - No Canvas layout timing issues
//   - GridContainer is a normal prefab you can modify freely
//
// Setup: see Unity instructions below the script.
// =============================================================================

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    // -------------------------------------------------------------------------
    [Header("Grid Container")]
    [Tooltip("The RectTransform that contains all 16 (or N) SlotController children. " +
             "These are pre-built in the Editor — not spawned at runtime.")]
    public RectTransform GridContainer;

    [Header("Starting Farm Plots")]
    [Tooltip("Slot indices (0-based, left-to-right top-to-bottom) that begin with a farm plot. " +
             "Default: 5,6,9,10 = centre 2×2 of a 4×4 grid.")]
    public List<int> StartingFarmPlotIndices = new List<int> { 5, 6, 9, 10 };

    // -------------------------------------------------------------------------
    private SlotController[] _slots;

    // -------------------------------------------------------------------------

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        InitialiseSlots();
        PlaceStartingFarmPlots();
    }

    // -------------------------------------------------------------------------

    /// <summary>
    /// Reads all SlotController components that are already children of GridContainer.
    /// Order matches the Hierarchy order (top to bottom = slot 0, 1, 2 ...).
    /// </summary>
    private void InitialiseSlots()
    {
        if (GridContainer == null)
        {
            Debug.LogError("[GridManager] GridContainer not assigned!");
            return;
        }

        // GetComponentsInChildren returns components in Hierarchy order.
        SlotController[] found = GridContainer.GetComponentsInChildren<SlotController>(true);

        if (found.Length == 0)
        {
            Debug.LogError("[GridManager] No SlotController components found inside GridContainer. " +
                           "Make sure you have built the grid in the Editor (see setup instructions).");
            return;
        }

        _slots = found;

        for (int i = 0; i < _slots.Length; i++)
            _slots[i].Initialise(i);

        Debug.Log($"[GridManager] Initialised {_slots.Length} pre-built slots.");
    }

    private void PlaceStartingFarmPlots()
    {
        if (_slots == null) return;
        foreach (int idx in StartingFarmPlotIndices)
        {
            if (idx < 0 || idx >= _slots.Length)
            { Debug.LogWarning($"[GridManager] Farm plot index {idx} out of range."); continue; }
            _slots[idx].PlaceFarmPlot();
        }
        Debug.Log($"[GridManager] Placed {StartingFarmPlotIndices.Count} starting farm plots.");
    }

    // -------------------------------------------------------------------------
    // Slot accessors
    // -------------------------------------------------------------------------

    public SlotController          GetSlot(int index)  =>
        (_slots != null && index >= 0 && index < _slots.Length) ? _slots[index] : null;

    public IEnumerable<SlotController> GetAllSlots()   => _slots;

    // -------------------------------------------------------------------------
    // Click routing — called by SlotController.OnPointerClick
    // -------------------------------------------------------------------------

    public void HandleSlotClick(SlotController slot, PointerEventData.InputButton button)
    {
        if (slot == null || button == PointerEventData.InputButton.Right) return;

        string activeItem = HotbarManager.Instance?.GetActiveItemID();

        if (string.IsNullOrEmpty(activeItem)) { TryHarvest(slot); return; }

        ShopItemData itemData = DataRegistry.Instance?.GetShopItem(activeItem);
        if (itemData == null) return;

        switch (itemData.ItemType)
        {
            case ShopItemType.Seed:     TryPlantSeed(slot, itemData);                       break;
            case ShopItemType.FarmPlot: TryPlaceFarmPlot(slot, activeItem);                 break;
            case ShopItemType.Tool:     TryUseTool(slot, activeItem, itemData as ToolData); break;
        }
    }

    // -------------------------------------------------------------------------
    // Action implementations
    // -------------------------------------------------------------------------

    private void TryPlantSeed(SlotController slot, ShopItemData seedData)
    {
        if (slot.State != SlotState.FarmPlot_Empty) return;
        PlantData plantData = DataRegistry.Instance?.GetPlant(seedData.LinkedPlantID);
        if (plantData == null) { Debug.LogWarning($"[GridManager] No PlantData for '{seedData.LinkedPlantID}'."); return; }
        if (!PlayerInventory.Instance.HasItem(seedData.ItemID)) return;

        slot.PlantSeed(plantData);
        PlayerInventory.Instance.RemoveItem(seedData.ItemID);
        if (!PlayerInventory.Instance.HasItem(seedData.ItemID))
            HotbarManager.Instance?.ClearSlotIfEmpty(seedData.ItemID);
    }

    private void TryPlaceFarmPlot(SlotController slot, string itemID)
    {
        if (slot.State != SlotState.Empty) return;
        if (!PlayerInventory.Instance.HasItem(itemID)) return;
        slot.PlaceFarmPlot();
        PlayerInventory.Instance.RemoveItem(itemID);
        if (!PlayerInventory.Instance.HasItem(itemID))
            HotbarManager.Instance?.ClearSlotIfEmpty(itemID);
    }

    private void TryUseTool(SlotController slot, string itemID, ToolData toolData)
    {
        if (toolData == null) return;
        switch (toolData.Type)
        {
            case ToolType.Shovel:      TryUseShovel(slot);                         break;
            case ToolType.WateringCan: TryUseWateringCan(slot, itemID, toolData);  break;
            case ToolType.Fertilizer:  TryUseFertilizer(slot, itemID);             break;
        }
    }

    private void TryUseShovel(SlotController slot)
    {
        if (slot.State == SlotState.Empty) return;
        slot.RemoveFarmPlot();
    }

    private void TryUseWateringCan(SlotController slot, string itemID, ToolData toolData)
    {
        if (slot.OccupyingPlant == null || !PlayerInventory.Instance.HasItem(itemID)) return;
        if (!slot.OccupyingPlant.AddModifier("Wet")) return;
        slot.OccupyingPlant.GrowthSpeedMultiplier =
            Mathf.Max(slot.OccupyingPlant.GrowthSpeedMultiplier, toolData.GrowthSpeedMultiplier);
        PlayerInventory.Instance.RemoveItem(itemID);
        AudioManager.Instance?.PlaySFX("watering_can");
        if (!PlayerInventory.Instance.HasItem(itemID))
            HotbarManager.Instance?.ClearSlotIfEmpty(itemID);
    }

    private void TryUseFertilizer(SlotController slot, string itemID)
    {
        if (slot.OccupyingPlant == null || slot.OccupyingPlant.IsFertilized) return;
        if (!PlayerInventory.Instance.HasItem(itemID)) return;
        slot.OccupyingPlant.AddModifier("Fertilized");
        PlayerInventory.Instance.RemoveItem(itemID);
        AudioManager.Instance?.PlaySFX("fertilizer");
        if (!PlayerInventory.Instance.HasItem(itemID))
            HotbarManager.Instance?.ClearSlotIfEmpty(itemID);
    }

    private void TryHarvest(SlotController slot)
    {
        if (slot.State != SlotState.FarmPlot_Ready) return;
        Debug.Log($"[GridManager] Harvest stub — slot {slot.SlotIndex}. Full harvest in Phase 2.");
        slot.ClearPlant();
        AudioManager.Instance?.PlaySFX("harvest");
    }
}
