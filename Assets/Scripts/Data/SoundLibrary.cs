// =============================================================================
// SoundLibrary.cs  |  Scripts/Data
// WaifuGarden — Phase 0
// ScriptableObject mapping string sound IDs → AudioClip assets.
// AudioManager holds one instance and calls GetClip() at runtime.
// Create ONE asset: Right-click > Create > WaifuGarden > Sound Library
// Assign that single asset to AudioManager in the Inspector.
// =============================================================================

using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SoundEntry
{
    [Tooltip("The ID string used in code: AudioManager.PlaySFX(\"seed_planted\")")]
    public string SoundID;
    [Tooltip("Assign a silent placeholder AudioClip in prototype. Swap for real audio later — no code changes.")]
    public AudioClip Clip;
}

[CreateAssetMenu(fileName = "SoundLibrary", menuName = "WaifuGarden/Sound Library")]
public class SoundLibrary : ScriptableObject
{
    [Tooltip("All sound entries. Order does not matter. Duplicate IDs: first entry wins.")]
    public List<SoundEntry> Sounds = new List<SoundEntry>();

    private Dictionary<string, AudioClip> _lookup;

    public void Initialize()
    {
        _lookup = new Dictionary<string, AudioClip>(Sounds.Count);
        foreach (var e in Sounds)
        {
            if (string.IsNullOrEmpty(e.SoundID)) continue;
            if (_lookup.ContainsKey(e.SoundID)) { Debug.LogWarning($"[SoundLibrary] Duplicate ID '{e.SoundID}'."); continue; }
            _lookup[e.SoundID] = e.Clip; // null clips are allowed (silent placeholders)
        }
    }

    public AudioClip GetClip(string soundID)
    {
        if (_lookup == null) Initialize();
        _lookup.TryGetValue(soundID, out AudioClip clip);
        return clip;
    }
}

// ---- Required sound IDs (add these entries in the Inspector) ----------------
// seed_planted  harvest        stage_transition  evolution
// item_sold     item_bought    watering_can      fertilizer
// shovel        farmplot_placed dialogue_advance  dialogue_choice
// event_start   event_end      ui_open           ui_close
// -----------------------------------------------------------------------------
