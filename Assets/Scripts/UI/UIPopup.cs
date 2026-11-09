using System.Collections;
using UnityEngine;

/// <summary>
/// Put on a popup's full-screen root. When the panel is activated, the dim overlay fades in,
/// the card bounces up from half size and every UIButtonFX inside pops in one after another.
/// </summary>
[DisallowMultipleComponent]
public class UIPopup : MonoBehaviour
{
    public RectTransform card;
    public float overlayFadeTime = 0.2f;
    public float cardTime = 0.45f;
    public float staggerStart = 0.18f;
    public float staggerStep = 0.08f;

    private CanvasGroup group;
    private Vector3 cardBaseScale;
    private bool captured;

    private void Awake()
    {
        Capture();
    }

    private void Capture()
    {
        if (captured) return;
        group = GetComponent<CanvasGroup>();
        if (group == null) group = gameObject.AddComponent<CanvasGroup>();
        if (card != null) cardBaseScale = card.localScale;
        captured = true;
    }

    private void OnEnable()
    {
        Capture();
        SoundManager.Popup();

        LeanTween.cancel(gameObject);
        group.alpha = 0f;
        UIFx.Tween(gameObject, 0f, 1f, overlayFadeTime, v => group.alpha = v);

        if (card != null)
        {
            UIFx.PopFrom(card, cardBaseScale, 0.5f, cardTime);
        }

        // Children run their own OnEnable after this one, so stagger them a frame later.
        // The overlay is still at alpha 0 on that frame, so nothing flickers.
        StartCoroutine(StaggerButtons());
    }

    private void OnDisable()
    {
        if (card != null)
        {
            LeanTween.cancel(card.gameObject);
            card.localScale = cardBaseScale;
        }
        if (group != null) group.alpha = 1f;
    }

    private IEnumerator StaggerButtons()
    {
        yield return null;
        if (card == null) yield break;
        int i = 0;
        foreach (var fx in card.GetComponentsInChildren<UIButtonFX>(false))
        {
            fx.PopIn(staggerStart + staggerStep * i++);
        }
    }
}
