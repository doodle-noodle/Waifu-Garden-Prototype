// =============================================================================
// ShopSellTabUI.cs  |  Scripts/UI
// WaifuGarden — Phase 2
// Manages the content of the Sell tab inside ShopPanel.
// Lists all unsold CropData from PlayerInventory with individual Sell buttons
// and a Sell All button at the bottom.
// =============================================================================

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopSellTabUI : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Scrollable content container where SellSlotUI entries are spawned.")]
    public RectTransform ContentContainer;

    [Tooltip("Prefab with a SellSlotUI component.")]
    public GameObject SellSlotPrefab;

    [Tooltip("Button that sells all crops at once.")]
    public Button SellAllButton;

    [Tooltip("Label shown when there are no crops to sell.")]
    public TextMeshProUGUI EmptyLabel;

    // -------------------------------------------------------------------------
    private readonly List<SellSlotUI> _activeSlots = new List<SellSlotUI>();

    // -------------------------------------------------------------------------

    private void OnEnable()
    {
        if (PlayerInventory.Instance != null)
            PlayerInventory.Instance.OnInventoryChanged += Refresh;

        if (SellAllButton != null)
            SellAllButton.onClick.AddListener(SellAll);

        Refresh();
    }

    private void OnDisable()
    {
        if (PlayerInventory.Instance != null)
            PlayerInventory.Instance.OnInventoryChanged -= Refresh;

        if (SellAllButton != null)
            SellAllButton.onClick.RemoveListener(SellAll);
    }

    // -------------------------------------------------------------------------

    private void Refresh()
    {
        if (ContentContainer == null || SellSlotPrefab == null) return;

        // Clear existing entries
        foreach (Transform child in ContentContainer)
            Destroy(child.gameObject);
        _activeSlots.Clear();

        List<CropData> crops = PlayerInventory.Instance?.HarvestedCrops;
        bool hasCrops = crops != null && crops.Count > 0;

        if (EmptyLabel != null)
            EmptyLabel.gameObject.SetActive(!hasCrops);

        if (!hasCrops) return;

        foreach (CropData crop in crops)
        {
            GameObject go  = Instantiate(SellSlotPrefab, ContentContainer);
            SellSlotUI  ui = go.GetComponent<SellSlotUI>();
            if (ui != null)
            {
                ui.SetCrop(crop);
                _activeSlots.Add(ui);
            }
        }

        if (SellAllButton != null)
            SellAllButton.interactable = hasCrops;
    }

    private void SellAll()
    {
        List<CropData> crops = PlayerInventory.Instance?.HarvestedCrops;
        if (crops == null || crops.Count == 0) return;

        // Snapshot the list since we'll be modifying it while iterating.
        var snapshot = new List<CropData>(crops);
        float totalValue = 0f;

        foreach (CropData crop in snapshot)
        {
            totalValue += crop.FinalSellValue;
            PlayerInventory.Instance.RemoveCrop(crop);
        }

        PlayerStats.Instance?.EarnCurrency(totalValue);
        AudioManager.Instance?.PlaySFX("item_sold");
        Debug.Log($"[ShopSellTabUI] Sold all crops for ¥{totalValue:F0}.");
    }
}
