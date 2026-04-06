// =============================================================================
// GameManager.cs  |  Scripts/Core
// WaifuGarden — Phase 3
// Added: UnlockNotification reference.
// =============================================================================

using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Core")]
    public DataRegistry    DataRegistry;
    public GameInitializer GameInitializer;
    public SaveManager     SaveManager;

    [Header("Player")]
    public PlayerStats      PlayerStats;
    public PlayerInventory  PlayerInventory;
    public PlayerCollection PlayerCollection;

    [Header("Grid  (Phase 1)")]
    public GridManager      GridManager;

    [Header("Systems  (Phase 2)")]
    public PlantLifecycleSystem PlantLifecycleSystem;

    [Header("Audio / Animation")]
    public AudioManager     AudioManager;

    [Header("UI  (Phase 1)")]
    public HotbarManager    HotbarManager;

    [Header("UI  (Phase 2)")]
    public ShopPanel        ShopPanel;

    [Header("UI  (Phase 3)")]
    public UnlockNotification UnlockNotification;

    [Header("Tutorial")]
    public TutorialManager  TutorialManager;

    // Phases 4–9 system references added each phase.

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        ValidateReferences();
        Debug.Log("[GameManager] Phase 3 startup OK.");
    }

    private void ValidateReferences()
    {
        if (!DataRegistry)         Debug.LogError("[GameManager] DataRegistry missing!");
        if (!PlayerStats)          Debug.LogError("[GameManager] PlayerStats missing!");
        if (!PlayerInventory)      Debug.LogError("[GameManager] PlayerInventory missing!");
        if (!PlayerCollection)     Debug.LogError("[GameManager] PlayerCollection missing!");
        if (!SaveManager)          Debug.LogError("[GameManager] SaveManager missing!");
        if (!AudioManager)         Debug.LogError("[GameManager] AudioManager missing!");
        if (!GridManager)          Debug.LogError("[GameManager] GridManager missing!");
        if (!HotbarManager)        Debug.LogError("[GameManager] HotbarManager missing!");
        if (!PlantLifecycleSystem) Debug.LogError("[GameManager] PlantLifecycleSystem missing!");
        if (!ShopPanel)            Debug.LogWarning("[GameManager] ShopPanel not assigned.");
        if (!UnlockNotification)   Debug.LogWarning("[GameManager] UnlockNotification not assigned.");
        if (!TutorialManager)      Debug.LogWarning("[GameManager] TutorialManager not assigned.");
    }
}
