// =============================================================================
// GridManager.cs  |  Scripts/Grid
// WaifuGarden — Phase 2
// Updated: TryHarvest now creates CropData with proper sell value calculation.
// Sell value = BaseHarvestValue × each modifier multiplier × character bonus (stub).
// =============================================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [Header("Grid Container")]
    public RectTransform GridContainer;

    [Header("Starting Farm Plots")]
    public List<int> StartingFarmPlotIndices = new List<int> { 5, 6, 9, 10 };

    // -------------------------------------------------------------------------
    public event Action<SlotController> OnPlantPlanted;
    public event Action<SlotController> OnPlantRemoved;
    // -------------------------------------------------------------------------

    private SlotController[] _slots;

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
        if (found.Length == 0)    { Debug.LogError("[GridManager] No SlotControllers found."); return; }
        _slots = found;
        for (int i = 0; i < _slots.Length; i++) _slots[i].Initialise(i);
        Debug.Log($"[GridManager] Initialised {_slots.Length} pre-built slots.");
    }

    private void PlaceStartingFarmPlots()
    {
        if (_slots == null) return;
        foreach (int idx in StartingFarmPlotIndices)
        {
            if (idx < 0 || idx >= _slots.Length) { Debug.LogWarning($"[GridManager] Farm plot index {idx} out of range."); continue; }
            _slots[idx].PlaceFarmPlot();
        }
        Debug.Log($"[GridManager] Placed {StartingFarmPlotIndices.Count} starting farm plots.");
    }

    // -------------------------------------------------------------------------
    // Accessors
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
        if (plantData == null) { Debug.LogWarning($"[GridManager] No PlantData for '{seedData.LinkedPlantID}'."); return; }
        if (!PlayerInventory.Instance.HasItem(seedData.ItemID)) return;

        slot.PlantSeed(plantData);
        PlayerInventory.Instance.RemoveItem(seedData.ItemID);
        if (!PlayerInventory.Instance.HasItem(seedData.ItemID))
            HotbarManager.Instance?.ClearSlotIfEmpty(seedData.ItemID);

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

    /// <summary>
    /// Harvests a mature (non-glowing) plant. Calculates final sell value,
    /// creates CropData, adds to PlayerInventory.
    /// </summary>
    private void TryHarvest(SlotController slot)
    {
        if (slot.State != SlotState.FarmPlot_Ready) return;

        PlantInstance plant = slot.OccupyingPlant;
        if (plant == null) return;

        // -----------------------------------------------------------------------
        // Sell value calculation (GDD Section 5.6):
        //   FinalValue = BaseHarvestValue
        //              × each modifier's SellValueMultiplier (sequential)
        //              × SellValue character bonus (Phase 7 — stub = 1.0)
        // -----------------------------------------------------------------------
        float value = plant.Data != null ? plant.Data.BaseHarvestValue : 0f;

        foreach (string modID in plant.ActiveModifierIDs)
        {
            ModifierData mod = DataRegistry.Instance?.GetModifier(modID);
            if (mod != null) value *= mod.SellValueMultiplier;
        }

        // Phase 7: value *= BonusManager.Instance.GetSellValueMultiplier();
        float characterBonus = 1.0f; // placeholder until Phase 7
        value *= characterBonus;

        // Create crop record
        string plantName = plant.Data != null ? plant.Data.PlantName : plant.PlantID;
        CropData crop = new CropData(plant.PlantID, plantName, value,
                                     new System.Collections.Generic.List<string>(plant.ActiveModifierIDs));

        // Harvest animation then clear
        AnimationHelper.PlayHarvestPop(plant.PlantImage, () =>
        {
            slot.ClearPlant();
            OnPlantRemoved?.Invoke(slot);
        });

        // Discover plant and add crop to inventory
        PlayerCollection.Instance?.DiscoverPlant(plant.PlantID);
        PlayerInventory.Instance?.AddCrop(crop);
        AudioManager.Instance?.PlaySFX("harvest");

        Debug.Log($"[GridManager] Harvested {plantName} for ¥{value:F0}.");
    }

    // -------------------------------------------------------------------------
    // Called by EvolutionConfirmDialogue (Phase 6 wires this fully)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Player confirmed evolution. Phase 6 will implement the full evolution sequence.
    /// For now, just clears the glowing plant and fires OnPlantRemoved.
    /// </summary>
    public void ConfirmEvolution(SlotController slot)
    {
        // Phase 6: EvolutionSystem.Instance.ExecuteEvolution(slot);
        Debug.Log($"[GridManager] Evolution confirmed for slot {slot.SlotIndex} — stub until Phase 6.");
        AnimationHelper.StopGlowPulse(slot.PlantDisplay);
        slot.ClearPlant();
        OnPlantRemoved?.Invoke(slot);
    }
}
