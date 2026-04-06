// =============================================================================
// ShopItemSlotUI.cs  |  Scripts/UI
// WaifuGarden — Phase 3
// One row in the Buy tab. Displays icon, name, description, cost, and a Buy
// button. Handles three special states:
//   - Greyed-out Buy button if player cannot afford
//   - "Owned" label instead of Buy button for permanent items already purchased
//   - Teaser mode: shows ??? with hint text and no Buy button
// Created dynamically by ShopBuyTabUI — do not add to scene manually.
// =============================================================================

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopItemSlotUI : MonoBehaviour
{
    [Header("Display")]
    public Image            ItemIcon;
    public TextMeshProUGUI  ItemNameLabel;
    public TextMeshProUGUI  DescriptionLabel;
    public TextMeshProUGUI  CostLabel;

    [Header("Buttons / State")]
    public Button           BuyButton;
    public TextMeshProUGUI  BuyButtonLabel;
    public GameObject       OwnedBadge;        // "Owned" label, shown for permanent items

    // -------------------------------------------------------------------------
    private ShopItemData _data;
    private bool         _isTeaser;

    // -------------------------------------------------------------------------

    /// <summary>Sets up this slot as a real purchasable item.</summary>
    public void SetItem(ShopItemData data)
    {
        _data     = data;
        _isTeaser = false;

        if (ItemIcon       != null) { ItemIcon.sprite = data.ItemIcon; ItemIcon.enabled = data.ItemIcon != null; }
        if (ItemNameLabel  != null) ItemNameLabel.text  = data.ItemName;
        if (DescriptionLabel != null) DescriptionLabel.text = data.Description;

        RefreshState();
    }

    /// <summary>Sets up this slot as the teaser (??? entry for next locked item).</summary>
    public void SetTeaser(ShopItemData nextLockedItem)
    {
        _data     = nextLockedItem;
        _isTeaser = true;

        if (ItemIcon       != null) { ItemIcon.sprite = null; ItemIcon.enabled = false; }
        if (ItemNameLabel  != null) ItemNameLabel.text   = "???";
        if (DescriptionLabel != null)
            DescriptionLabel.text = nextLockedItem?.CatalogueHint ?? "Keep earning to unlock more.";
        if (CostLabel      != null) CostLabel.text        = "";
        if (BuyButton      != null) BuyButton.gameObject.SetActive(false);
        if (OwnedBadge     != null) OwnedBadge.SetActive(false);
    }

    /// <summary>Refreshes button state — called on every currency change.</summary>
    public void RefreshState()
    {
        if (_data == null || _isTeaser) return;

        bool isPermanent  = _data.MaxPurchaseCount == 1;
        bool alreadyOwned = isPermanent && PlayerInventory.Instance != null
                            && PlayerInventory.Instance.HasItem(_data.ItemID);

        // Dynamic farm plot price
        int cost = IsFarmPlot(_data)
            ? PlayerStats.Instance?.GetNextFarmPlotCost() ?? _data.BuyCost
            : _data.BuyCost;

        if (CostLabel != null) CostLabel.text = $"{cost} G";

        if (OwnedBadge != null) OwnedBadge.SetActive(alreadyOwned);

        if (BuyButton != null)
        {
            if (alreadyOwned)
            {
                BuyButton.gameObject.SetActive(false);
            }
            else
            {
                BuyButton.gameObject.SetActive(true);
                bool canAfford = PlayerStats.Instance != null
                                 && PlayerStats.Instance.CurrentCurrency >= cost;
                BuyButton.interactable = canAfford;

                if (BuyButtonLabel != null)
                    BuyButtonLabel.color = canAfford ? Color.white : new Color(0.5f, 0.5f, 0.5f, 1f);

                BuyButton.onClick.RemoveAllListeners();
                BuyButton.onClick.AddListener(() => OnBuyClicked(cost));
            }
        }
    }

    // -------------------------------------------------------------------------

    private void OnBuyClicked(int cost)
    {
        if (_data == null || PlayerStats.Instance == null) return;

        if (!PlayerStats.Instance.SpendCurrency(cost)) return;

        // Determine quantity added to inventory
        int quantity = 1;
        if (_data is ToolData tool)
            quantity = tool.UsesPerPurchase > 0 ? tool.UsesPerPurchase : 1;

        PlayerInventory.Instance?.AddItem(_data.ItemID, quantity);
        PlayerCollection.Instance?.DiscoverTool(_data.ItemID);

        if (IsFarmPlot(_data))
            PlayerStats.Instance.RecordFarmPlotPurchase();

        AudioManager.Instance?.PlaySFX("item_bought");
        Debug.Log($"[ShopItemSlotUI] Bought {_data.ItemName} for {cost} G.");

        RefreshState();
    }

    private static bool IsFarmPlot(ShopItemData data) =>
        data != null && data.ItemType == ShopItemType.FarmPlot;
}
