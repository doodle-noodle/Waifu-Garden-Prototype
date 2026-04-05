// =============================================================================
// AnimationHelper.cs  |  Scripts/Systems
// WaifuGarden — Pre-Phase 2 Fixes
// Fix: GlowPulseCoroutine and UIGlowPulseCoroutine now check for null/destroyed
// targets at the top of each loop, preventing MissingReferenceException spam
// when a slot is removed mid-glow (e.g. Shovel used on a glowing plant).
// =============================================================================

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class AnimationHelper
{
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

    private static readonly Dictionary<SpriteRenderer, Coroutine> _activeGlows
        = new Dictionary<SpriteRenderer, Coroutine>();
    private static readonly Dictionary<RectTransform, Coroutine> _activeUIGlows
        = new Dictionary<RectTransform, Coroutine>();

    // =========================================================================
    // Plant / Sprite Animations
    // =========================================================================

    public static void PlayGrowthPop(SpriteRenderer sr)
    {
        if (sr == null) return;
        Runner.StartCoroutine(ScalePopCoroutine(sr.transform, 1.2f, 0.1f, 0.1f));
    }

    public static void PlayGlowPulse(SpriteRenderer sr, Color glowColor)
    {
        if (sr == null) return;
        StopGlowPulse(sr);
        Coroutine c = Runner.StartCoroutine(GlowPulseCoroutine(sr, glowColor, 0.6f));
        _activeGlows[sr] = c;
    }

    public static void StopGlowPulse(SpriteRenderer sr)
    {
        if (sr == null) return;
        if (_activeGlows.TryGetValue(sr, out Coroutine c))
        {
            if (c != null) Runner.StopCoroutine(c);
            _activeGlows.Remove(sr);
        }
        // Guard: sr may have been destroyed by the time we get here.
        if (sr != null) sr.color = Color.white;
    }

    public static void PlayEvolutionReveal(Image portrait, TextMeshProUGUI text)
    {
        if (portrait != null) Runner.StartCoroutine(FadeInImageCoroutine(portrait, 0.5f));
        if (text     != null) Runner.StartCoroutine(ScalePopCoroutine(text.transform, 1.1f, 0.2f, 0.1f));
    }

    public static void PlayHarvestPop(SpriteRenderer sr, Action onComplete)
    {
        if (sr == null) { onComplete?.Invoke(); return; }
        Runner.StartCoroutine(ScaleDownCoroutine(sr.transform, 0.2f, onComplete));
    }

    public static void PlayShake(RectTransform rt, float duration = 0.3f, float strength = 8f)
    {
        if (rt == null) return;
        Runner.StartCoroutine(ShakeCoroutine(rt, duration, strength));
    }

    public static void PlayFloatingText(string text, Vector3 worldPos)
    {
        // TODO (Phase 11): Instantiate floating text prefab, animate up + fade.
        Debug.Log($"[AnimationHelper] FloatingText: \"{text}\" at {worldPos}");
    }

    // =========================================================================
    // UI Glow (TutorialManager)
    // =========================================================================

    public static void PlayUIGlow(RectTransform rt)
    {
        if (rt == null) return;
        Image img = rt.GetComponent<Image>();
        if (img == null) return;
        StopUIGlow(rt);
        Coroutine c = Runner.StartCoroutine(UIGlowPulseCoroutine(img, new Color(1f, 0.85f, 0f, 0.5f), 0.7f));
        _activeUIGlows[rt] = c;
    }

    public static void StopUIGlow(RectTransform rt)
    {
        if (rt == null) return;
        if (_activeUIGlows.TryGetValue(rt, out Coroutine c))
        {
            if (c != null) Runner.StopCoroutine(c);
            _activeUIGlows.Remove(rt);
        }
        if (rt == null) return;
        Image img = rt.GetComponent<Image>();
        if (img != null) img.color = Color.white;
    }

    // =========================================================================
    // Coroutine implementations
    // =========================================================================

    private static IEnumerator ScalePopCoroutine(Transform t, float peak, float upDur, float downDur)
    {
        if (t == null) yield break;
        Vector3 original = t.localScale;
        for (float e = 0; e < upDur;   e += Time.deltaTime) { if (t == null) yield break; t.localScale = Vector3.Lerp(original, original * peak, e / upDur);   yield return null; }
        for (float e = 0; e < downDur; e += Time.deltaTime) { if (t == null) yield break; t.localScale = Vector3.Lerp(original * peak, original, e / downDur); yield return null; }
        if (t != null) t.localScale = original;
    }

    private static IEnumerator ScaleDownCoroutine(Transform t, float duration, Action onComplete)
    {
        if (t == null) { onComplete?.Invoke(); yield break; }
        Vector3 original = t.localScale;
        for (float e = 0; e < duration; e += Time.deltaTime)
        { if (t == null) break; t.localScale = Vector3.Lerp(original, Vector3.zero, e / duration); yield return null; }
        if (t != null) t.localScale = Vector3.zero;
        onComplete?.Invoke();
    }

    private static IEnumerator GlowPulseCoroutine(SpriteRenderer sr, Color glowColor, float period)
    {
        float half = period * 0.5f;
        while (sr != null) // ← null guard: stops cleanly if the object is destroyed
        {
            for (float e = 0; e < half; e += Time.deltaTime)
            { if (sr == null) yield break; sr.color = Color.Lerp(Color.white, glowColor, e / half); yield return null; }
            for (float e = 0; e < half; e += Time.deltaTime)
            { if (sr == null) yield break; sr.color = Color.Lerp(glowColor, Color.white, e / half); yield return null; }
        }
    }

    private static IEnumerator UIGlowPulseCoroutine(Image img, Color glowColor, float period)
    {
        if (img == null) yield break;
        Color baseColor = img.color;
        float half = period * 0.5f;
        while (img != null) // ← null guard: stops cleanly if the UI element is destroyed
        {
            for (float e = 0; e < half; e += Time.deltaTime)
            { if (img == null) yield break; img.color = Color.Lerp(baseColor, glowColor, e / half); yield return null; }
            for (float e = 0; e < half; e += Time.deltaTime)
            { if (img == null) yield break; img.color = Color.Lerp(glowColor, baseColor, e / half); yield return null; }
        }
    }

    private static IEnumerator FadeInImageCoroutine(Image img, float duration)
    {
        if (img == null) yield break;
        Color c = img.color; c.a = 0f; img.color = c;
        for (float e = 0; e < duration; e += Time.deltaTime)
        { if (img == null) yield break; c.a = e / duration; img.color = c; yield return null; }
        if (img != null) { c.a = 1f; img.color = c; }
    }

    private static IEnumerator ShakeCoroutine(RectTransform rt, float duration, float strength)
    {
        if (rt == null) yield break;
        Vector3 origin = rt.localPosition;
        for (float e = 0; e < duration; e += Time.deltaTime)
        {
            if (rt == null) yield break;
            rt.localPosition = origin + (Vector3)(UnityEngine.Random.insideUnitCircle * strength * (1f - e / duration));
            yield return null;
        }
        if (rt != null) rt.localPosition = origin;
    }
}
