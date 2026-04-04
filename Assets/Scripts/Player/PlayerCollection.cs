// =============================================================================
// PlayerCollection.cs  |  Scripts/Player
// WaifuGarden — Phase 0
// Tracks all content the player has discovered and relationship point totals.
// All data is in-memory for prototype. SaveManager will persist it in v1.0.
// Attach to the GameManager GameObject.
// =============================================================================

using System.Collections.Generic;
using UnityEngine;

public class PlayerCollection : MonoBehaviour
{
    public static PlayerCollection Instance { get; private set; }

    // -------------------------------------------------------------------------
    // Discovery sets — one entry per content type.
    // -------------------------------------------------------------------------
    private readonly HashSet<string> _discoveredPlantIDs     = new HashSet<string>();
    private readonly HashSet<string> _discoveredCharacterIDs = new HashSet<string>();
    private readonly HashSet<string> _discoveredToolIDs      = new HashSet<string>();

    // -------------------------------------------------------------------------
    // Relationship points per character.
    // Key: CharacterID.  Value: cumulative points earned.
    // RelationshipLevel = floor(points / CharacterData.RelationshipPointsPerLevel).
    // -------------------------------------------------------------------------
    private readonly Dictionary<string, int> _relationshipPoints = new Dictionary<string, int>();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // -------------------------------------------------------------------------
    // Discovery API
    // -------------------------------------------------------------------------

    public void DiscoverPlant(string plantID)
    {
        if (_discoveredPlantIDs.Add(plantID)) { Debug.Log($"[Collection] Plant discovered: {plantID}"); OnDiscoveryChanged?.Invoke(); }
    }
    public void DiscoverCharacter(string characterID)
    {
        if (_discoveredCharacterIDs.Add(characterID)) { Debug.Log($"[Collection] Character discovered: {characterID}"); OnDiscoveryChanged?.Invoke(); }
    }
    public void DiscoverTool(string itemID)
    {
        if (_discoveredToolIDs.Add(itemID)) { Debug.Log($"[Collection] Tool discovered: {itemID}"); OnDiscoveryChanged?.Invoke(); }
    }

    public bool HasDiscoveredPlant(string id)     => _discoveredPlantIDs.Contains(id);
    public bool HasDiscoveredCharacter(string id) => _discoveredCharacterIDs.Contains(id);
    public bool HasDiscoveredTool(string id)      => _discoveredToolIDs.Contains(id);

    public IEnumerable<string> GetAllDiscoveredCharacterIDs() => _discoveredCharacterIDs;

    // -------------------------------------------------------------------------
    // Relationship API
    // -------------------------------------------------------------------------

    /// <summary>Awards relationship points. Character must already be discovered.</summary>
    public void AwardRelationshipPoints(string characterID, int points)
    {
        if (!_discoveredCharacterIDs.Contains(characterID))
        {
            Debug.LogWarning($"[Collection] Cannot award points to undiscovered character: {characterID}");
            return;
        }
        _relationshipPoints.TryGetValue(characterID, out int current);
        _relationshipPoints[characterID] = current + points;
        Debug.Log($"[Collection] +{points} pts for {characterID}. Total: {_relationshipPoints[characterID]}");
        OnRelationshipChanged?.Invoke(characterID);
    }

    public int GetRelationshipPoints(string characterID)
    {
        _relationshipPoints.TryGetValue(characterID, out int pts);
        return pts;
    }

    /// <summary>Returns the current relationship level given the per-level threshold.</summary>
    public int GetRelationshipLevel(string characterID, int pointsPerLevel)
    {
        if (pointsPerLevel <= 0) return 0;
        return GetRelationshipPoints(characterID) / pointsPerLevel;
    }

    // -------------------------------------------------------------------------
    // Events
    // -------------------------------------------------------------------------

    /// <summary>Fired when any discovery set changes. Catalogue UI subscribes.</summary>
    public event System.Action OnDiscoveryChanged;

    /// <summary>Fired when a character's relationship points change. Passes characterID.</summary>
    public event System.Action<string> OnRelationshipChanged;
}
