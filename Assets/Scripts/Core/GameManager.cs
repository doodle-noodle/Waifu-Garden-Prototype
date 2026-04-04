// =============================================================================
// GameManager.cs  |  Scripts/Core
// WaifuGarden — Phase 0
// Central coordinator singleton. Holds references to all major systems.
// Systems never reference each other directly — they go through GameManager
// or communicate via C# events.
// Attach to a "GameManager" GameObject in the scene root.
// =============================================================================

using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // -------------------------------------------------------------------------
    [Header("Player")]
    // -------------------------------------------------------------------------
    public PlayerStats      PlayerStats;
    public PlayerInventory  PlayerInventory;
    public PlayerCollection PlayerCollection;

    // -------------------------------------------------------------------------
    [Header("Core")]
    // -------------------------------------------------------------------------
    public SaveManager      SaveManager;
    public AudioManager     AudioManager;

    // -------------------------------------------------------------------------
    [Header("Tutorial")]
    // -------------------------------------------------------------------------
    public TutorialManager  TutorialManager;

    // -------------------------------------------------------------------------
    // Systems added in later phases — leave blank for now.
    // Phase 1:  GridManager, HotbarManager
    // Phase 2:  PlantLifecycleSystem
    // Phase 3:  ShopManager
    // Phase 4:  EventManager, ModifierSystem
    // Phase 5:  (tools wired into existing systems)
    // Phase 6:  EvolutionSystem
    // Phase 7:  BonusManager, GreenhouseManager
    // Phase 8:  RelationshipManager, DialogueManager
    // Phase 9:  CatalogueManager
    // -------------------------------------------------------------------------

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        ValidateReferences();
        Debug.Log("[GameManager] Phase 0 startup OK.");
    }

    private void ValidateReferences()
    {
        if (!PlayerStats)      Debug.LogError("[GameManager] PlayerStats missing!");
        if (!PlayerInventory)  Debug.LogError("[GameManager] PlayerInventory missing!");
        if (!PlayerCollection) Debug.LogError("[GameManager] PlayerCollection missing!");
        if (!SaveManager)      Debug.LogError("[GameManager] SaveManager missing!");
        if (!AudioManager)     Debug.LogError("[GameManager] AudioManager missing!");
        if (!TutorialManager)  Debug.LogWarning("[GameManager] TutorialManager missing — tutorial will not run.");
    }
}
