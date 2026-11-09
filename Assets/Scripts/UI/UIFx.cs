using System;
using UnityEngine;

/// <summary>
/// Small LeanTween helpers shared by the UI animation components.
/// Every tween ignores Time.timeScale so popups still animate while the game is paused.
/// </summary>
public static class UIFx
{
    public static LTDescr Tween(GameObject target, float from, float to, float time, Action<float> onUpdate,
        LeanTweenType ease = LeanTweenType.easeOutQuad, float delay = 0f)
    {
        return LeanTween.value(target, from, to, time)
            .setOnUpdate(onUpdate)
            .setEase(ease)
            .setDelay(delay)
            .setIgnoreTimeScale(true);
    }

    /// <summary>Quick scale "boing" around a known base scale.</summary>
    public static LTDescr Punch(Transform t, Vector3 baseScale, float amount = 0.15f, float time = 0.35f)
    {
        LeanTween.cancel(t.gameObject);
        return Tween(t.gameObject, 0f, 1f, time, v =>
        {
            float wobble = Mathf.Sin(v * Mathf.PI * 2f) * (1f - v) * 0.4f;
            t.localScale = baseScale * (1f + amount * (Mathf.Sin(v * Mathf.PI) + wobble));
        }).setOnComplete(() => t.localScale = baseScale);
    }

    /// <summary>Scale from a fraction of the base scale up to the base, with an overshoot.</summary>
    public static LTDescr PopFrom(Transform t, Vector3 baseScale, float fromFactor = 0f, float time = 0.4f, float delay = 0f)
    {
        LeanTween.cancel(t.gameObject);
        t.localScale = baseScale * fromFactor;
        return Tween(t.gameObject, fromFactor, 1f, time, v => t.localScale = baseScale * v,
            LeanTweenType.easeOutBack, delay);
    }
}
