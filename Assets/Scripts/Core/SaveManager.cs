// =============================================================================
// SaveManager.cs  |  Scripts/Core
// WaifuGarden — Phase 0
// Persistence stub. Save() and Load() are intentionally empty.
// All other systems only write to in-memory objects.
// In v1.0: replace method bodies with Easy Save 3 calls. Nothing else changes.
// Attach to the GameManager GameObject.
// =============================================================================

using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Persist all game state to disk.
    /// STUB — replace with Easy Save 3 in v1.0.
    /// </summary>
    public void Save()
    {
        // TODO (v1.0):
        // ES3.Save("CurrentCurrency",    PlayerStats.Instance.CurrentCurrency);
        // ES3.Save("TotalEarned",        PlayerStats.Instance.TotalCurrencyEarned);
        // ES3.Save("FarmPlotsBought",    PlayerStats.Instance.FarmPlotsPurchased);
        // ES3.Save("DiscoveredChars",    PlayerCollection.Instance.SerialiseCharacters());
        // ES3.Save("RelationshipPts",    PlayerCollection.Instance.SerialiseRelationships());
        // ES3.Save("InventoryItems",     PlayerInventory.Instance.SerialiseItems());
        Debug.Log("[SaveManager] Save() called — stub, no data written.");
    }

    /// <summary>
    /// Load persisted game state from disk.
    /// STUB — replace with Easy Save 3 in v1.0.
    /// </summary>
    public void Load()
    {
        // TODO (v1.0):
        // if (ES3.KeyExists("CurrentCurrency"))
        //     PlayerStats.Instance.CurrentCurrency = ES3.Load<float>("CurrentCurrency");
        Debug.Log("[SaveManager] Load() called — stub, no data loaded.");
    }

    /// <summary>Delete all saved data. STUB.</summary>
    public void DeleteSave()
    {
        // TODO (v1.0): ES3.DeleteFile();
        Debug.Log("[SaveManager] DeleteSave() called — stub.");
    }
}
