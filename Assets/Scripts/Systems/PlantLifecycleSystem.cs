// =============================================================================
// PlantLifecycleSystem.cs  |  Scripts/Systems
// WaifuGarden — Phase 2
// The ONLY system that advances plant growth timers.
// PlantInstance has no Update() — this system drives all growth logic centrally,
// making it trivial to pause all growth (cutscene, tutorial gate, etc.)
//
// Attach to its own root-level GameObject.
// =============================================================================

using System.Collections.Generic;
using UnityEngine;

public class PlantLifecycleSystem : MonoBehaviour
{
    public static PlantLifecycleSystem Instance { get; private set; }

    [Header("Global Growth Control")]
    [Tooltip("Set to false to freeze all plant growth (cutscenes, pausing, etc.).")]
    public bool GrowthEnabled = true;

    // -------------------------------------------------------------------------
    // Active plant tracking — populated via GridManager events
    // -------------------------------------------------------------------------
    private readonly List<SlotController> _activeSlots = new List<SlotController>();

    // -------------------------------------------------------------------------

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        if (GridManager.Instance == null)
        {
            Debug.LogError("[PlantLifecycleSystem] GridManager not found!");
            return;
        }
        GridManager.Instance.OnPlantPlanted += OnPlantPlanted;
        GridManager.Instance.OnPlantRemoved += OnPlantRemoved;
        Debug.Log("[PlantLifecycleSystem] Subscribed to GridManager events.");
    }

    private void OnDestroy()
    {
        if (GridManager.Instance != null)
        {
            GridManager.Instance.OnPlantPlanted -= OnPlantPlanted;
            GridManager.Instance.OnPlantRemoved -= OnPlantRemoved;
        }
    }

    // -------------------------------------------------------------------------
    // Slot tracking
    // -------------------------------------------------------------------------

    private void OnPlantPlanted(SlotController slot)
    {
        if (!_activeSlots.Contains(slot))
            _activeSlots.Add(slot);
    }

    private void OnPlantRemoved(SlotController slot)
    {
        _activeSlots.Remove(slot);
    }

    // -------------------------------------------------------------------------
    // Growth tick — runs every frame, advances all active plant timers
    // -------------------------------------------------------------------------

    private void Update()
    {
        if (!GrowthEnabled) return;

        for (int i = _activeSlots.Count - 1; i >= 0; i--)
        {
            SlotController slot = _activeSlots[i];
            if (slot == null) { _activeSlots.RemoveAt(i); continue; }

            PlantInstance plant = slot.OccupyingPlant;
            if (plant == null || plant.Stage == GrowthStage.Mature)
                continue;

            // ---------------------------------------------------------------
            // Strange Plant tutorial gate:
            // If TutorialPlantMode is on, freeze the timer until the tutorial
            // step is reached AND all three required modifiers are present.
            // When TutorialEnabled = false, this gate is skipped entirely.
            // ---------------------------------------------------------------
            if (plant.Data.TutorialPlantMode
                && TutorialManager.Instance != null
                && TutorialManager.Instance.TutorialEnabled)
            {
                bool conditionsMet =
                    TutorialManager.Instance.IsStepReached(TutorialStep.WaitForHeatwave)
                    && plant.HasModifier("Fertilized")
                    && plant.HasModifier("Wet")
                    && plant.HasModifier("Sunkissed");

                if (!conditionsMet) continue;
            }

            // Advance timer
            plant.StageTimer += Time.deltaTime;

            // Check stage transition
            float threshold = plant.GetCurrentStageThreshold();
            if (plant.StageTimer >= threshold)
                TransitionStage(slot, plant);
        }
    }

    // -------------------------------------------------------------------------
    // Stage transition
    // -------------------------------------------------------------------------

    private void TransitionStage(SlotController slot, PlantInstance plant)
    {
        if (plant.Stage == GrowthStage.Seed)
        {
            // Seed → Sprout
            plant.AdvanceStage();
            slot.SetState(SlotState.FarmPlot_Growing);
            AudioManager.Instance?.PlaySFX("stage_transition");
            AnimationHelper.PlayGrowthPop(plant.PlantImage?.GetComponent<RectTransform>());
            return;
        }

        if (plant.Stage == GrowthStage.Sprout)
        {
            // Sprout → Mature
            plant.AdvanceStage();
            AudioManager.Instance?.PlaySFX("stage_transition");
            AnimationHelper.PlayGrowthPop(plant.PlantImage?.GetComponent<RectTransform>());

            // ---------------------------------------------------------------
            // Evolution check.
            // Phase 6: replace stub with EvolutionSystem.Instance.CheckEvolution(slot)
            // ---------------------------------------------------------------
            bool evolutionPending = false; // Phase 6 stub — always false for now

            if (evolutionPending)
            {
                plant.EvolutionPending = true;
                slot.SetState(SlotState.FarmPlot_Glowing);
                AnimationHelper.PlayGlowPulse(plant.PlantImage, new Color(1f, 0.9f, 0.2f, 1f));
                AudioManager.Instance?.PlaySFX("stage_transition");
            }
            else
            {
                // Check if player has already collected this character (duplicate evolution)
                slot.SetState(SlotState.FarmPlot_Ready);
            }
        }
    }
}
