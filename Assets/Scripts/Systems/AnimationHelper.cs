// =============================================================================
// AnimationHelper.cs  |  Scripts/Systems
// WaifuGarden — Phase 2
// Fully rewritten to use DOTween Pro on Image / RectTransform.
// All SpriteRenderer-based methods replaced with Image/RectTransform equivalents.
// CoroutineRunner kept only for PlayFloatingText (Phase 11 stub).
// Requires: DOTween Pro imported. If DOTween is not yet initialised in your
// project, add DOTween.Init() once in GameManager.Start().
// =============================================================================

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public static class AnimationHelper
{
    // CoroutineRunner retained for PlayFloatingText (Phase 11) and any
    // future coroutine-based utilities.
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

    // =========================================================================
    // Plant / Image Animations
    // =========================================================================

    /// <summary>
    /// Brief punch-scale pop on a RectTransform.
    /// Called on every plant growth stage transition.
    /// </summary>
    public static void PlayGrowthPop(RectTransform rt)
    {
        if (rt == null) return;
        rt.DOKill();
        rt.DOPunchScale(Vector3.one * 0.25f, 0.2f, 1, 0.5f).SetUpdate(true);
    }

    /// <summary>
    /// Continuously pulses the Image colour between white and glowColor.
    /// Kills any existing tween on this Image first.
    /// Called when EvolutionPending = true.
    /// </summary>
    public static void PlayGlowPulse(Image img, Color glowColor)
    {
        if (img == null) return;
        img.DOKill();
        img.DOColor(glowColor, 0.6f)
           .SetLoops(-1, LoopType.Yoyo)
           .SetEase(Ease.InOutSine);
    }

    /// <summary>Stops the glow pulse and resets the Image colour to white.</summary>
    public static void StopGlowPulse(Image img)
    {
        if (img == null) return;
        img.DOKill();
        img.color = Color.white;
    }

    /// <summary>
    /// Evolution Screen reveal: portrait fades in over 0.5s, text pops (scale 0 → 1).
    /// </summary>
    public static void PlayEvolutionReveal(Image portrait, TextMeshProUGUI text)
    {
        if (portrait != null)
        {
            portrait.DOKill();
            portrait.color = new Color(1f, 1f, 1f, 0f);
            portrait.DOFade(1f, 0.5f).SetEase(Ease.InOutSine);
        }
        if (text != null)
        {
            text.transform.DOKill();
            text.transform.localScale = Vector3.zero;
            text.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack).SetDelay(0.3f);
        }
    }

    /// <summary>
    /// Scales the plant Image down to zero over 0.2s, then fires onComplete.
    /// Called on plant harvest.
    /// </summary>
    public static void PlayHarvestPop(Image img, Action onComplete)
    {
        if (img == null) { onComplete?.Invoke(); return; }
        RectTransform rt = img.GetComponent<RectTransform>();
        if (rt == null)  { onComplete?.Invoke(); return; }
        rt.DOKill();
        rt.DOScale(Vector3.zero, 0.2f)
          .SetEase(Ease.InBack)
          .OnComplete(() =>
          {
              rt.localScale = Vector3.one; // reset for reuse
              onComplete?.Invoke();
          });
    }

    // =========================================================================
    // UI Animations
    // =========================================================================

    /// <summary>Applies a repeating golden glow pulse to a UI RectTransform's Image.</summary>
    public static void PlayUIGlow(RectTransform rt)
    {
        if (rt == null) return;
        Image img = rt.GetComponent<Image>();
        if (img == null) return;
        StopUIGlow(rt);
        img.DOColor(new Color(1f, 0.85f, 0f, 0.6f), 0.7f)
           .SetLoops(-1, LoopType.Yoyo)
           .SetEase(Ease.InOutSine);
    }

    /// <summary>Stops the UI glow and resets the Image colour to white.</summary>
    public static void StopUIGlow(RectTransform rt)
    {
        if (rt == null) return;
        Image img = rt.GetComponent<Image>();
        if (img == null) return;
        img.DOKill();
        img.color = Color.white;
    }

    /// <summary>General-purpose position shake for UI feedback.</summary>
    public static void PlayShake(RectTransform rt, float duration = 0.3f, float strength = 8f)
    {
        if (rt == null) return;
        rt.DOKill();
        rt.DOShakePosition(duration, strength, 20, 90, false, true);
    }

    /// <summary>Slides a panel in from off-screen. Direction: 1 = from bottom, -1 = from top.</summary>
    public static void PlayPanelSlideIn(RectTransform rt, float duration = 0.25f)
    {
        if (rt == null) return;
        rt.DOKill();
        Vector2 target = rt.anchoredPosition;
        rt.anchoredPosition = target + Vector2.down * 60f;
        rt.DOAnchorPos(target, duration).SetEase(Ease.OutCubic);
    }

    /// <summary>Fades a CanvasGroup in or out.</summary>
    public static void FadeCanvasGroup(CanvasGroup cg, float targetAlpha, float duration, Action onComplete = null)
    {
        if (cg == null) { onComplete?.Invoke(); return; }
        cg.DOKill();
        cg.DOFade(targetAlpha, duration).OnComplete(() => onComplete?.Invoke());
    }

    /// <summary>
    /// Placeholder for floating text. Phase 11 will instantiate a TMP prefab.
    /// </summary>
    public static void PlayFloatingText(string text, Vector3 worldPos)
    {
        Debug.Log($"[AnimationHelper] FloatingText (stub): \"{text}\" at {worldPos}");
    }
}
