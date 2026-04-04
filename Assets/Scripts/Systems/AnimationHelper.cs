// =============================================================================
// AnimationHelper.cs  |  Scripts/Systems
// WaifuGarden — Phase 0
// Static utility class for all reusable animations.
// Coroutine-based implementations now. DOTween variants wired in Phase 11.
//
// KEY RULE: All methods accept their target component generically.
// No method references a specific sprite, prefab, plant, or character.
// Swapping any sprite requires zero animation code changes.
// =============================================================================

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class AnimationHelper
{
    // -------------------------------------------------------------------------
    // Lazy coroutine runner — created once, persists for the session.
    // -------------------------------------------------------------------------

    private static CoroutineRunner _runner;

    private static CoroutineRunner Runner
    {
        get
        {
            if (_runner == null)
            {
                var go = new GameObject("[AnimationHelperRunner]");
                UnityEngine.Object.DontDestroyOnLoad(go);
                _runner = go.AddComponent<CoroutineRunner>();
            }
            return _runner;
        }
    }

    // Tracks active glow coroutines so they can be cancelled cleanly.
    private static readonly Dictionary<SpriteRenderer, Coroutine> _activeGlows
        = new Dictionary<SpriteRenderer, Coroutine>();
    private static readonly Dictionary<RectTransform, Coroutine> _activeUIGlows
        = new Dictionary<RectTransform, Coroutine>();

    // =========================================================================
    // Plant / Sprite Animations
    // =========================================================================

    /// <summary>
    /// Scale-pop: 100% → 120% over 0.1s, back to 100% over 0.1s.
    /// Called on every plant growth stage transition.
    /// </summary>
    public static void PlayGrowthPop(SpriteRenderer sr)
    {
        if (sr == null) return;
        Runner.StartCoroutine(ScalePopCoroutine(sr.transform, 1.2f, 0.1f, 0.1f));
    }

    /// <summary>
    /// Continuously pulses sprite colour between white and glowColor.
    /// Any existing glow on this SpriteRenderer is cancelled first.
    /// Called when EvolutionPending = true.
    /// </summary>
    public static void PlayGlowPulse(SpriteRenderer sr, Color glowColor)
    {
        if (sr == null) return;
        StopGlowPulse(sr);
        Coroutine c = Runner.StartCoroutine(GlowPulseCoroutine(sr, glowColor, 0.6f));
        _activeGlows[sr] = c;
    }

    /// <summary>Stops the glow pulse and resets the sprite to white.</summary>
    public static void StopGlowPulse(SpriteRenderer sr)
    {
        if (sr == null) return;
        if (_activeGlows.TryGetValue(sr, out Coroutine c))
        {
            if (c != null) Runner.StopCoroutine(c);
            _activeGlows.Remove(sr);
        }
        sr.color = Color.white;
    }

    /// <summary>
    /// Evolution Screen reveal: portrait fades in over 0.5s, text pops (0 → 110% → 100%).
    /// </summary>
    public static void PlayEvolutionReveal(Image portrait, TextMeshProUGUI text)
    {
        if (portrait != null)
            Runner.StartCoroutine(FadeInImageCoroutine(portrait, 0.5f));
        if (text != null)
            Runner.StartCoroutine(ScalePopCoroutine(text.transform, 1.1f, 0.2f, 0.1f));
    }

    /// <summary>
    /// Scales the sprite down to zero over 0.2s, then fires onComplete.
    /// Called on plant harvest — onComplete removes the slot's PlantInstance.
    /// </summary>
    public static void PlayHarvestPop(SpriteRenderer sr, Action onComplete)
    {
        if (sr == null) { onComplete?.Invoke(); return; }
        Runner.StartCoroutine(ScaleDownCoroutine(sr.transform, 0.2f, onComplete));
    }

    /// <summary>General-purpose shake for UI feedback (wrong action, etc.).</summary>
    public static void PlayShake(RectTransform rt, float duration = 0.3f, float strength = 8f)
    {
        if (rt == null) return;
        Runner.StartCoroutine(ShakeCoroutine(rt, duration, strength));
    }

    /// <summary>
    /// Logs a placeholder for the floating text system.
    /// Phase 11 will instantiate a TextMeshPro prefab, float it upward, and fade it out.
    /// </summary>
    public static void PlayFloatingText(string text, Vector3 worldPos)
    {
        // TODO (Phase 11): Instantiate floating text prefab, animate up + fade.
        Debug.Log($"[AnimationHelper] FloatingText: \"{text}\" at {worldPos}");
    }

    // =========================================================================
    // UI Glow (used by TutorialManager to highlight buttons)
    // =========================================================================

    /// <summary>Applies a repeating golden glow pulse to a UI Image component.</summary>
    public static void PlayUIGlow(RectTransform rt)
    {
        if (rt == null) return;
        Image img = rt.GetComponent<Image>();
        if (img == null) return;

        StopUIGlow(rt);
        Coroutine c = Runner.StartCoroutine(UIGlowPulseCoroutine(img, new Color(1f, 0.85f, 0f, 0.5f), 0.7f));
        _activeUIGlows[rt] = c;
    }

    /// <summary>Stops the UI glow pulse and resets the Image colour to white.</summary>
    public static void StopUIGlow(RectTransform rt)
    {
        if (rt == null) return;
        if (_activeUIGlows.TryGetValue(rt, out Coroutine c))
        {
            if (c != null) Runner.StopCoroutine(c);
            _activeUIGlows.Remove(rt);
        }
        Image img = rt.GetComponent<Image>();
        if (img != null) img.color = Color.white;
    }

    // =========================================================================
    // Private coroutine implementations
    // =========================================================================

    private static IEnumerator ScalePopCoroutine(
        Transform t, float peak, float upDuration, float downDuration)
    {
        Vector3 original = t.localScale;
        for (float e = 0; e < upDuration; e += Time.deltaTime)
        {
            t.localScale = Vector3.Lerp(original, original * peak, e / upDuration);
            yield return null;
        }
        for (float e = 0; e < downDuration; e += Time.deltaTime)
        {
            t.localScale = Vector3.Lerp(original * peak, original, e / downDuration);
            yield return null;
        }
        t.localScale = original;
    }

    private static IEnumerator ScaleDownCoroutine(Transform t, float duration, Action onComplete)
    {
        Vector3 original = t.localScale;
        for (float e = 0; e < duration; e += Time.deltaTime)
        {
            t.localScale = Vector3.Lerp(original, Vector3.zero, e / duration);
            yield return null;
        }
        t.localScale = Vector3.zero;
        onComplete?.Invoke();
    }

    private static IEnumerator GlowPulseCoroutine(SpriteRenderer sr, Color glowColor, float period)
    {
        float half = period * 0.5f;
        while (true)
        {
            for (float e = 0; e < half; e += Time.deltaTime)
            { sr.color = Color.Lerp(Color.white, glowColor, e / half); yield return null; }
            for (float e = 0; e < half; e += Time.deltaTime)
            { sr.color = Color.Lerp(glowColor, Color.white, e / half); yield return null; }
        }
    }

    private static IEnumerator UIGlowPulseCoroutine(Image img, Color glowColor, float period)
    {
        Color baseColor = img.color;
        float half = period * 0.5f;
        while (true)
        {
            for (float e = 0; e < half; e += Time.deltaTime)
            { img.color = Color.Lerp(baseColor, glowColor, e / half); yield return null; }
            for (float e = 0; e < half; e += Time.deltaTime)
            { img.color = Color.Lerp(glowColor, baseColor, e / half); yield return null; }
        }
    }

    private static IEnumerator FadeInImageCoroutine(Image img, float duration)
    {
        Color c = img.color; c.a = 0f; img.color = c;
        for (float e = 0; e < duration; e += Time.deltaTime)
        { c.a = e / duration; img.color = c; yield return null; }
        c.a = 1f; img.color = c;
    }

    private static IEnumerator ShakeCoroutine(RectTransform rt, float duration, float strength)
    {
        Vector3 origin = rt.localPosition;
        for (float e = 0; e < duration; e += Time.deltaTime)
        {
            float decay = 1f - (e / duration);
            rt.localPosition = origin + (Vector3)(UnityEngine.Random.insideUnitCircle * strength * decay);
            yield return null;
        }
        rt.localPosition = origin;
    }
}
