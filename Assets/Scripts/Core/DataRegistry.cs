// =============================================================================
// DataRegistry.cs  |  Scripts/Core
// WaifuGarden — Phase 1
// Central lookup table for all ScriptableObject assets.
// Systems never search Resources/ folders or hold direct SO references.
// They call:  DataRegistry.Instance.GetPlant("Carrot")
// Attach to the GameManager GameObject. Populate all lists in the Inspector.
// =============================================================================

using System.Collections.Generic;
using UnityEngine;

public class DataRegistry : MonoBehaviour
{
    public static DataRegistry Instance { get; private set; }

    // -------------------------------------------------------------------------
    // Inspector lists.
    // Drag EVERY ScriptableObject asset of each type into the matching list.
    // ToolData assets go in AllTools — NOT in AllShopItems.
    // -------------------------------------------------------------------------

    [Header("Plants")]
    public List<PlantData> AllPlants = new List<PlantData>();

    [Header("Characters")]
    public List<CharacterData> AllCharacters = new List<CharacterData>();

    [Header("Modifiers")]
    public List<ModifierData> AllModifiers = new List<ModifierData>();

    [Header("World Events")]
    public List<WorldEventData> AllWorldEvents = new List<WorldEventData>();

    [Header("Evolution Recipes")]
    public List<EvolutionRecipe> AllEvolutionRecipes = new List<EvolutionRecipe>();

    [Header("Shop Items  (seeds + farm plots — do NOT put tools here)")]
    public List<ShopItemData> AllShopItems = new List<ShopItemData>();

    [Header("Tools  (Shovel, WateringCan, Fertilizer)")]
    public List<ToolData> AllTools = new List<ToolData>();

    // -------------------------------------------------------------------------
    // Runtime lookup dictionaries — built once in Awake.
    // -------------------------------------------------------------------------

    private readonly Dictionary<string, PlantData>       _plants     = new Dictionary<string, PlantData>();
    private readonly Dictionary<string, CharacterData>   _characters = new Dictionary<string, CharacterData>();
    private readonly Dictionary<string, ModifierData>    _modifiers  = new Dictionary<string, ModifierData>();
    private readonly Dictionary<string, WorldEventData>  _events     = new Dictionary<string, WorldEventData>();
    private readonly Dictionary<string, EvolutionRecipe> _recipes    = new Dictionary<string, EvolutionRecipe>();
    private readonly Dictionary<string, ShopItemData>    _shopItems  = new Dictionary<string, ShopItemData>();
    private readonly Dictionary<string, ToolData>        _tools      = new Dictionary<string, ToolData>();

    // -------------------------------------------------------------------------

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        BuildAllLookups();
    }

    private void BuildAllLookups()
    {
        BuildDict(AllPlants,           _plants,     p => p.PlantID,      "PlantData");
        BuildDict(AllCharacters,       _characters, c => c.CharacterID,  "CharacterData");
        BuildDict(AllModifiers,        _modifiers,  m => m.ModifierID,   "ModifierData");
        BuildDict(AllWorldEvents,      _events,     e => e.EventID,      "WorldEventData");
        BuildDict(AllEvolutionRecipes, _recipes,    r => r.RecipeID,     "EvolutionRecipe");
        BuildDict(AllShopItems,        _shopItems,  s => s.ItemID,       "ShopItemData");
        BuildDict(AllTools,            _tools,      t => t.ItemID,       "ToolData");

        Debug.Log($"[DataRegistry] Ready — Plants:{_plants.Count} | Characters:{_characters.Count} | " +
                  $"Modifiers:{_modifiers.Count} | Events:{_events.Count} | " +
                  $"ShopItems:{_shopItems.Count} | Tools:{_tools.Count}");
    }

    private static void BuildDict<T>(
        List<T> source,
        Dictionary<string, T> target,
        System.Func<T, string> keySelector,
        string typeName)
        where T : UnityEngine.Object
    {
        foreach (T item in source)
        {
            if (item == null) continue;
            string key = keySelector(item);
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogWarning($"[DataRegistry] A {typeName} asset has an empty ID — skipped.");
                continue;
            }
            if (target.ContainsKey(key))
            {
                Debug.LogWarning($"[DataRegistry] Duplicate {typeName} ID '{key}' — keeping first.");
                continue;
            }
            target[key] = item;
        }
    }

    // -------------------------------------------------------------------------
    // Lookup API
    // All methods return null (with a warning) if the ID is not registered.
    // -------------------------------------------------------------------------

    public PlantData       GetPlant(string id)       => Lookup(_plants,     id, "PlantData");
    public CharacterData   GetCharacter(string id)   => Lookup(_characters, id, "CharacterData");
    public ModifierData    GetModifier(string id)    => Lookup(_modifiers,  id, "ModifierData");
    public WorldEventData  GetWorldEvent(string id)  => Lookup(_events,     id, "WorldEventData");
    public EvolutionRecipe GetRecipe(string id)      => Lookup(_recipes,    id, "EvolutionRecipe");
    public ToolData        GetTool(string id)        => Lookup(_tools,      id, "ToolData");

    /// <summary>
    /// Returns ShopItemData for the given ItemID.
    /// Checks the Tools dictionary first (since ToolData : ShopItemData).
    /// </summary>
    public ShopItemData GetShopItem(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        if (_tools.TryGetValue(id, out ToolData tool)) return tool;
        return Lookup(_shopItems, id, "ShopItemData");
    }

    private T Lookup<T>(Dictionary<string, T> dict, string id, string typeName) where T : class
    {
        if (string.IsNullOrEmpty(id)) return null;
        if (dict.TryGetValue(id, out T val)) return val;
        Debug.LogWarning($"[DataRegistry] {typeName} with ID '{id}' not found.");
        return null;
    }
}
