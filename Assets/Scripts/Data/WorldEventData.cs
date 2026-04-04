// =============================================================================
// WorldEventData.cs  |  Scripts/Data
// WaifuGarden — Phase 0
// ScriptableObject defining one world event (weather / time-of-day state).
// Create assets: Right-click > Create > WaifuGarden > World Event Data
// One asset per event (Day, Night, Rain, Heatwave, Frost, BloodMoon).
// =============================================================================

using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ModifierApplication
{
    [Tooltip("ModifierID to attempt to apply to each growing plant each modifier tick.")]
    public string ModifierID;
    [Range(0f, 1f)]
    [Tooltip("Probability per tick (0–1). Mushroom Girl's MutationChance bonus is added to this at runtime.")]
    public float ChancePerTick = 0.33f;
}

[CreateAssetMenu(fileName = "NewWorldEventData", menuName = "WaifuGarden/World Event Data")]
public class WorldEventData : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("Unique key. 'Day' is reserved for the default neutral event.")]
    public string EventID;
    public string EventName;

    [Header("Display")]
    public Sprite EventIcon;
    [TextArea(2, 4)]
    public string Description;

    [Header("Visuals")]
    [Tooltip("Tint applied to background/camera while active. White = no tint (use for Day).")]
    public Color BackgroundTint = Color.white;
    [Tooltip("VFX prefab instantiated while this event is active. Leave null for Day.")]
    public GameObject VisualEffectPrefab;

    [Header("Audio")]
    [Tooltip("Ambient loop played while active. Leave null for Day.")]
    public AudioClip AmbientSound;

    [Header("Modifier Chances")]
    [Tooltip("Modifiers this event may apply to growing plants each tick. Empty list for Day.")]
    public List<ModifierApplication> ModifierChances = new List<ModifierApplication>();
}
