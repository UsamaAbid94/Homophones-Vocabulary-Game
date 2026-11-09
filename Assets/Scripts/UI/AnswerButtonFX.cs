using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Visual states for an answer button: normal, hinted (green), correct (green + bounce)
/// and wrong (red flash + shake). Called by LevelManager.
/// </summary>
[RequireComponent(typeof(Image))]
public class AnswerButtonFX : MonoBehaviour
{
    public Sprite normalSprite;
    public Sprite hintSprite;
    public Sprite correctSprite;
    public Color wrongTint = new Color(1f, 0.45f, 0.45f, 1f);
    public float shakeDistance = 18f;

    private Image image;
    private RectTransform rt;
    private UIButtonFX buttonFx;
    private Vector2 basePos;
    private bool hinted;
    private bool captured;

    private void Awake()
    {
        Capture();
    }

    private void Capture()
    {
        if (captured) return;
        image = GetComponent<Image>();
        rt = (RectTransform)transform;
        buttonFx = GetComponent<UIButtonFX>();
        basePos = rt.anchoredPosition;
        if (normalSprite == null) normalSprite = image.sprite;
        captured = true;
    }

    public void ResetVisual()
    {
        Capture();
        hinted = false;
        LeanTween.cancel(gameObject);
        rt.anchoredPosition = basePos;
        image.sprite = normalSprite;
        image.color = Color.white;
    }

    public void ShowHint()
    {
        Capture();
        hinted = true;
        if (hintSprite != null) image.sprite = hintSprite;
        else image.color = Color.green;
        if (buttonFx != null) buttonFx.Punch(0.3f);
    }

    public void PlayCorrect()
    {
        Capture();
        if (correctSprite != null) image.sprite = correctSprite;
        image.color = Color.white;
        SoundManager.Correct();
        if (buttonFx != null) buttonFx.Punch(0.35f);
    }

    public void PlayWrong()
    {
        Capture();
        LeanTween.cancel(gameObject);
        Color restColor = Color.white;
        image.sprite = hinted && hintSprite != null ? hintSprite : normalSprite;
        image.color = wrongTint;
        UIFx.Tween(gameObject, 0f, 1f, 0.45f, v =>
        {
            float offset = Mathf.Sin(v * Mathf.PI * 7f) * shakeDistance * (1f - v);
            rt.anchoredPosition = basePos + new Vector2(offset, 0f);
            image.color = Color.Lerp(wrongTint, restColor, v);
        }, LeanTweenType.linear).setOnComplete(() =>
        {
            rt.anchoredPosition = basePos;
            image.color = restColor;
        });
    }

    public void PopIn(float delay)
    {
        Capture();
        if (buttonFx != null) buttonFx.PopIn(delay);
    }
}
