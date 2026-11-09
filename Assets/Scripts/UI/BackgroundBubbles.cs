using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Spawns soft shapes that drift upward, sway and spin behind the UI.
/// Put on a full-screen RectTransform that sits just above the background image.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class BackgroundBubbles : MonoBehaviour
{
    public Sprite[] sprites;
    public int count = 14;
    public Vector2 sizeRange = new Vector2(30f, 110f);
    public Vector2 speedRange = new Vector2(20f, 60f);
    [Range(0f, 1f)] public float maxAlpha = 0.28f;

    private class Bubble
    {
        public RectTransform rt;
        public float speed, sway, phase, spin, baseX, size;
    }

    private readonly List<Bubble> bubbles = new List<Bubble>();
    private RectTransform area;

    private void Start()
    {
        area = (RectTransform)transform;
        if (sprites == null || sprites.Length == 0) return;

        for (int i = 0; i < count; i++)
        {
            var go = new GameObject("Bubble", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(transform, false);
            var img = go.GetComponent<Image>();
            img.sprite = sprites[Random.Range(0, sprites.Length)];
            img.preserveAspect = true;
            img.raycastTarget = false;
            img.color = new Color(1f, 1f, 1f, Random.Range(maxAlpha * 0.4f, maxAlpha));

            var b = new Bubble
            {
                rt = (RectTransform)go.transform,
                size = Random.Range(sizeRange.x, sizeRange.y),
                speed = Random.Range(speedRange.x, speedRange.y),
                sway = Random.Range(10f, 40f),
                phase = Random.value * 10f,
                spin = Random.Range(-25f, 25f)
            };
            b.rt.sizeDelta = new Vector2(b.size, b.size);
            Respawn(b, Random.Range(0f, 1f));
            bubbles.Add(b);
        }
    }

    private void Respawn(Bubble b, float heightFraction)
    {
        Rect r = area.rect;
        b.baseX = Random.Range(r.xMin, r.xMax);
        float y = Mathf.Lerp(r.yMin - b.size, r.yMax, heightFraction);
        b.rt.anchoredPosition = new Vector2(b.baseX, y);
    }

    private void Update()
    {
        if (area == null) return;
        float dt = Time.unscaledDeltaTime;
        float t = Time.unscaledTime;
        float top = area.rect.yMax + 120f;

        foreach (var b in bubbles)
        {
            Vector2 p = b.rt.anchoredPosition;
            p.y += b.speed * dt;
            p.x = b.baseX + Mathf.Sin(t * 0.5f + b.phase) * b.sway;
            b.rt.anchoredPosition = p;
            b.rt.Rotate(0f, 0f, b.spin * dt);
            if (p.y > top) Respawn(b, 0f);
        }
    }
}
