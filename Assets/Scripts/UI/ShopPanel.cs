// =============================================================================
// ShopPanel.cs  |  Scripts/UI
// WaifuGarden — Phase 2 Fix
// Lazy init pattern: never calls SetActive in Awake/Start.
// Panel stays in whatever state it was left in the Hierarchy.
// Toggle() manages all open/close state from the first click onward.
// =============================================================================

using UnityEngine;

public class ShopPanel : MonoBehaviour
{
    public static ShopPanel Instance { get; private set; }

    [Header("Panel Root")]
    [Tooltip("The root GameObject of the shop panel. Shown/hidden on open/close.")]
    public GameObject PanelRoot;

    [Header("Tab Content GameObjects")]
    public GameObject BuyTabContent;
    public GameObject SellTabContent;

    [Header("Tab Buttons (optional — for colour highlight)")]
    public UnityEngine.UI.Button BuyTabButton;
    public UnityEngine.UI.Button SellTabButton;

    [Header("Tab Button Colours")]
    public Color ActiveTabColour   = new Color(1f,   1f,   1f,   1f);
    public Color InactiveTabColour = new Color(0.7f, 0.7f, 0.7f, 1f);

    // -------------------------------------------------------------------------
    private bool _isOpen = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        // No SetActive calls here — lazy init avoids the multi-click state mismatch.
    }

    // -------------------------------------------------------------------------
    // Wire Shop HUD button OnClick → Toggle() in the Inspector.
    // -------------------------------------------------------------------------

    public void Toggle()
    {
        if (_isOpen) Close();
        else         Open();
    }

    public void Open()
    {
        if (_isOpen) return;
        _isOpen = true;
        if (PanelRoot != null) PanelRoot.SetActive(true);
        ShowSellTab();
        AudioManager.Instance?.PlaySFX("ui_open");
        var rt = PanelRoot != null ? PanelRoot.GetComponent<RectTransform>() : null;
        if (rt != null) AnimationHelper.PlayPanelSlideIn(rt);
    }

    public void Close()
    {
        if (!_isOpen) return;
        _isOpen = false;
        if (PanelRoot != null) PanelRoot.SetActive(false);
        AudioManager.Instance?.PlaySFX("ui_close");
    }

    // -------------------------------------------------------------------------
    // Wire these to Buy/Sell tab button OnClick lists in the Inspector.
    // -------------------------------------------------------------------------

    public void ShowBuyTab()
    {
        if (BuyTabContent  != null) BuyTabContent.SetActive(true);
        if (SellTabContent != null) SellTabContent.SetActive(false);
        SetTabHighlight(BuyTabButton,  true);
        SetTabHighlight(SellTabButton, false);
    }

    public void ShowSellTab()
    {
        if (BuyTabContent  != null) BuyTabContent.SetActive(false);
        if (SellTabContent != null) SellTabContent.SetActive(true);
        SetTabHighlight(BuyTabButton,  false);
        SetTabHighlight(SellTabButton, true);
    }

    private void SetTabHighlight(UnityEngine.UI.Button btn, bool active)
    {
        if (btn == null) return;
        var img = btn.GetComponent<UnityEngine.UI.Image>();
        if (img != null) img.color = active ? ActiveTabColour : InactiveTabColour;
    }
}
