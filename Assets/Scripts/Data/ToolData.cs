// =============================================================================
// ToolData.cs  |  Scripts/Data
// WaifuGarden — Phase 0
// ScriptableObject for tool items. Extends ShopItemData with tool-specific fields.
// Create assets: Right-click > Create > WaifuGarden > Tool Data
// ALWAYS use "Tool Data" for tools, never "Shop Item Data" — you need these fields.
// One asset per tool (Shovel, WateringCan, Fertilizer).
// =============================================================================

using UnityEngine;

[CreateAssetMenu(fileName = "NewToolData", menuName = "WaifuGarden/Tool Data")]
public class ToolData : ShopItemData
{
    [Header("Tool Configuration")]
    public ToolType Type;
    [Tooltip("Uses per purchase. -1 = permanent infinite (Shovel). WateringCan = 10. Fertilizer = 1.")]
    public int UsesPerPurchase = 1;
    [Tooltip("ModifierID applied when this tool is used on a plant. Empty = no modifier (Shovel).")]
    public string ModifierApplied = "";
    [Tooltip("Growth speed multiplier applied to the plant on use. WateringCan = 2.0. Others = 1.0.")]
    public float GrowthSpeedMultiplier = 1.0f;
}
