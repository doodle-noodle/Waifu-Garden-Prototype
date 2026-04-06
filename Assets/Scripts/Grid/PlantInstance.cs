// =============================================================================
// PlantInstance.cs  |  Scripts/Grid
// WaifuGarden — Phase 2 Fix 2
// Fix A: GetRemainingTime() now returns TOTAL remaining time until Mature,
//        not just time until the next stage.
//        Seed stage:   (SeedThreshold - StageTimer) + full SproutThreshold
//        Sprout stage: (SproutThreshold - StageTimer)
// Fix B: GetStatusText() uses the corrected time.
// =============================================================================

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlantInstance : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Identity & Stage
    // -------------------------------------------------------------------------
    public string      PlantID          { get; private set; }
    public PlantData   Data             { get; private set; }
    public GrowthStage Stage            { get; private set; } = GrowthStage.Seed;
    public bool        EvolutionPending { get; set; }         = false;
    public bool        IsFertilized     { get; private set; } = false;
    public bool        IsWatered        { get; private set; } = false;
    public float       GrowthSpeedMultiplier { get; set; }    = 1.0f;

    /// <summary>Counts up via PlantLifecycleSystem. Reset to 0 on stage change.</summary>
    public float StageTimer { get; set; } = 0f;

    public List<string> ActiveModifierIDs { get; private set; } = new List<string>();

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
    // Growth time helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Duration of the Seed stage at current speed (SeedToSproutTime / multiplier).
    /// </summary>
    public float GetSeedThreshold() =>
        Data == null ? 0f : Data.SeedToSproutTime / Mathf.Max(GrowthSpeedMultiplier, 0.01f);

    /// <summary>
    /// Duration of the Sprout stage at current speed (SproutToMatureTime / multiplier).
    /// </summary>
    public float GetSproutThreshold() =>
        Data == null ? 0f : Data.SproutToMatureTime / Mathf.Max(GrowthSpeedMultiplier, 0.01f);

    /// <summary>
    /// Threshold for the current stage only — used by PlantLifecycleSystem to
    /// know when to trigger a transition.
    /// </summary>
    public float GetCurrentStageThreshold() =>
        Stage == GrowthStage.Seed ? GetSeedThreshold() : GetSproutThreshold();

    /// <summary>
    /// TOTAL remaining seconds until Mature, regardless of current stage.
    ///   Seed stage:   (SeedThreshold - StageTimer) + full SproutThreshold
    ///   Sprout stage: (SproutThreshold - StageTimer)
    ///   Mature:       0
    /// This is what is shown in the slot label.
    /// </summary>
    public float GetTotalRemainingTime()
    {
        if (Stage == GrowthStage.Mature) return 0f;

        if (Stage == GrowthStage.Seed)
        {
            float remainingInSeed   = Mathf.Max(0f, GetSeedThreshold()   - StageTimer);
            float fullSproutTime    = GetSproutThreshold();
            return remainingInSeed + fullSproutTime;
        }

        // Sprout stage
        return Mathf.Max(0f, GetSproutThreshold() - StageTimer);
    }

    /// <summary>
    /// Status string shown in the slot label.
    ///   Growing:  "Carrot — 5.0s"   (total time to Mature)
    ///   Ready:    "Carrot — Ready for harvest!"
    ///   Glowing:  "Carrot — Wants to evolve!"
    /// </summary>
    public string GetStatusText()
    {
        string name = Data != null ? Data.PlantName : PlantID;
        if (Stage != GrowthStage.Mature)
            return $"{name} — {GetTotalRemainingTime():F1}s";
        return EvolutionPending
            ? $"{name} — Wants to evolve!"
            : $"{name} — Ready for harvest!";
    }

    // -------------------------------------------------------------------------
    // Modifier API
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
