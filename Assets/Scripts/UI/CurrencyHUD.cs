// =============================================================================
// CurrencyHUD.cs  |  Scripts/UI
// WaifuGarden — Phase 1
// Displays the player's current spendable currency balance.
// Subscribes to PlayerStats.OnCurrencyChanged and updates itself automatically.
// Attach to the currency display GameObject in the main HUD Canvas.
// =============================================================================

using UnityEngine;
using TMPro;

public class CurrencyHUD : MonoBehaviour
{
    [Tooltip("TextMeshProUGUI component that shows the currency amount.")]
    public TextMeshProUGUI CurrencyLabel;

    [Tooltip("Format string for the amount. {0} is the value. Example: '¥{0:F0}'")]
    public string Format = "¥{0:F0}";

    // -------------------------------------------------------------------------

    private void Start()
    {
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.OnCurrencyChanged += Refresh;
            Refresh(); // show correct value immediately
        }
        else
        {
            Debug.LogWarning("[CurrencyHUD] PlayerStats not found.");
        }
    }

    private void OnDestroy()
    {
        if (PlayerStats.Instance != null)
            PlayerStats.Instance.OnCurrencyChanged -= Refresh;
    }

    private void Refresh()
    {
        if (CurrencyLabel == null || PlayerStats.Instance == null) return;
        CurrencyLabel.text = string.Format(Format, PlayerStats.Instance.CurrentCurrency);
    }
}
