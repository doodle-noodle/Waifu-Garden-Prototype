// =============================================================================
// PlayerCollection.cs  |  Scripts/Player
// WaifuGarden — Pre-Phase 2 Fixes
// Fix: GetAllDiscoveredCharacterIDs() returns a snapshot copy, not the live set.
// =============================================================================

using System.Collections.Generic;
using UnityEngine;

public class PlayerCollection : MonoBehaviour
{
    public static PlayerCollection Instance { get; private set; }

    private readonly HashSet<string> _discoveredPlantIDs     = new HashSet<string>();
    private readonly HashSet<string> _discoveredCharacterIDs = new HashSet<string>();
    private readonly HashSet<string> _discoveredToolIDs      = new HashSet<string>();
    private readonly Dictionary<string, int> _relationshipPoints = new Dictionary<string, int>();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // -------------------------------------------------------------------------
    // Discovery API
    // -------------------------------------------------------------------------

    public void DiscoverPlant(string id)
    {
        if (_discoveredPlantIDs.Add(id)) { Debug.Log($"[Collection] Plant discovered: {id}"); OnDiscoveryChanged?.Invoke(); }
    }
    public void DiscoverCharacter(string id)
    {
        if (_discoveredCharacterIDs.Add(id)) { Debug.Log($"[Collection] Character discovered: {id}"); OnDiscoveryChanged?.Invoke(); }
    }
    public void DiscoverTool(string id)
    {
        if (_discoveredToolIDs.Add(id)) { Debug.Log($"[Collection] Tool discovered: {id}"); OnDiscoveryChanged?.Invoke(); }
    }

    public bool HasDiscoveredPlant(string id)     => _discoveredPlantIDs.Contains(id);
    public bool HasDiscoveredCharacter(string id) => _discoveredCharacterIDs.Contains(id);
    public bool HasDiscoveredTool(string id)      => _discoveredToolIDs.Contains(id);

    /// <summary>Returns a snapshot copy. Safe to iterate even if collection changes.</summary>
    public IEnumerable<string> GetAllDiscoveredCharacterIDs() => new List<string>(_discoveredCharacterIDs);

    // -------------------------------------------------------------------------
    // Relationship API
    // -------------------------------------------------------------------------

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

    public int GetRelationshipLevel(string characterID, int pointsPerLevel)
    {
        if (pointsPerLevel <= 0) return 0;
        return GetRelationshipPoints(characterID) / pointsPerLevel;
    }

    public event System.Action         OnDiscoveryChanged;
    public event System.Action<string> OnRelationshipChanged;
}
