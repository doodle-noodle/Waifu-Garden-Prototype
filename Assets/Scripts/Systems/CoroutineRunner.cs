// =============================================================================
// CoroutineRunner.cs  |  Scripts/Systems
// WaifuGarden — Phase 0
// Minimal MonoBehaviour that gives the static AnimationHelper class a
// coroutine execution context. AnimationHelper creates one instance lazily.
// Do NOT manually add this component to any GameObject.
// =============================================================================

using UnityEngine;

/// <summary>
/// Invisible scene object used exclusively by AnimationHelper to run coroutines.
/// Created automatically at runtime — do not add this component manually.
/// </summary>
public class CoroutineRunner : MonoBehaviour { }
