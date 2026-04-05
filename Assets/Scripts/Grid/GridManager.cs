// =============================================================================
// GridManager.cs  |  Scripts/Grid
// WaifuGarden — Pre-Phase 2 Fixes
// Added: OnPlantPlanted event fired whenever a seed is successfully planted.
// ModifierSystem (Phase 4) and EvolutionSystem (Phase 6) subscribe to this
// so they are notified of new PlantInstances without polling.
// =============================================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [Header("Grid Container")]
    [Tooltip("RectTransform containing all pre-built SlotController children.")]
    public RectTransform GridContainer;

    [Header("Starting Farm Plots")]
    public List<int> StartingFarmPlotIndices = new List<int> { 5, 6, 9, 10 };

    // -------------------------------------------------------------------------
    // Events
    // -------------------------------------------------------------------------

    /// <summary>
    /// Fired whenever a seed is successfully planted in a slot.
    /// Passes the SlotController that now contains the new PlantInstance.
    /// Subscribers: ModifierSystem (Phase 4), EvolutionSystem (Phase 6),
    ///              PlantLifecycleSystem (Phase 2).
    /// </summary>
    public event Action<SlotController> OnPlantPlanted;

    /// <summary>
    /// Fired whenever a plant is removed from a slot (harvest, shovel, evolution).
    /// Passes the SlotController that was cleared.
    /// </summary>
    public event Action<SlotController> OnPlantRemoved;

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

    private void InitialiseSlots()
    {
        if (GridContainer == null) { Debug.LogError("[GridManager] GridContainer not assigned!"); return; }

        SlotController[] found = GridContainer.GetComponentsInChildren<SlotController>(true);
        if (found.Length == 0) { Debug.LogError("[GridManager] No SlotControllers found in GridContainer."); return; }

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

    public SlotController              GetSlot(int index) =>
        (_slots != null && index >= 0 && index < _slots.Length) ? _slots[index] : null;

    public IEnumerable<SlotController> GetAllSlots()      => _slots;

    // -------------------------------------------------------------------------
    // Click routing
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
            case ShopItemType.Seed:     TryPlantSeed(slot, itemData);                        break;
            case ShopItemType.FarmPlot: TryPlaceFarmPlot(slot, activeItem);                  break;
            case ShopItemType.Tool:     TryUseTool(slot, activeItem, itemData as ToolData);  break;
        }
    }

    // -------------------------------------------------------------------------
    // Actions
    // -------------------------------------------------------------------------

    private void TryPlantSeed(SlotController slot, ShopItemData seedData)
    {
        if (slot.State != SlotState.FarmPlot_Empty) return;

        PlantData plantData = DataRegistry.Instance?.GetPlant(seedData.LinkedPlantID);
        if (plantData == null)
        { Debug.LogWarning($"[GridManager] No PlantData for '{seedData.LinkedPlantID}'."); return; }

        if (!PlayerInventory.Instance.HasItem(seedData.ItemID)) return;

        slot.PlantSeed(plantData);
        PlayerInventory.Instance.RemoveItem(seedData.ItemID);

        if (!PlayerInventory.Instance.HasItem(seedData.ItemID))
            HotbarManager.Instance?.ClearSlotIfEmpty(seedData.ItemID);

        // Notify all subscribers that a new plant exists in this slot.
        OnPlantPlanted?.Invoke(slot);
    }

    private void TryPlaceFarmPlot(SlotController slot, string itemID)
    {
        if (slot.State != SlotState.Empty || !PlayerInventory.Instance.HasItem(itemID)) return;
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
        bool hadPlant = slot.OccupyingPlant != null;
        slot.RemoveFarmPlot();
        if (hadPlant) OnPlantRemoved?.Invoke(slot);
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
        // Phase 2: full harvest with CropData creation and sell value calculation.
        Debug.Log($"[GridManager] Harvest stub — slot {slot.SlotIndex}. Full implementation in Phase 2.");
        slot.ClearPlant();
        OnPlantRemoved?.Invoke(slot);
        AudioManager.Instance?.PlaySFX("harvest");
    }
}
