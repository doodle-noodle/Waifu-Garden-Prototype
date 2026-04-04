// =============================================================================
// ShopItemData.cs  |  Scripts/Data
// WaifuGarden — Phase 0
// ScriptableObject defining a purchasable shop item (seeds, farm plots).
// Tools use ToolData.cs which extends this class — do not use this for tools.
// Create assets: Right-click > Create > WaifuGarden > Shop Item Data
// =============================================================================

using UnityEngine;

[CreateAssetMenu(fileName = "NewShopItemData", menuName = "WaifuGarden/Shop Item Data")]
public class ShopItemData : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("Unique key across ALL shop items (e.g. 'Seed_Carrot', 'FarmPlot').")]
    public string ItemID;
    public string ItemName;

    [Header("Display")]
    public Sprite ItemIcon;
    [TextArea(2, 4)]
    public string Description;

    [Header("Economy")]
    public int BuyCost;
    public ShopItemType ItemType;
    [Tooltip("-1 = unlimited purchases. 1 = buy once (permanent tools).")]
    public int MaxPurchaseCount = -1;
    [Tooltip("Hidden from shop until TotalCurrencyEarned reaches this. 0 = available from start.")]
    public long UnlockAtTotalCurrency = 0;

    [Header("Seed Link")]
    [Tooltip("For Seed items only: PlantID of the plant this seed grows. " +
             "Must match a PlantData.PlantID. Leave empty for non-seed items.")]
    public string LinkedPlantID;

    [Header("Catalogue")]
    [TextArea(2, 4)]
    public string CatalogueHint;
}
