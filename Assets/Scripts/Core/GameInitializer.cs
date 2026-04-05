// =============================================================================
// GameInitializer.cs  |  Scripts/Core
// WaifuGarden — Phase 1
// Runs once on game start to set up the starting game state:
//   - Gives the player their starting inventory items
//   - Tells GridManager to place the starting 4 farm plots
//   - Kicks off the tutorial or starts normal gameplay
//
// Attach to the GameManager GameObject.
// All starting values are Inspector-configurable — no magic numbers in code.
// =============================================================================

using System.Collections.Generic;
using UnityEngine;

public class GameInitializer : MonoBehaviour
{
    public static GameInitializer Instance { get; private set; }

    // -------------------------------------------------------------------------
    [Header("Starting Inventory")]
    [Tooltip("Item IDs and quantities given to the player at game start.")]
    public List<StartingItem> StartingItems = new List<StartingItem>();

    [System.Serializable]
    public class StartingItem
    {
        [Tooltip("Must match an ItemID in DataRegistry (ShopItemData or ToolData).")]
        public string ItemID;
        public int    Quantity = 1;
    }

    // -------------------------------------------------------------------------
    [Header("Starting Currency")]
    [Tooltip("Current spendable currency at game start. Also set on PlayerStats directly.")]
    public float StartingCurrency = 20f;

    // -------------------------------------------------------------------------

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        // DataRegistry and PlayerStats must be initialised before this runs.
        // Execution order: DataRegistry.Awake → PlayerStats.Awake → GameInitializer.Start
        InitialisePlayerStats();
        InitialiseInventory();
        // GridManager.PlaceStartingFarmPlots() is called from GridManager.Start()
        // so we don't need to call it here.
    }

    private void InitialisePlayerStats()
    {
        if (PlayerStats.Instance == null) { Debug.LogError("[GameInitializer] PlayerStats missing!"); return; }
        // Starting currency is set directly on the Inspector field of PlayerStats.
        // We only log here for confirmation.
        Debug.Log($"[GameInitializer] Starting currency: {PlayerStats.Instance.CurrentCurrency}");
    }

    private void InitialiseInventory()
    {
        if (PlayerInventory.Instance == null) { Debug.LogError("[GameInitializer] PlayerInventory missing!"); return; }

        foreach (StartingItem item in StartingItems)
        {
            if (string.IsNullOrEmpty(item.ItemID)) continue;
            PlayerInventory.Instance.AddItem(item.ItemID, item.Quantity);
            Debug.Log($"[GameInitializer] Starting item: {item.Quantity}x {item.ItemID}");
        }
    }
}
