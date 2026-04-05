// =============================================================================
// SlotController.cs  |  Scripts/Grid
// WaifuGarden — Phase 1 Update 5
// Fix: Removed all OccupyingPlant.gameObject.SetActive() calls.
// PlantInstance is a component on the SAME GameObject as SlotController,
// so calling SetActive on it disabled the entire slot. Visibility is now
// controlled entirely through the PlantDisplay Image component instead.
// =============================================================================

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SlotController : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    // -------------------------------------------------------------------------
    [Header("Visual Layers")]
    // -------------------------------------------------------------------------

    [Tooltip("Root Image — the slot background square.")]
    public Image BackgroundImage;

    [Tooltip("Child Image shown when a farm plot is placed.")]
    public Image FarmPlotOverlay;

    [Tooltip("Child Image showing the current plant sprite.")]
    public Image PlantDisplay;

    // -------------------------------------------------------------------------
    [Header("Farm Plot Sprites")]
    // -------------------------------------------------------------------------

    public Sprite FarmPlotDefault;
    public Sprite FarmPlotFertilized;
    public Sprite FarmPlotFertilizedWet;

    // -------------------------------------------------------------------------
    // Runtime state
    // -------------------------------------------------------------------------

    public SlotState     State          { get; private set; } = SlotState.Empty;
    public PlantInstance OccupyingPlant { get; private set; }
    public int           SlotIndex      { get; private set; }

    // -------------------------------------------------------------------------
    // Initialisation
    // -------------------------------------------------------------------------

    public void Initialise(int index)
    {
        SlotIndex = index;

        // Get the PlantInstance component — it lives on this same GameObject.
        // Do NOT call SetActive on its gameObject; that would disable this slot.
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
        // Reset the PlantInstance state without touching gameObject activation.
        if (OccupyingPlant != null)
        {
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
            Debug.LogError("[SlotController] PlantInstance component missing from this slot prefab!");
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
            OccupyingPlant.OnModifiersChanged -= RefreshFarmPlotSprite;

        // No SetActive calls — just update state and hide the display image.
        SetState(SlotState.FarmPlot_Empty);
    }

    // -------------------------------------------------------------------------
    // Pointer events
    // -------------------------------------------------------------------------

    public void OnPointerClick(PointerEventData eventData)
    {
        GridManager.Instance?.HandleSlotClick(this, eventData.button);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Phase 2: show tooltip with remaining grow time
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Phase 2: hide tooltip
    }
}
