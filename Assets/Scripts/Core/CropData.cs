// =============================================================================
// CropData.cs  |  Scripts/Core
// WaifuGarden — Phase 0
// Plain C# class (NOT a ScriptableObject) representing one harvested crop.
// Each harvest creates a new runtime instance stored in PlayerInventory.
// The sell value is calculated once at harvest time and cached here.
// =============================================================================

using System.Collections.Generic;

[System.Serializable]
public class CropData
{
    /// <summary>PlantID of the source plant. Links to PlantData for name and sprite.</summary>
    public string PlantID;

    /// <summary>Display name cached at harvest (safe if PlantData asset is later renamed).</summary>
    public string PlantName;

    /// <summary>
    /// Final sell value calculated at harvest:
    ///   BaseHarvestValue × modifier multipliers (sequential) × character bonus multiplier.
    /// Cached here so the sell tab always shows the correct value instantly.
    /// </summary>
    public float FinalSellValue;

    /// <summary>Snapshot of modifier IDs active at the moment of harvest. Shown in sell tab.</summary>
    public List<string> AppliedModifierIDs = new List<string>();

    public CropData(string plantID, string plantName, float finalSellValue, List<string> modifiers)
    {
        PlantID           = plantID;
        PlantName         = plantName;
        FinalSellValue    = finalSellValue;
        AppliedModifierIDs = modifiers != null ? new List<string>(modifiers) : new List<string>();
    }
}
