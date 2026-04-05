// =============================================================================
// GameManager.cs  |  Scripts/Core
// WaifuGarden — Phase 1  (replaces Phase 0 version)
// Central coordinator singleton. Holds inspector references to all systems.
// Add new system references each phase — never remove old ones.
// Attach to the GameManager GameObject.
// =============================================================================

using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // -------------------------------------------------------------------------
    [Header("Core")]
    // -------------------------------------------------------------------------
    public DataRegistry     DataRegistry;
    public GameInitializer  GameInitializer;
    public SaveManager      SaveManager;

    // -------------------------------------------------------------------------
    [Header("Player")]
    // -------------------------------------------------------------------------
    public PlayerStats      PlayerStats;
    public PlayerInventory  PlayerInventory;
    public PlayerCollection PlayerCollection;

    // -------------------------------------------------------------------------
    [Header("Grid  (Phase 1)")]
    // -------------------------------------------------------------------------
    public GridManager      GridManager;

    // -------------------------------------------------------------------------
    [Header("Audio / Animation")]
    // -------------------------------------------------------------------------
    public AudioManager     AudioManager;

    // -------------------------------------------------------------------------
    [Header("UI  (Phase 1)")]
    // -------------------------------------------------------------------------
    public HotbarManager    HotbarManager;

    // -------------------------------------------------------------------------
    [Header("Tutorial")]
    // -------------------------------------------------------------------------
    public TutorialManager  TutorialManager;

    // -------------------------------------------------------------------------
    // Systems added in later phases — leave blank for now.
    // Phase 2:  PlantLifecycleSystem
    // Phase 3:  ShopManager
    // Phase 4:  EventManager, ModifierSystem
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
        Debug.Log("[GameManager] Phase 1 startup OK.");
    }

    private void ValidateReferences()
    {
        if (!DataRegistry)    Debug.LogError("[GameManager] DataRegistry missing!");
        if (!PlayerStats)     Debug.LogError("[GameManager] PlayerStats missing!");
        if (!PlayerInventory) Debug.LogError("[GameManager] PlayerInventory missing!");
        if (!PlayerCollection)Debug.LogError("[GameManager] PlayerCollection missing!");
        if (!SaveManager)     Debug.LogError("[GameManager] SaveManager missing!");
        if (!AudioManager)    Debug.LogError("[GameManager] AudioManager missing!");
        if (!GridManager)     Debug.LogError("[GameManager] GridManager missing!");
        if (!HotbarManager)   Debug.LogError("[GameManager] HotbarManager missing!");
        if (!TutorialManager) Debug.LogWarning("[GameManager] TutorialManager missing — tutorial will not run.");
    }
}
