// =============================================================================
// ModifierData.cs  |  Scripts/Data
// WaifuGarden — Phase 0
// ScriptableObject defining one plant modifier (status effect) and any
// combination rules it participates in as the RESULT.
// Create assets: Right-click > Create > WaifuGarden > Modifier Data
// One asset per modifier (Wet, Chilled, Frozen, Sunkissed, Moonlit, Bloody, Fertilized).
// =============================================================================

using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CombinationRule
{
    [Tooltip("All modifier IDs that must be present on the plant to trigger this rule.")]
    public List<string> RequiredModifierIDs = new List<string>();
    [Tooltip("The modifier produced. The required modifiers are consumed; this one is added.")]
    public string ResultModifierID;
}

[CreateAssetMenu(fileName = "NewModifierData", menuName = "WaifuGarden/Modifier Data")]
public class ModifierData : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("Unique key (e.g. 'Wet'). Used throughout code to reference this modifier.")]
    public string ModifierID;
    public string ModifierName;

    [Header("Display")]
    [Tooltip("Small icon shown in the modifier strip below a plant on its slot.")]
    public Sprite ModifierIcon;
    [TextArea(2, 4)]
    public string Description;

    [Header("Economy")]
    [Tooltip("Multiplied into crop sell value when present. 1.0 = no change. 2.0 = doubles value.")]
    public float SellValueMultiplier = 1.0f;

    [Header("Combination Rules")]
    [Tooltip("Put rules on the RESULT modifier's asset. " +
             "Example: the Frozen asset holds the rule {Wet + Chilled → Frozen}. " +
             "ModifierSystem checks all rules from all assets after any modifier is added.")]
    public List<CombinationRule> CombinationRules = new List<CombinationRule>();
}
