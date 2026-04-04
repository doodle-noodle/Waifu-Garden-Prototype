// =============================================================================
// CharacterData.cs  |  Scripts/Data
// WaifuGarden — Phase 0
// ScriptableObject defining one evolvable garden character.
// Create assets: Right-click > Create > WaifuGarden > Character Data
// One asset per character (SunflowerGirl, AppleGirl, MushroomGirl).
// =============================================================================

using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacterData", menuName = "WaifuGarden/Character Data")]
public class CharacterData : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("Unique key (e.g. 'SunflowerGirl'). Must match ResultCharacterID on EvolutionRecipe assets.")]
    public string CharacterID;
    public string CharacterName;

    [Header("Sprites")]
    [Tooltip("Full-size portrait shown in Evolution Screen and Character Interaction Panel.")]
    public Sprite PortraitSprite;
    [Tooltip("Small potted-plant sprite shown in the Greenhouse grid.")]
    public Sprite PottedPlantSprite;

    [Header("Dialogue (Yarn Spinner)")]
    [Tooltip("Entry node name in this character's .yarn file (e.g. 'SunflowerGirl_Start').")]
    public string YarnDialogueStartNode;

    [Header("Passive Bonus")]
    public PassiveBonusType BonusType;
    [Tooltip("GrowthSpeed / SellValue: fraction added to 1 (0.10 = +10%). " +
             "MutationChance: flat probability added per tick (0.10 = +10pp).")]
    public float PassiveBonusValue = 0.10f;

    [Header("Relationship")]
    [Tooltip("Points needed to advance one relationship level. Level = floor(points / this).")]
    public int RelationshipPointsPerLevel = 5;
    [Tooltip("At relationship level 2, passive bonus is multiplied by this value (e.g. 1.5 = +50% stronger bonus).")]
    public float Level2BonusMultiplier = 1.5f;

    [Header("Catalogue")]
    [TextArea(2, 4)]
    public string CatalogueHint;
}
