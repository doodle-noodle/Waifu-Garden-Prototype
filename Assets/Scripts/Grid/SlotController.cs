// =============================================================================
// SlotController.cs  |  Scripts/Grid
// WaifuGarden — Phase 2
// Updated: OnPointerEnter/Exit now show/hide SlotLabelUI.
// Glowing slot clicks open EvolutionConfirmDialogue instead of harvesting.
// =============================================================================

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SlotController : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    // -------------------------------------------------------------------------
    [Header("Visual Layers")]
    // -------------------------------------------------------------------------
    public Image BackgroundImage;
    public Image FarmPlotOverlay;
    public Image PlantDisplay;

    [Header("Farm Plot Sprites")]
    public Sprite FarmPlotDefault;
    public Sprite FarmPlotFertilized;
    public Sprite FarmPlotFertilizedWet;

    // -------------------------------------------------------------------------
    public SlotState     State          { get; private set; } = SlotState.Empty;
    public PlantInstance OccupyingPlant { get; private set; }
    public int           SlotIndex      { get; private set; }

    // -------------------------------------------------------------------------

    public void Initialise(int index)
    {
        SlotIndex      = index;
        OccupyingPlant = GetComponent<PlantInstance>();
        SetState(SlotState.Empty);
    }

    // -------------------------------------------------------------------------
    // State
    // -------------------------------------------------------------------------

    public void SetState(SlotState newState)
    {
        State = newState;
        RefreshVisuals();
    }

    private void RefreshVisuals()
    {
        bool hasFarmPlot = State != SlotState.Empty;
        bool hasPlant    = State == SlotState.FarmPlot_Growing
                        || State == SlotState.FarmPlot_Ready
                        || State == SlotState.FarmPlot_Glowing;

        if (FarmPlotOverlay != null) FarmPlotOverlay.enabled = hasFarmPlot;
        if (PlantDisplay    != null) PlantDisplay.enabled    = hasPlant;
        RefreshFarmPlotSprite();
    }

    private void RefreshFarmPlotSprite()
    {
        if (FarmPlotOverlay == null) return;
        if (OccupyingPlant != null && OccupyingPlant.IsFertilized && OccupyingPlant.IsWatered
            && FarmPlotFertilizedWet != null)
            FarmPlotOverlay.sprite = FarmPlotFertilizedWet;
        else if (OccupyingPlant != null && OccupyingPlant.IsFertilized
            && FarmPlotFertilized != null)
            FarmPlotOverlay.sprite = FarmPlotFertilized;
        else if (FarmPlotDefault != null)
            FarmPlotOverlay.sprite = FarmPlotDefault;
    }

    // -------------------------------------------------------------------------
    // Farm plot actions
    // -------------------------------------------------------------------------

    public void PlaceFarmPlot()
    {
        if (State != SlotState.Empty) return;
        SetState(SlotState.FarmPlot_Empty);
        AudioManager.Instance?.PlaySFX("farmplot_placed");
    }

    public void RemoveFarmPlot()
    {
        if (OccupyingPlant != null)
        {
            AnimationHelper.StopGlowPulse(PlantDisplay);
            OccupyingPlant.OnModifiersChanged -= RefreshFarmPlotSprite;
        }
        SetState(SlotState.Empty);
        AudioManager.Instance?.PlaySFX("shovel");
    }

    // -------------------------------------------------------------------------
    // Planting
    // -------------------------------------------------------------------------

    public void PlantSeed(PlantData plantData)
    {
        if (State != SlotState.FarmPlot_Empty || plantData == null) return;

        if (OccupyingPlant == null)
        {
            Debug.LogError("[SlotController] PlantInstance component missing!");
            return;
        }

        OccupyingPlant.Initialise(plantData, PlantDisplay);
        OccupyingPlant.OnModifiersChanged += RefreshFarmPlotSprite;

        SetState(SlotState.FarmPlot_Growing);
        AudioManager.Instance?.PlaySFX("seed_planted");
        Debug.Log($"[SlotController] Slot {SlotIndex}: planted {plantData.PlantID}");
    }

    public void ClearPlant()
    {
        if (OccupyingPlant != null)
        {
            AnimationHelper.StopGlowPulse(PlantDisplay);
            OccupyingPlant.OnModifiersChanged -= RefreshFarmPlotSprite;
        }
        SetState(SlotState.FarmPlot_Empty);
    }

    // -------------------------------------------------------------------------
    // Pointer events
    // -------------------------------------------------------------------------

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right) return;

        // Glowing slot: open evolution confirmation dialogue instead of harvesting.
        if (State == SlotState.FarmPlot_Glowing && OccupyingPlant != null
            && HotbarManager.Instance?.GetActiveItemID() == null)
        {
            EvolutionConfirmDialogue.Instance?.Show(this, OccupyingPlant.Data?.PlantName ?? "Plant");
            return;
        }

        GridManager.Instance?.HandleSlotClick(this, eventData.button);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (OccupyingPlant == null) return;
        SlotLabelUI.Instance?.Show(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SlotLabelUI.Instance?.Hide();
    }
}
