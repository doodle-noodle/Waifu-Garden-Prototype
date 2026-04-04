// =============================================================================
// EvolutionRecipe.cs  |  Scripts/Data
// WaifuGarden — Phase 0
// ScriptableObject defining conditions needed to evolve a plant into a character.
// Create assets: Right-click > Create > WaifuGarden > Evolution Recipe
// One asset per evolution (StrangePlant→SunflowerGirl, Apple→AppleGirl, Mushroom→MushroomGirl).
// These assets are then placed in the matching PlantData.Evolutions list.
// =============================================================================

using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEvolutionRecipe", menuName = "WaifuGarden/Evolution Recipe")]
public class EvolutionRecipe : ScriptableObject
{
    [Header("Identity")]
    public string RecipeID;

    [Header("Requirements")]
    [Tooltip("Must match a PlantData.PlantID exactly.")]
    public string RequiredPlantID;
    [Tooltip("All modifiers that MUST be present. Extra modifiers beyond this list are fine.")]
    public List<string> RequiredModifierIDs = new List<string>();
    [Tooltip("World event that must be active. Leave EMPTY to allow evolution under any event.")]
    public string RequiredWorldEventID = "";

    [Header("Result")]
    [Tooltip("Must match a CharacterData.CharacterID exactly.")]
    public string ResultCharacterID;

    [Header("Priority")]
    [Tooltip("Lower number = checked first when a plant has multiple possible evolutions.")]
    public int SortOrder = 0;
}
