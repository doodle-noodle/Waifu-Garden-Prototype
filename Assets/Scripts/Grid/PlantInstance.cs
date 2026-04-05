// =============================================================================
// PlantInstance.cs  |  Scripts/Grid
// WaifuGarden — Phase 1
// Tracks all runtime state for one planted crop on a grid slot.
// Attached as a component to the SlotPrefab. Disabled when the slot has no plant.
//
// Phase 1:  Identity + Seed stage sprite display.
// Phase 2:  Growth timer, stage transitions, hover tooltip.
// Phase 4:  ActiveModifierIDs populated by ModifierSystem.
// Phase 6:  EvolutionPending flag set by EvolutionSystem.
// =============================================================================

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlantInstance : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // State — populated by SlotController.PlantSeed()
    // -------------------------------------------------------------------------

    public string    PlantID             { get; private set; }
    public PlantData Data                { get; private set; }
    public GrowthStage Stage             { get; private set; } = GrowthStage.Seed;
    public bool      EvolutionPending    { get; set; }         = false;
    public bool      IsFertilized        { get; private set; } = false;
    public bool      IsWatered           { get; private set; } = false;

    /// <summary>
    /// Combined growth speed multiplier.
    /// 1.0 base → WateringCan sets to 2.0 → BonusManager adds GrowthSpeed bonuses on top.
    /// </summary>
    public float GrowthSpeedMultiplier { get; set; } = 1.0f;

    /// <summary>All modifier IDs currently active on this plant.</summary>
    public List<string> ActiveModifierIDs { get; private set; } = new List<string>();

    // -------------------------------------------------------------------------
    // Visual reference — set by SlotController during Initialise()
    // -------------------------------------------------------------------------

    [HideInInspector] public Image PlantImage; // The Image component that shows plant sprites

    // -------------------------------------------------------------------------
    // Phase 2 growth timer fields (declared now so Phase 2 can simply uncomment)
    // -------------------------------------------------------------------------

    [HideInInspector] public float StageTimer = 0f;

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    /// <summary>
    /// Called by SlotController when a seed is planted.
    /// Initialises identity, sets Seed stage, displays seed sprite.
    /// </summary>
    public void Initialise(PlantData data, Image displayImage)
    {
        Data               = data;
        PlantID            = data.PlantID;
        Stage              = GrowthStage.Seed;
        StageTimer         = 0f;
        EvolutionPending   = false;
        IsFertilized       = false;
        IsWatered          = false;
        GrowthSpeedMultiplier = 1.0f;
        ActiveModifierIDs.Clear();

        PlantImage = displayImage;
        UpdateSprite();

        gameObject.SetActive(true);
        Debug.Log($"[PlantInstance] Planted: {PlantID}");
    }

    /// <summary>Sets the plant image sprite to match the current growth stage.</summary>
    public void UpdateSprite()
    {
        if (PlantImage == null || Data == null) return;

        Sprite sprite = Stage switch
        {
            GrowthStage.Seed   => Data.SeedSprite,
            GrowthStage.Sprout => Data.SproutSprite,
            GrowthStage.Mature => Data.MatureSprite,
            _                  => Data.SeedSprite
        };

        PlantImage.sprite  = sprite;
        PlantImage.enabled = sprite != null;
    }

    // -------------------------------------------------------------------------
    // Modifier API — called by ModifierSystem (Phase 4) and tools (Phase 5)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Adds a modifier to this plant if not already present.
    /// Returns true if the modifier was newly added.
    /// </summary>
    public bool AddModifier(string modifierID)
    {
        if (string.IsNullOrEmpty(modifierID))         return false;
        if (ActiveModifierIDs.Contains(modifierID))   return false;

        ActiveModifierIDs.Add(modifierID);

        // Cache convenience flags
        if (modifierID == "Fertilized") IsFertilized = true;
        if (modifierID == "Wet")        IsWatered    = true;

        Debug.Log($"[PlantInstance] {PlantID} gained modifier: {modifierID}");
        OnModifiersChanged?.Invoke();
        return true;
    }

    /// <summary>Removes a modifier by ID. Used by combination rules (Phase 4).</summary>
    public bool RemoveModifier(string modifierID)
    {
        bool removed = ActiveModifierIDs.Remove(modifierID);
        if (removed)
        {
            if (modifierID == "Fertilized") IsFertilized = false;
            if (modifierID == "Wet")        IsWatered    = false;
            OnModifiersChanged?.Invoke();
        }
        return removed;
    }

    public bool HasModifier(string modifierID) => ActiveModifierIDs.Contains(modifierID);

    // -------------------------------------------------------------------------
    // Phase 2 — Growth stage transition (implemented in Phase 2)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Advances to the next growth stage and updates the sprite.
    /// Called by PlantLifecycleSystem (Phase 2).
    /// </summary>
    public void AdvanceStage()
    {
        if (Stage == GrowthStage.Mature) return;
        Stage = Stage == GrowthStage.Seed ? GrowthStage.Sprout : GrowthStage.Mature;
        StageTimer = 0f;
        UpdateSprite();
        // Phase 2: AnimationHelper.PlayGrowthPop(PlantImage) — wired there
        Debug.Log($"[PlantInstance] {PlantID} → {Stage}");
        OnStageChanged?.Invoke(Stage);
    }

    // -------------------------------------------------------------------------
    // Events — subscribed to by SlotController and UI systems
    // -------------------------------------------------------------------------

    public event System.Action             OnModifiersChanged;
    public event System.Action<GrowthStage> OnStageChanged;
}
