// =============================================================================
// PlayerStats.cs  |  Scripts/Player
// WaifuGarden — Phase 0
// Runtime container for all numeric player stats (currency, farm plots bought).
// All values are in-memory; SaveManager will persist them in v1.0.
// Attach to the GameManager GameObject.
// =============================================================================

using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance { get; private set; }

    // -------------------------------------------------------------------------
    [Header("Currency")]
    // -------------------------------------------------------------------------
    [Tooltip("Spendable balance. Decreases on purchases.")]
    public float CurrentCurrency = 20f;

    [Tooltip("Monotonically increasing lifetime total. Used for shop unlock thresholds. Never decreases.")]
    public float TotalCurrencyEarned = 0f;

    // -------------------------------------------------------------------------
    [Header("Farm Plots")]
    // -------------------------------------------------------------------------
    [Tooltip("Number of extra farm plots bought from the shop (not counting the starting 4). " +
             "Used to compute next plot price: 10 × (2 ^ FarmPlotsPurchased).")]
    public int FarmPlotsPurchased = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    /// <summary>Adds amount to CurrentCurrency AND TotalCurrencyEarned.</summary>
    public void EarnCurrency(float amount)
    {
        if (amount <= 0f) return;
        CurrentCurrency      += amount;
        TotalCurrencyEarned  += amount;
        OnCurrencyChanged?.Invoke();
        Debug.Log($"[PlayerStats] +{amount:F0}  |  Balance: {CurrentCurrency:F0}  |  Total earned: {TotalCurrencyEarned:F0}");
    }

    /// <summary>
    /// Deducts amount from CurrentCurrency only. Returns false if insufficient funds.
    /// TotalCurrencyEarned is never reduced.
    /// </summary>
    public bool SpendCurrency(float amount)
    {
        if (amount <= 0f) return true;
        if (CurrentCurrency < amount) return false;
        CurrentCurrency -= amount;
        OnCurrencyChanged?.Invoke();
        Debug.Log($"[PlayerStats] -{amount:F0}  |  Balance: {CurrentCurrency:F0}");
        return true;
    }

    /// <summary>Returns the cost of the next farm plot: 10 × 2^FarmPlotsPurchased.</summary>
    public int GetNextFarmPlotCost() => Mathf.RoundToInt(10f * Mathf.Pow(2f, FarmPlotsPurchased));

    /// <summary>Called by ShopManager after a successful farm plot purchase.</summary>
    public void RecordFarmPlotPurchase() => FarmPlotsPurchased++;

    /// <summary>Fired whenever CurrentCurrency or TotalCurrencyEarned changes. HUD subscribes.</summary>
    public event System.Action OnCurrencyChanged;
}
