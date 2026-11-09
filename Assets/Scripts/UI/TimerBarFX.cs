using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Changes the timer fill sprite green -> yellow -> pink as time runs out,
/// and makes the bar throb while time is low.
/// </summary>
public class TimerBarFX : MonoBehaviour
{
    public Image fill;
    public Sprite highSprite, midSprite, lowSprite;
    [Range(0f, 1f)] public float midThreshold = 0.5f;
    [Range(0f, 1f)] public float lowThreshold = 0.25f;
    public RectTransform pulseTarget;
    public float pulseAmount = 0.06f;
    public float pulseSpeed = 4f;

    private Vector3 pulseBaseScale;
    private bool wasLow;

    private void Awake()
    {
        if (fill == null) fill = GetComponent<Image>();
        if (pulseTarget != null) pulseBaseScale = pulseTarget.localScale;
    }

    private void Update()
    {
        if (fill == null) return;
        float f = fill.fillAmount;

        Sprite s = f > midThreshold ? highSprite : f > lowThreshold ? midSprite : lowSprite;
        if (s != null && fill.sprite != s) fill.sprite = s;

        if (pulseTarget == null) return;
        bool low = f <= lowThreshold && f > 0f;
        if (low)
        {
            float k = Mathf.Abs(Mathf.Sin(Time.unscaledTime * pulseSpeed * Mathf.PI));
            pulseTarget.localScale = pulseBaseScale * (1f + pulseAmount * k);
        }
        else if (wasLow)
        {
            pulseTarget.localScale = pulseBaseScale;
        }
        wasLow = low;
    }
}
