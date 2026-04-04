// =============================================================================
// PlantData.cs  |  Scripts/Data
// WaifuGarden — Phase 0
// ScriptableObject defining one plant species.
// Create assets: Right-click in Project > Create > WaifuGarden > Plant Data
// One asset per plant (Carrot, Tomato, Apple, Mushroom, Pepper, Pumpkin, StrangePlant).
// =============================================================================

using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPlantData", menuName = "WaifuGarden/Plant Data")]
public class PlantData : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("Unique key used in code (e.g. 'Carrot'). Must match LinkedPlantID on the seed's ShopItemData.")]
    public string PlantID;
    public string PlantName;

    [Header("Sprites")]
    public Sprite SeedSprite;
    public Sprite SproutSprite;
    public Sprite MatureSprite;

    [Header("Growth Timing (seconds)")]
    [Tooltip("Time from Seed → Sprout at base speed. Set to 0 for tutorial-controlled plants.")]
    public float SeedToSproutTime   = 5f;
    [Tooltip("Time from Sprout → Mature at base speed. Set to 0 for tutorial-controlled plants.")]
    public float SproutToMatureTime = 5f;

    [Header("Economy")]
    [Tooltip("Base currency value on harvest, before modifier and character multipliers.")]
    public float BaseHarvestValue = 10f;

    [Header("Catalogue")]
    [TextArea(2, 4)]
    public string ShopUnlockHint;

    [Header("Tutorial")]
    [Tooltip("TRUE only on Strange Plant. Freezes the growth timer until TutorialManager permits it. " +
             "Has no effect when TutorialEnabled = false.")]
    public bool TutorialPlantMode = false;

    [Header("Evolutions")]
    [Tooltip("Possible evolutions from this plant. Checked in SortOrder when plant reaches Mature.")]
    public List<EvolutionRecipe> Evolutions = new List<EvolutionRecipe>();
}
