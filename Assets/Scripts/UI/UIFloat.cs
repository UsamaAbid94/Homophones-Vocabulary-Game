using UnityEngine;

/// <summary>Gentle idle bob and sway for titles and decorations.</summary>
[DisallowMultipleComponent]
public class UIFloat : MonoBehaviour
{
    public float bobAmount = 10f;
    public float bobSpeed = 0.6f;
    public float swayDegrees = 1.5f;
    public float swaySpeed = 0.4f;

    private RectTransform rt;
    private Vector2 basePos;
    private Vector3 baseEuler;
    private float phase;

    private void OnEnable()
    {
        rt = transform as RectTransform;
        if (rt != null) basePos = rt.anchoredPosition;
        baseEuler = transform.localEulerAngles;
        phase = Random.value * 10f;
    }

    private void OnDisable()
    {
        if (rt != null) rt.anchoredPosition = basePos;
        transform.localEulerAngles = baseEuler;
    }

    private void LateUpdate()
    {
        float t = Time.unscaledTime + phase;
        if (rt != null)
            rt.anchoredPosition = basePos + new Vector2(0f, Mathf.Sin(t * bobSpeed * Mathf.PI * 2f) * bobAmount);
        transform.localEulerAngles = baseEuler + new Vector3(0f, 0f, Mathf.Sin(t * swaySpeed * Mathf.PI * 2f) * swayDegrees);
    }
}
