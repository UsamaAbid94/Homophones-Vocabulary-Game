using UnityEngine;

/// <summary>
/// One-shot entrance animation that plays every time the object is enabled.
/// Don't combine with another component that drives the same property (scale or position).
/// </summary>
[DisallowMultipleComponent]
public class UIEntrance : MonoBehaviour
{
    public enum Mode
    {
        Pop,          // scale 0 -> base with overshoot
        FadeSlideUp,  // fade in while sliding up into place
        DropIn,       // fall from above with a bounce
        Fade          // alpha only
    }

    public Mode mode = Mode.Pop;
    public float delay = 0f;
    public float duration = 0.4f;
    public float distance = 60f;
    [Tooltip("Pop mode only: spin in from this many degrees.")]
    public float spinFrom = 0f;

    private RectTransform rt;
    private CanvasGroup group;
    private Vector3 baseScale;
    private Vector2 basePos;
    private Vector3 baseEuler;
    private bool captured;

    private void Awake()
    {
        Capture();
    }

    private void Capture()
    {
        if (captured) return;
        rt = transform as RectTransform;
        baseScale = transform.localScale;
        baseEuler = transform.localEulerAngles;
        if (rt != null) basePos = rt.anchoredPosition;
        if (mode == Mode.FadeSlideUp || mode == Mode.Fade)
        {
            group = GetComponent<CanvasGroup>();
            if (group == null) group = gameObject.AddComponent<CanvasGroup>();
        }
        captured = true;
    }

    private void OnEnable()
    {
        Capture();
        Play();
    }

    private void OnDisable()
    {
        LeanTween.cancel(gameObject);
        Restore();
    }

    private void Restore()
    {
        if (!captured) return;
        transform.localScale = baseScale;
        transform.localEulerAngles = baseEuler;
        if (rt != null) rt.anchoredPosition = basePos;
        if (group != null) group.alpha = 1f;
    }

    public void Play()
    {
        LeanTween.cancel(gameObject);
        Restore();

        switch (mode)
        {
            case Mode.Pop:
                transform.localScale = Vector3.zero;
                UIFx.Tween(gameObject, 0f, 1f, duration, v =>
                {
                    transform.localScale = baseScale * v;
                    if (spinFrom != 0f)
                        transform.localEulerAngles = baseEuler + new Vector3(0f, 0f, spinFrom * (1f - v));
                }, LeanTweenType.easeOutBack, delay);
                break;

            case Mode.FadeSlideUp:
                group.alpha = 0f;
                if (rt != null) rt.anchoredPosition = basePos - new Vector2(0f, distance);
                UIFx.Tween(gameObject, 0f, 1f, duration, v =>
                {
                    group.alpha = v;
                    if (rt != null) rt.anchoredPosition = basePos - new Vector2(0f, distance * (1f - v));
                }, LeanTweenType.easeOutCubic, delay);
                break;

            case Mode.DropIn:
                if (rt != null) rt.anchoredPosition = basePos + new Vector2(0f, distance);
                UIFx.Tween(gameObject, 0f, 1f, duration, v =>
                {
                    if (rt != null) rt.anchoredPosition = basePos + new Vector2(0f, distance * (1f - v));
                }, LeanTweenType.easeOutBounce, delay);
                break;

            case Mode.Fade:
                group.alpha = 0f;
                UIFx.Tween(gameObject, 0f, 1f, duration, v => group.alpha = v, LeanTweenType.easeOutQuad, delay);
                break;
        }
    }
}
