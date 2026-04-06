// =============================================================================
// CurrencyHUD.cs  |  Scripts/UI
// WaifuGarden — Phase 3
// Updated: default format changed from ¥{0:F0} to {0:F0} G
// =============================================================================

using UnityEngine;
using TMPro;

public class CurrencyHUD : MonoBehaviour
{
    [Tooltip("TextMeshProUGUI showing the current spendable balance.")]
    public TextMeshProUGUI CurrencyLabel;

    [Tooltip("Format string. {0} = current currency. Default: '{0:F0} G'")]
    public string Format = "{0:F0} G";

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

    private void Refresh()
    {
        if (CurrencyLabel == null || PlayerStats.Instance == null) return;
        CurrencyLabel.text = string.Format(Format, PlayerStats.Instance.CurrentCurrency);
    }
}
