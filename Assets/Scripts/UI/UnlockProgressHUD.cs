// =============================================================================
// UnlockProgressHUD.cs  |  Scripts/UI
// WaifuGarden — Phase 3
// Displays "X / Y G" where X = TotalCurrencyEarned and Y = the threshold of
// the next locked shop item. Sits below the currency display.
// When all items are unlocked, shows "All items unlocked!".
// =============================================================================

using System.Linq;
using UnityEngine;
using TMPro;

public class UnlockProgressHUD : MonoBehaviour
{
    [Tooltip("TextMeshProUGUI showing e.g. '127 / 2000 G'.")]
    public TextMeshProUGUI ProgressLabel;

    [Tooltip("Format for in-progress state. {0} = total earned, {1} = next threshold.")]
    public string InProgressFormat = "{0:F0} / {1:F0} G";

    [Tooltip("Text shown when all items are unlocked.")]
    public string AllUnlockedText = "All items unlocked!";

    // -------------------------------------------------------------------------

    private void Start()
    {
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.OnCurrencyChanged += Refresh;
            Refresh();
        }
    }

    private void OnDestroy()
    {
        if (PlayerStats.Instance != null)
            PlayerStats.Instance.OnCurrencyChanged -= Refresh;
    }

    // -------------------------------------------------------------------------

    private void Refresh()
    {
        if (ProgressLabel == null || PlayerStats.Instance == null) return;

        float totalEarned = PlayerStats.Instance.TotalCurrencyEarned;
        long? nextThreshold = GetNextUnlockThreshold(totalEarned);

        if (nextThreshold == null)
            ProgressLabel.text = AllUnlockedText;
        else
            ProgressLabel.text = string.Format(InProgressFormat, totalEarned, nextThreshold.Value);
    }

    private static long? GetNextUnlockThreshold(float totalEarned)
    {
        if (DataRegistry.Instance == null) return null;

        long? lowest = null;

        foreach (ShopItemData item in DataRegistry.Instance.AllShopItems)
        {
            if (item == null) continue;
            if (item.UnlockAtTotalCurrency > (long)totalEarned)
            {
                if (lowest == null || item.UnlockAtTotalCurrency < lowest.Value)
                    lowest = item.UnlockAtTotalCurrency;
            }
        }

        foreach (ToolData tool in DataRegistry.Instance.AllTools)
        {
            if (tool == null) continue;
            if (tool.UnlockAtTotalCurrency > (long)totalEarned)
            {
                if (lowest == null || tool.UnlockAtTotalCurrency < lowest.Value)
                    lowest = tool.UnlockAtTotalCurrency;
            }
        }

        return lowest;
    }
}
