// =============================================================================
// PlantInstance.cs  |  Scripts/Grid
// WaifuGarden — Phase 2
// PlantInstance is a PURE DATA CONTAINER. It holds all runtime state for one
// planted crop but contains zero logic. PlantLifecycleSystem drives all growth.
// Updated: StageTimer, GetRemainingTime(), GetStatusText() added.
// =============================================================================

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlantInstance : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Identity & Stage
    // -------------------------------------------------------------------------
    public string      PlantID            { get; private set; }
    public PlantData   Data               { get; private set; }
    public GrowthStage Stage              { get; private set; } = GrowthStage.Seed;
    public bool        EvolutionPending   { get; set; }        = false;
    public bool        IsFertilized       { get; private set; } = false;
    public bool        IsWatered          { get; private set; } = false;

    /// <summary>
    /// Effective growth speed multiplier.
    /// 1.0 base. WateringCan sets to 2.0. BonusManager (Phase 7) stacks on top.
    /// </summary>
    public float GrowthSpeedMultiplier { get; set; } = 1.0f;

    /// <summary>Counts up each frame via PlantLifecycleSystem. Reset to 0 on stage change.</summary>
    public float StageTimer { get; set; } = 0f;

    public List<string> ActiveModifierIDs { get; private set; } = new List<string>();

    // Visual reference — set by SlotController.PlantSeed()
    [HideInInspector] public Image PlantImage;

    // -------------------------------------------------------------------------
    // Initialisation
    // -------------------------------------------------------------------------

    public void Initialise(PlantData data, Image displayImage)
    {
        Data                  = data;
        PlantID               = data.PlantID;
        Stage                 = GrowthStage.Seed;
        StageTimer            = 0f;
        EvolutionPending      = false;
        IsFertilized          = false;
        IsWatered             = false;
        GrowthSpeedMultiplier = 1.0f;
        ActiveModifierIDs.Clear();

        PlantImage = displayImage;
        UpdateSprite();
        Debug.Log($"[PlantInstance] Planted: {PlantID}");
    }

    // -------------------------------------------------------------------------
    // Sprite
    // -------------------------------------------------------------------------

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
    // Stage advancement — called by PlantLifecycleSystem only
    // -------------------------------------------------------------------------

    public void AdvanceStage()
    {
        if (Stage == GrowthStage.Mature) return;
        Stage      = Stage == GrowthStage.Seed ? GrowthStage.Sprout : GrowthStage.Mature;
        StageTimer = 0f;
        UpdateSprite();
        Debug.Log($"[PlantInstance] {PlantID} → {Stage}");
        OnStageChanged?.Invoke(Stage);
    }

    // -------------------------------------------------------------------------
    // Growth time helpers — used by SlotLabelUI and PlantLifecycleSystem
    // -------------------------------------------------------------------------

    /// <summary>
    /// Threshold for the current stage (seconds at base speed × current multiplier).
    /// </summary>
    public float GetCurrentStageThreshold()
    {
        if (Data == null) return float.MaxValue;
        float baseTime = Stage == GrowthStage.Seed
            ? Data.SeedToSproutTime
            : Data.SproutToMatureTime;
        return baseTime / Mathf.Max(GrowthSpeedMultiplier, 0.01f);
    }

    /// <summary>Remaining seconds until the current stage completes. Always >= 0.</summary>
    public float GetRemainingTime()
    {
        if (Stage == GrowthStage.Mature) return 0f;
        return Mathf.Max(0f, GetCurrentStageThreshold() - StageTimer);
    }

    /// <summary>
    /// Short status string shown in the slot label.
    ///   Growing:  "Carrot — 3.2s"
    ///   Ready:    "Carrot — Ready for harvest!"
    ///   Glowing:  "Carrot — Wants to evolve!"
    /// </summary>
    public string GetStatusText()
    {
        string name = Data != null ? Data.PlantName : PlantID;
        if (Stage != GrowthStage.Mature)
            return $"{name} — {GetRemainingTime():F1}s";
        return EvolutionPending
            ? $"{name} — Wants to evolve!"
            : $"{name} — Ready for harvest!";
    }

    // -------------------------------------------------------------------------
    // Modifier API — called by ModifierSystem (Phase 4) and tools (Phase 5)
    // -------------------------------------------------------------------------

    public bool AddModifier(string modifierID)
    {
        if (string.IsNullOrEmpty(modifierID))       return false;
        if (ActiveModifierIDs.Contains(modifierID)) return false;
        ActiveModifierIDs.Add(modifierID);
        if (modifierID == "Fertilized") IsFertilized = true;
        if (modifierID == "Wet")        IsWatered    = true;
        Debug.Log($"[PlantInstance] {PlantID} gained modifier: {modifierID}");
        OnModifiersChanged?.Invoke();
        return true;
    }

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
    // Events
    // -------------------------------------------------------------------------
    public event System.Action              OnModifiersChanged;
    public event System.Action<GrowthStage> OnStageChanged;
}
