// =============================================================================
// TutorialManager.cs  |  Scripts/Tutorial
// WaifuGarden — Phase 0
// Stub. Full step-by-step implementation added in Phase 10.
//
// Phase 0 responsibilities:
//   - Exposes TutorialEnabled flag (Inspector-toggleable)
//   - Exposes CurrentStep so PlantInstance can gate Strange Plant growth
//   - When TutorialEnabled = false, this script does NOTHING AT ALL
//     and the base game runs identically without it
// =============================================================================

using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    // -------------------------------------------------------------------------
    [Header("Tutorial Control")]
    // -------------------------------------------------------------------------

    [Tooltip("Master on/off for the entire tutorial system.\n\n" +
             "TRUE  → Tutorial runs on new game (normal player experience).\n" +
             "FALSE → Tutorial is fully skipped. Use this during development\n" +
             "         to test non-tutorial gameplay without any code changes.")]
    public bool TutorialEnabled = true;

    /// <summary>Current tutorial progress. Read by PlantInstance and other systems.</summary>
    public TutorialStep CurrentStep { get; private set; } = TutorialStep.Idle;

    // -------------------------------------------------------------------------
    // Unity lifecycle
    // -------------------------------------------------------------------------

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        if (!TutorialEnabled)
        {
            CurrentStep = TutorialStep.Complete;
            Debug.Log("[TutorialManager] Tutorial DISABLED — skipping to Complete.");
            return;
        }
        // Full tutorial boot sequence implemented in Phase 10.
        Debug.Log("[TutorialManager] Tutorial ENABLED — awaiting Phase 10 implementation.");
        CurrentStep = TutorialStep.Idle;
    }

    // -------------------------------------------------------------------------
    // Public API — safe to call from any system; stubs until Phase 10.
    // -------------------------------------------------------------------------

    /// <summary>
    /// Forces the specified world event to activate immediately, bypassing the timer.
    /// Called by TutorialManager at Step 9 (WaitForHeatwave).
    /// Wired to EventManager.ForceEvent() in Phase 10.
    /// </summary>
    public void ForceEvent(string eventID)
    {
        // Phase 10: EventManager.Instance.ForceEvent(eventID);
        Debug.Log($"[TutorialManager] ForceEvent('{eventID}') — stub, wired in Phase 10.");
    }

    /// <summary>
    /// Forces a shop item to appear unlocked and free for the current tutorial step.
    /// Called at Steps 5 (Fertilizer) and 7 (WateringCan).
    /// Wired to ShopManager in Phase 10.
    /// </summary>
    public void ForceUnlockShopItem(string itemID)
    {
        // Phase 10: ShopManager.Instance.SetTutorialOverride(itemID, free: true);
        Debug.Log($"[TutorialManager] ForceUnlockShopItem('{itemID}') — stub, wired in Phase 10.");
    }

    /// <summary>
    /// Returns true if the tutorial is either disabled OR the current step
    /// has reached or passed the given step.
    /// Used by PlantInstance: IsStepReached(WaitForHeatwave) gates Strange Plant growth.
    /// </summary>
    public bool IsStepReached(TutorialStep step)
    {
        if (!TutorialEnabled) return true;
        return CurrentStep >= step;
    }

    // -------------------------------------------------------------------------
    // Internal step control — called only from within TutorialManager in Phase 10.
    // -------------------------------------------------------------------------

    /// <summary>Advances to the next tutorial step. Full logic implemented in Phase 10.</summary>
    private void AdvanceStep()
    {
        if (CurrentStep == TutorialStep.Complete) return;
        CurrentStep++;
        Debug.Log($"[TutorialManager] → Step: {CurrentStep}");
    }
}
