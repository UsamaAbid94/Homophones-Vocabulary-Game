using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Springy button feel: squash on press, bounce back on release, optional idle "breathing" pulse
/// and a bouncy pop-in. Driven by a damped spring on unscaled time so it works while paused.
/// </summary>
[DisallowMultipleComponent]
public class UIButtonFX : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [Header("Press")]
    public float pressScale = 0.88f;

    [Header("Idle pulse")]
    public bool idlePulse = false;
    public float pulseAmount = 0.05f;
    public float pulseSpeed = 1.2f;

    [Header("Pop in when enabled")]
    public bool popInOnEnable = false;
    public float popInDelay = 0f;

    [Header("Spring")]
    public float stiffness = 320f;
    public float damping = 15f;

    private Vector3 baseScale;
    private bool captured;
    private Button button;
    private float current = 1f, velocity, target = 1f;
    private float delayTimer;
    private bool waitingToPop;

    private void Awake()
    {
        Capture();
        button = GetComponent<Button>();
    }

    private void Capture()
    {
        if (captured) return;
        baseScale = transform.localScale;
        captured = true;
    }

    private void OnEnable()
    {
        Capture();
        if (popInOnEnable)
        {
            PopIn(popInDelay);
        }
        else if (!waitingToPop)
        {
            current = target = 1f;
            velocity = 0f;
            Apply();
        }
    }

    private void OnDisable()
    {
        waitingToPop = false;
        if (captured) transform.localScale = baseScale;
    }

    /// <summary>Hide the button, then spring it in after <paramref name="delay"/> seconds.</summary>
    public void PopIn(float delay)
    {
        Capture();
        current = 0f;
        velocity = 0f;
        target = 0f;
        delayTimer = Mathf.Max(0f, delay);
        waitingToPop = true;
        Apply();
    }

    /// <summary>Kick the spring so the button bounces.</summary>
    public void Punch(float strength = 0.25f)
    {
        velocity += strength * 24f;
    }

    private bool Interactable => button == null || button.interactable;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!waitingToPop && Interactable) target = pressScale;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!waitingToPop) target = 1f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!waitingToPop) target = 1f;
    }

    private void Update()
    {
        float dt = Mathf.Min(Time.unscaledDeltaTime, 0.05f);

        if (waitingToPop)
        {
            delayTimer -= dt;
            if (delayTimer > 0f)
            {
                Apply();
                return;
            }
            waitingToPop = false;
            target = 1f;
        }

        velocity += ((target - current) * stiffness - velocity * damping) * dt;
        current += velocity * dt;
        Apply();
    }

    private void Apply()
    {
        float pulse = idlePulse && !waitingToPop
            ? 1f + pulseAmount * Mathf.Sin(Time.unscaledTime * pulseSpeed * Mathf.PI * 2f)
            : 1f;
        transform.localScale = baseScale * Mathf.Max(0f, current) * pulse;
    }
}
