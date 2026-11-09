using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Result star: spins and pops in when the panel opens, then gives a bounce
/// the moment its slider fills completely.
/// </summary>
[RequireComponent(typeof(Slider))]
public class StarFX : MonoBehaviour
{
    public float popDelay = 0f;
    public float popTime = 0.5f;

    private Slider slider;
    private Vector3 baseScale;
    private Vector3 baseEuler;
    private bool captured;
    private bool wasFull;

    private void Awake()
    {
        slider = GetComponent<Slider>();
        baseScale = transform.localScale;
        baseEuler = transform.localEulerAngles;
        captured = true;
    }

    private void OnEnable()
    {
        if (!captured) Awake();
        wasFull = false;
        LeanTween.cancel(gameObject);
        transform.localScale = Vector3.zero;
        UIFx.Tween(gameObject, 0f, 1f, popTime, v =>
        {
            transform.localScale = baseScale * v;
            transform.localEulerAngles = baseEuler + new Vector3(0f, 0f, -120f * (1f - v));
        }, LeanTweenType.easeOutBack, popDelay);
    }

    private void OnDisable()
    {
        LeanTween.cancel(gameObject);
        transform.localScale = baseScale;
        transform.localEulerAngles = baseEuler;
    }

    private void Update()
    {
        if (wasFull || slider == null) return;
        if (slider.value >= slider.maxValue - 0.05f)
        {
            wasFull = true;
            transform.localEulerAngles = baseEuler;
            UIFx.Punch(transform, baseScale, 0.3f, 0.45f);
        }
    }
}
