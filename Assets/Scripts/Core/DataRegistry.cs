// =============================================================================
// DataRegistry.cs  |  Scripts/Core
// WaifuGarden — Pre-Phase 2 Fixes
// Added: [ContextMenu] "Auto-populate from Assets" scans the ScriptableObjects
// folder and fills all lists automatically. Right-click the DataRegistry
// component in the Inspector and select that option after adding any new asset.
// =============================================================================

using System.Collections.Generic;
using UnityEngine;

public class DataRegistry : MonoBehaviour
{
    public static DataRegistry Instance { get; private set; }

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

    [Header("Shop Items  (seeds + farm plots — NOT tools)")]
    public List<ShopItemData> AllShopItems = new List<ShopItemData>();

    [Header("Tools  (Shovel, WateringCan, Fertilizer)")]
    public List<ToolData> AllTools = new List<ToolData>();

    // -------------------------------------------------------------------------
    // Runtime lookup dictionaries
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
        BuildDict(AllPlants,           _plants,     p => p.PlantID,     "PlantData");
        BuildDict(AllCharacters,       _characters, c => c.CharacterID, "CharacterData");
        BuildDict(AllModifiers,        _modifiers,  m => m.ModifierID,  "ModifierData");
        BuildDict(AllWorldEvents,      _events,     e => e.EventID,     "WorldEventData");
        BuildDict(AllEvolutionRecipes, _recipes,    r => r.RecipeID,    "EvolutionRecipe");
        BuildDict(AllShopItems,        _shopItems,  s => s.ItemID,      "ShopItemData");
        BuildDict(AllTools,            _tools,      t => t.ItemID,      "ToolData");

        Debug.Log($"[DataRegistry] Ready — Plants:{_plants.Count} | Characters:{_characters.Count} | " +
                  $"Modifiers:{_modifiers.Count} | Events:{_events.Count} | " +
                  $"ShopItems:{_shopItems.Count} | Tools:{_tools.Count}");
    }

    private static void BuildDict<T>(List<T> source, Dictionary<string, T> target,
        System.Func<T, string> keySelector, string typeName) where T : UnityEngine.Object
    {
        foreach (T item in source)
        {
            if (item == null) continue;
            string key = keySelector(item);
            if (string.IsNullOrEmpty(key)) { Debug.LogWarning($"[DataRegistry] {typeName} has empty ID."); continue; }
            if (target.ContainsKey(key))   { Debug.LogWarning($"[DataRegistry] Duplicate {typeName} ID '{key}'."); continue; }
            target[key] = item;
        }
    }

    // -------------------------------------------------------------------------
    // Lookup API
    // -------------------------------------------------------------------------

    public PlantData       GetPlant(string id)      => Lookup(_plants,     id, "PlantData");
    public CharacterData   GetCharacter(string id)  => Lookup(_characters, id, "CharacterData");
    public ModifierData    GetModifier(string id)   => Lookup(_modifiers,  id, "ModifierData");
    public WorldEventData  GetWorldEvent(string id) => Lookup(_events,     id, "WorldEventData");
    public EvolutionRecipe GetRecipe(string id)     => Lookup(_recipes,    id, "EvolutionRecipe");
    public ToolData        GetTool(string id)       => Lookup(_tools,      id, "ToolData");

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
        Debug.LogWarning($"[DataRegistry] {typeName} ID '{id}' not found.");
        return null;
    }

    // =========================================================================
    // Editor utility — right-click DataRegistry component → Auto-populate
    // Scans Assets/ScriptableObjects/ and fills all lists automatically.
    // Run this whenever you add a new ScriptableObject asset.
    // =========================================================================

#if UNITY_EDITOR
    [ContextMenu("Auto-populate all lists from Assets/ScriptableObjects")]
    private void AutoPopulate()
    {
        AllPlants.Clear();
        AllCharacters.Clear();
        AllModifiers.Clear();
        AllWorldEvents.Clear();
        AllEvolutionRecipes.Clear();
        AllShopItems.Clear();
        AllTools.Clear();

        LoadAssets("t:PlantData",       AllPlants);
        LoadAssets("t:CharacterData",   AllCharacters);
        LoadAssets("t:ModifierData",    AllModifiers);
        LoadAssets("t:WorldEventData",  AllWorldEvents);
        LoadAssets("t:EvolutionRecipe", AllEvolutionRecipes);

        // Tools must be loaded before ShopItems so ToolData assets don't end up
        // in both lists (ToolData inherits ShopItemData).
        LoadAssets("t:ToolData",        AllTools);

        // Load ShopItemData but exclude any asset that is also a ToolData.
        var allShopRaw = new List<ShopItemData>();
        LoadAssets("t:ShopItemData", allShopRaw);
        foreach (var item in allShopRaw)
            if (!(item is ToolData)) AllShopItems.Add(item);

        UnityEditor.EditorUtility.SetDirty(this);

        Debug.Log($"[DataRegistry] Auto-populated — " +
                  $"Plants:{AllPlants.Count} | Characters:{AllCharacters.Count} | " +
                  $"Modifiers:{AllModifiers.Count} | Events:{AllWorldEvents.Count} | " +
                  $"Recipes:{AllEvolutionRecipes.Count} | " +
                  $"ShopItems:{AllShopItems.Count} | Tools:{AllTools.Count}");
    }

    private static void LoadAssets<T>(string filter, List<T> target) where T : UnityEngine.Object
    {
        string[] guids = UnityEditor.AssetDatabase.FindAssets(filter,
            new[] { "Assets/ScriptableObjects" });
        foreach (string guid in guids)
        {
            string path  = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            T      asset = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) target.Add(asset);
        }
    }
#endif
}
