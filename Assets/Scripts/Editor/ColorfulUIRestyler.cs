using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// One-click reskin of the Menu and Game scenes with the Colorful_UI pack and the UI animation components.
/// Every element is laid out explicitly on a 1080x1920 portrait reference at scale 1.
/// Menu: Tools > Homophones > Apply Colorful UI. Safe to run more than once.
/// </summary>
public static class ColorfulUIRestyler
{
    private const string Pack = "Assets/Colorful_UI/colorful/";
    private const string GeneratedDir = "Assets/UI/ColorfulTheme";

    private static readonly Color Ink = new Color32(0x4A, 0x1F, 0x8C, 0xFF);          // dark purple text
    private static readonly Color SoftInk = new Color32(0x7A, 0x5C, 0xB0, 0xFF);      // secondary text
    private static readonly Color OutlineColor = new Color(0.20f, 0.05f, 0.38f, 0.55f);
    private static readonly Color OverlayColor = new Color(0.10f, 0.03f, 0.22f, 0.80f);
    private static readonly Color StarEmpty = new Color(0.45f, 0.35f, 0.65f, 0.25f);

    private static readonly Vector2 Center = new Vector2(0.5f, 0.5f);
    private static readonly Vector2 Top = new Vector2(0.5f, 1f);
    private static readonly Vector2 Bottom = new Vector2(0.5f, 0f);
    private static readonly Vector2 TopLeft = new Vector2(0f, 1f);
    private static readonly Vector2 TopRight = new Vector2(1f, 1f);
    private static readonly Vector2 Left = new Vector2(0f, 0.5f);
    private static readonly Vector2 Right = new Vector2(1f, 0.5f);

    private static Font font;

    [MenuItem("Tools/Homophones/Apply Colorful UI")]
    public static void Apply()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorUtility.DisplayDialog("Colorful UI", "Exit Play Mode first.", "OK");
            return;
        }
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        Run();
        EditorUtility.DisplayDialog("Colorful UI", "Menu and Game scenes were restyled and saved.", "Nice");
    }

    /// <summary>Same as the menu item, without dialogs (used by automation).</summary>
    public static void ApplySilently()
    {
        Scene active = SceneManager.GetActiveScene();
        if (active.isDirty) EditorSceneManager.SaveScene(active);
        Run();
    }

    private static void Run()
    {
        string previousScene = SceneManager.GetActiveScene().path;

        PrepareSprites();
        font = AssetDatabase.LoadAssetAtPath<Font>(Pack + "font/Digitalt.ttf");
        if (font == null) Debug.LogWarning("[ColorfulUI] Digitalt font not found, keeping existing fonts.");

        Sprite menuBg = MakeGradient("bg_menu.png", new Color32(0x5E, 0x35, 0xD6, 0xFF), new Color32(0xC8, 0x5A, 0xD8, 0xFF));
        Sprite gameBg = MakeGradient("bg_game.png", new Color32(0x4E, 0x34, 0xC9, 0xFF), new Color32(0x3A, 0x9C, 0xE8, 0xFF));

        Restyle("Assets/Scenes/Menu.unity", canvas => RestyleMenu(canvas, menuBg));
        Restyle("Assets/Scenes/Game.unity", canvas => RestyleGame(canvas, gameBg));

        if (!string.IsNullOrEmpty(previousScene)) EditorSceneManager.OpenScene(previousScene);
    }

    // ------------------------------------------------------------------ scenes

    private static void Restyle(string scenePath, System.Action<Transform> body)
    {
        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        GameObject canvas = scene.GetRootGameObjects().FirstOrDefault(g => g.name == "Canvas");
        if (canvas == null)
        {
            Debug.LogError("[ColorfulUI] No root 'Canvas' in " + scenePath);
            return;
        }
        PortraitScaler(canvas);
        body(canvas.transform);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[ColorfulUI] Restyled " + scenePath);
    }

    private static void PortraitScaler(GameObject canvas)
    {
        var scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler == null) return;
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        Dirty(scaler);
    }

    // ================================================================== MENU

    private static void RestyleMenu(Transform c, Sprite bg)
    {
        ApplyFontEverywhere(c);
        Background(c, bg);

        // Title + tagline
        Transform title = Find(c, "GameTitle");
        At(title, Top, new Vector2(0f, -330f), new Vector2(980f, 380f));
        Txt(title, 132, Color.white, outline: true, outlineSize: 5f);
        if (title != null) title.GetComponent<Text>().lineSpacing = 0.85f;
        AddShadow(title);
        Entrance(title, UIEntrance.Mode.Pop, 0.1f, 0.6f);
        var titleFloat = GetOrAdd<UIFloat>(title);
        titleFloat.bobAmount = 12f;
        titleFloat.swayDegrees = 1.2f;
        Dirty(titleFloat);

        Transform tagline = Child(c, "Tagline", typeof(Text));
        tagline.SetSiblingIndex(title.GetSiblingIndex() + 1);
        At(tagline, Top, new Vector2(0f, -560f), new Vector2(900f, 70f));
        Txt(tagline, 46, new Color(1f, 1f, 1f, 0.9f), outline: false, text: "Pick the word that fits!");
        Entrance(tagline, UIEntrance.Mode.FadeSlideUp, 0.45f, 0.5f, 30f);

        // Showcase: two homophone cards gently floating in the middle
        Transform show = Child(c, "Showcase");
        show.SetSiblingIndex(tagline.GetSiblingIndex() + 1);
        At(show, Center, new Vector2(0f, 40f), new Vector2(900f, 420f));
        WordCard(show, "CardA", "WRITE", new Vector2(-185f, 55f), 7f, Sp("gameUI/card_bg.png"), new Color32(0x3E, 0xA8, 0xF5, 0xFF), 0.55f);
        WordCard(show, "CardB", "RIGHT", new Vector2(185f, -55f), -7f, Sp("gameUI/card_bg.png"), new Color32(0xF0, 0x55, 0xC0, 0xFF), 0.7f);
        Transform spark = Child(show, "Sparkle", typeof(Image));
        At(spark, Center, new Vector2(0f, 10f), new Vector2(110f, 110f));
        SetImage(spark, Sp("icons/star_big.png"), preserveAspect: true);
        spark.GetComponent<Image>().raycastTarget = false;
        var sparkPop = Entrance(spark, UIEntrance.Mode.Pop, 0.9f, 0.5f);
        if (sparkPop != null) sparkPop.spinFrom = -200f;

        // Main buttons (sprites already contain the label)
        Transform play = Find(c, "PlayButton");
        SetImage(play, Sp("button/play.png"));
        At(play, Bottom, new Vector2(0f, 500f), new Vector2(500f, 214f));
        var playFx = ButtonFx(play);
        playFx.idlePulse = true;
        playFx.pulseAmount = 0.035f;
        playFx.popInOnEnable = true;
        playFx.popInDelay = 0.4f;

        Transform settings = Find(c, "SettingButton");
        SetImage(settings, Sp("button/settings.png"));
        At(settings, Bottom, new Vector2(0f, 290f), new Vector2(360f, 154f));
        var settingsFx = ButtonFx(settings);
        settingsFx.idlePulse = false;
        settingsFx.popInOnEnable = true;
        settingsFx.popInDelay = 0.55f;

        // Settings popup
        Transform card = Find(c, "SettingsPanel/POP");
        StylePopup(Find(c, "SettingsPanel"), card, "Title", Sp("gameUI/purpleBg.png"), new Vector2(860f, 520f), 480f);

        Transform close = Find(c, "SettingsPanel/POP/CancelButton");
        SetImage(close, Sp("icons/close_btn.png"), preserveAspect: true);
        At(close, TopRight, new Vector2(-20f, -20f), new Vector2(100f, 108f));
        Hide(Find(c, "SettingsPanel/POP/CancelButton/CrossImg"));

        SettingRow(card, "MusicLabel", "MusicSlider", "MUSIC", 40f, Sp("slider/yellowBarFill.png"), Sp("slider/Ellipse 18.png"));
        SettingRow(card, "SoundLabel", "SoundSlider", "SOUND", -100f, Sp("slider/greenBarFill.png"), Sp("slider/EllipseGreen.png"));

        SetImage(Find(c, "SettingsPanel/POP/RemoveAdsButton"), Sp("button/emptyButtons/yellowButton.png"));
        At(Find(c, "SettingsPanel/POP/RemoveAdsButton"), Center, new Vector2(150f, -220f), new Vector2(240f, 103f));
        Stretch(Find(c, "SettingsPanel/POP/RemoveAdsButton/Text"), 10f, 14f, 10f, 10f);
        Txt(Find(c, "SettingsPanel/POP/RemoveAdsButton/Text"), 44, Color.white, outline: true);
        At(Find(c, "SettingsPanel/POP/RemoveAds"), Center, new Vector2(-130f, -220f), new Vector2(300f, 70f));
        Txt(Find(c, "SettingsPanel/POP/RemoveAds"), 44, Ink, outline: false);

        AddButtonFxToAll(c);
        Bubbles(c);
    }

    private static void WordCard(Transform parent, string name, string word, Vector2 pos, float tilt, Sprite sprite, Color wordColor, float delay)
    {
        Transform card = Child(parent, name, typeof(Image));
        At(card, Center, pos, new Vector2(360f, 190f), tilt);
        SetSliced(card, sprite);
        card.GetComponent<Image>().raycastTarget = false;

        Transform label = Child(card, "Word", typeof(Text));
        Stretch(label, 16f, 22f, 16f, 12f);
        Txt(label, 86, wordColor, outline: false, text: word);

        Entrance(card, UIEntrance.Mode.Pop, delay, 0.55f);
        var f = GetOrAdd<UIFloat>(card);
        f.bobAmount = 14f;
        f.bobSpeed = 0.35f;
        f.swayDegrees = 2.5f;
        f.swaySpeed = 0.25f;
        Dirty(f);
    }

    private static void SettingRow(Transform card, string labelName, string sliderName, string label, float y,
        Sprite fill, Sprite handle)
    {
        if (card == null) return;
        Transform l = card.Find(labelName);
        At(l, Center, new Vector2(-230f, y), new Vector2(260f, 80f));
        Txt(l, 54, Ink, outline: false, text: label);
        if (l != null) l.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;

        Transform s = card.Find(sliderName);
        At(s, Center, new Vector2(120f, y), new Vector2(420f, 44f));
        SetSliced(s, Sp("slider/progress_bar (1).png"));
        if (s == null) return;

        Transform f = s.Find("Filler");
        Stretch(f, 7f, 7f, 7f, 7f);
        SetImage(f, fill, keepType: true);

        Transform h = s.Find("Handle");
        SetImage(h, handle, preserveAspect: true);
        if (h is RectTransform hrt)
        {
            hrt.localScale = Vector3.one;
            hrt.sizeDelta = new Vector2(72f, 28f); // height = bar (44) + 28 => 72px circle
            Dirty(hrt);
        }
    }

    // ================================================================== GAME

    private static void RestyleGame(Transform c, Sprite bg)
    {
        ApplyFontEverywhere(c);
        Background(c, bg);

        // ---- Header row: coins | level | pause
        Transform header = Find(c, "HeaderPanel");
        if (header is RectTransform hrt)
        {
            hrt.anchorMin = new Vector2(0f, 1f);
            hrt.anchorMax = new Vector2(1f, 1f);
            hrt.pivot = new Vector2(0.5f, 1f);
            hrt.anchoredPosition = new Vector2(0f, -40f);
            hrt.sizeDelta = new Vector2(0f, 130f);
            hrt.localScale = Vector3.one;
            Dirty(hrt);
            var headerImg = header.GetComponent<Image>();
            if (headerImg != null)
            {
                headerImg.sprite = null;
                headerImg.color = new Color(1f, 1f, 1f, 0f);
                headerImg.raycastTarget = false;
                Dirty(headerImg);
            }
        }
        Entrance(header, UIEntrance.Mode.DropIn, 0.05f, 0.7f, 200f);

        Transform coins = Find(c, "HeaderPanel/CoinsHolder");
        At(coins, Left, new Vector2(215f, 0f), new Vector2(280f, 92f));
        SetSliced(coins, Sp("gameUI/title_bg.png"));
        Transform coinImg = Find(c, "HeaderPanel/CoinsHolder/CoinImage");
        At(coinImg, Left, new Vector2(4f, 4f), new Vector2(124f, 124f));
        SetImage(coinImg, Sp("gameUI/coin_img (1).png"), preserveAspect: true);
        Transform coinsText = Find(c, "HeaderPanel/CoinsHolder/CoinsText");
        Stretch(coinsText, 70f, 8f, 20f, 4f);
        Txt(coinsText, 54, Ink, outline: false);

        Transform level = Find(c, "HeaderPanel/LevelBar");
        At(level, Center, Vector2.zero, new Vector2(330f, 104f));
        SetSliced(level, Sp("gameUI/purpleBg.png"));
        Stretch(Find(c, "HeaderPanel/LevelBar/Text"), 16f, 10f, 16f, 6f);
        Txt(Find(c, "HeaderPanel/LevelBar/Text"), 56, Color.white, outline: true, text: "LEVEL 1");

        Transform pause = Find(c, "HeaderPanel/PauseButton");
        At(pause, Right, new Vector2(-95f, 0f), new Vector2(112f, 120f));
        SetImage(pause, Sp("icons/back_btn (1).png"), preserveAspect: true);
        Hide(Find(c, "HeaderPanel/PauseButton/Text"));

        // ---- Timer bar
        Transform timeBar = Find(c, "TimeBar");
        At(timeBar, Top, new Vector2(0f, -255f), new Vector2(760f, 58f));
        SetSliced(timeBar, Sp("slider/progress_bar (1).png"));
        Transform filler = Find(c, "TimeBar/Filler");
        Stretch(filler, 9f, 9f, 9f, 9f);
        SetImage(filler, Sp("slider/greenBarFill.png"), keepType: true);
        if (filler != null)
        {
            var timer = GetOrAdd<TimerBarFX>(filler);
            timer.fill = filler.GetComponent<Image>();
            timer.highSprite = Sp("slider/greenBarFill.png");
            timer.midSprite = Sp("slider/yellowBarFill.png");
            timer.lowSprite = Sp("slider/pinkBarFill.png");
            timer.pulseTarget = timeBar as RectTransform;
            timer.pulseAmount = 0.04f;
            Dirty(timer);
        }
        Entrance(timeBar, UIEntrance.Mode.Fade, 0.3f, 0.4f);

        // ---- Strike indicators
        for (int i = 1; i <= 3; i++)
        {
            Transform slot = Find(c, "Cross" + i);
            At(slot, Top, new Vector2((i - 2) * 125f, -365f), new Vector2(100f, 100f));
            SetImage(slot, Sp("icons/close_btn (3).png"), preserveAspect: true, color: new Color(1f, 1f, 1f, 0.4f));
            Transform x = Find(c, "Cross" + i + "/cross");
            Stretch(x, 0f, 0f, 0f, 0f);
            SetImage(x, Sp("icons/close_btn.png"), preserveAspect: true);
            var pop = Entrance(x, UIEntrance.Mode.Pop, 0f, 0.4f);
            if (pop != null) pop.spinFrom = -180f;
        }

        // ---- Question card
        Transform qPanel = Find(c, "QuestionPanel");
        At(qPanel, Center, new Vector2(0f, 110f), new Vector2(940f, 640f));
        SetSliced(qPanel, Sp("gameUI/card_bg.png"));
        Entrance(qPanel, UIEntrance.Mode.Pop, 0.15f, 0.5f);
        Transform question = Find(c, "QuestionPanel/Question");
        Stretch(question, 70f, 70f, 70f, 70f);
        Txt(question, 78, Ink, outline: false);
        if (question != null) question.GetComponent<Text>().lineSpacing = 1.1f;

        // ---- Answers, side by side
        Transform leftBtn = Find(c, "LeftAnswerButton");
        Transform rightBtn = Find(c, "RightAnswerButton");
        At(leftBtn, Center, new Vector2(-245f, -445f), new Vector2(450f, 193f));
        At(rightBtn, Center, new Vector2(245f, -445f), new Vector2(450f, 193f));
        AnswerButton(leftBtn, Sp("button/emptyButtons/blueButton.png"));
        AnswerButton(rightBtn, Sp("button/emptyButtons/pinkButton.png"));
        LinkAnswerFx(leftBtn, rightBtn);

        // ---- Hint button, bottom centre, with a counter badge
        Transform hint = Find(c, "HintButton");
        At(hint, Bottom, new Vector2(0f, 230f), new Vector2(150f, 161f));
        SetImage(hint, Sp("icons/settings_btn (1).png"), preserveAspect: true);
        var hintFx = ButtonFx(hint);
        hintFx.idlePulse = true;
        hintFx.pulseAmount = 0.04f;
        hintFx.popInOnEnable = true;
        hintFx.popInDelay = 0.6f;
        Transform hintLabel = Find(c, "HintButton/HintText");
        if (hintLabel is RectTransform hl)
        {
            hl.anchorMin = new Vector2(0f, 0f);
            hl.anchorMax = new Vector2(1f, 0f);
            hl.pivot = new Vector2(0.5f, 1f);
            hl.anchoredPosition = new Vector2(0f, -4f);
            hl.sizeDelta = new Vector2(100f, 56f);
            hl.localScale = Vector3.one;
            Dirty(hl);
        }
        Txt(hintLabel, 42, Color.white, outline: true, text: "HINT");
        if (hint != null)
        {
            Transform counter = hint.Find("HintsCounter");
            if (counter == null) counter = hint.Find("CounterBadge/HintsCounter");
            HintBadge(hint, counter);
        }

        // ---- Pause popup
        Transform pauseCard = Find(c, "LevelPausePanel/POP");
        StylePopup(Find(c, "LevelPausePanel"), pauseCard, "Title", Sp("gameUI/purpleBg.png"), new Vector2(760f, 640f), 460f);
        Transform resume = Find(c, "LevelPausePanel/POP/ResumeButton");
        SetImage(resume, Sp("button/resume.png"));
        At(resume, Center, new Vector2(0f, 40f), new Vector2(440f, 189f));
        Hide(Find(c, "LevelPausePanel/POP/ResumeButton/Text"));
        TextButton(Find(c, "LevelPausePanel/POP/HomeButton"), Sp("button/emptyButtons/blueButton.png"), new Vector2(0f, -170f), new Vector2(380f, 163f), 64);

        // ---- Failed popup
        Transform failCard = Find(c, "LevelFailedPanel/POP");
        StylePopup(Find(c, "LevelFailedPanel"), failCard, "Title", Sp("gameUI/red_btn.png"), new Vector2(780f, 700f), 560f);
        Transform chance = Find(c, "LevelFailedPanel/POP/ChanceText");
        At(chance, Center, new Vector2(0f, 120f), new Vector2(660f, 90f));
        Txt(chance, 52, Ink, outline: false, text: "ONE MORE CHANCE?");
        Transform countdown = Find(c, "LevelFailedPanel/POP/TimeToEndImage");
        At(countdown, Center, new Vector2(0f, 40f), new Vector2(520f, 28f));
        SetImage(countdown, Sp("slider/yellowBarFill.png"), keepType: true);
        Transform retry = Find(c, "LevelFailedPanel/POP/RetryButton");
        SetImage(retry, Sp("button/restart.png"));
        At(retry, Center, new Vector2(0f, -150f), new Vector2(440f, 189f));
        Hide(Find(c, "LevelFailedPanel/POP/RetryButton/Text"));
        var retryFx = ButtonFx(retry);
        retryFx.popInOnEnable = true;
        TextButton(Find(c, "LevelFailedPanel/POP/LastTry"), Sp("button/emptyButtons/yellowButton.png"), new Vector2(0f, -150f), new Vector2(440f, 189f), 66);

        // ---- Completed popup
        Transform doneCard = Find(c, "LevelCompletedPanel/POP");
        StylePopup(Find(c, "LevelCompletedPanel"), doneCard, "Title", Sp("gameUI/green_btn.png"), new Vector2(860f, 980f), 640f);
        At(Find(c, "LevelCompletedPanel/ParticleEffect"), Center, new Vector2(0f, 120f), new Vector2(1500f, 1500f));
        var rays = Find(c, "LevelCompletedPanel/ParticleEffect");
        if (rays != null)
        {
            var raysImg = rays.GetComponent<Image>();
            raysImg.color = new Color(1f, 1f, 1f, 0.35f);
            Dirty(raysImg);
        }
        Star(Find(c, "LevelCompletedPanel/POP/Star_1"), new Vector2(-215f, 255f), 180f, 0f, 3f, 0.30f);
        Star(Find(c, "LevelCompletedPanel/POP/Star_2"), new Vector2(0f, 300f), 230f, 3f, 4f, 0.45f);
        Star(Find(c, "LevelCompletedPanel/POP/Star_3"), new Vector2(215f, 255f), 180f, 4f, 5f, 0.60f);
        string[] resultTexts = { "CorrectAnswersText", "WrongAnswersText", "CoinsText" };
        for (int i = 0; i < resultTexts.Length; i++)
        {
            Transform t = Find(c, "LevelCompletedPanel/POP/" + resultTexts[i]);
            At(t, Center, new Vector2(0f, 70f - i * 85f), new Vector2(660f, 76f));
            Txt(t, 50, i == 2 ? new Color32(0xE8, 0x8A, 0x00, 0xFF) : Ink, outline: false);
            Entrance(t, UIEntrance.Mode.FadeSlideUp, 0f, 0.35f, 40f);
        }
        // The holder slides to localPosition (0,-10) in GameManager; buttons sit below that point.
        Transform holder = Find(c, "LevelCompletedPanel/POP/CompleteButtons");
        At(holder, Center, new Vector2(0f, -160f), new Vector2(100f, 100f));
        TextButton(Find(c, "LevelCompletedPanel/POP/CompleteButtons/NextButton"), Sp("button/emptyButtons/greenButton.png"), new Vector2(190f, -330f), new Vector2(340f, 146f), 60);
        TextButton(Find(c, "LevelCompletedPanel/POP/CompleteButtons/HomeButton"), Sp("button/emptyButtons/blueButton.png"), new Vector2(-190f, -330f), new Vector2(340f, 146f), 60);

        // ---- Hints popup
        Transform hintCard = Find(c, "HintsPanel/POP");
        StylePopup(Find(c, "HintsPanel"), hintCard, "Text", Sp("button/emptyButtons/yellowButton.png"), new Vector2(740f, 620f), 560f);
        Transform back = Find(c, "HintsPanel/POP/BackButton");
        SetImage(back, Sp("icons/close_btn.png"), preserveAspect: true);
        At(back, TopRight, new Vector2(-20f, -20f), new Vector2(100f, 108f));
        Transform pill = Find(c, "HintsPanel/POP/Image");
        At(pill, Center, new Vector2(0f, 60f), new Vector2(460f, 110f));
        SetSliced(pill, Sp("gameUI/title_bg.png"));
        Stretch(Find(c, "HintsPanel/POP/Image/Text"), 10f, 10f, 10f, 6f);
        Txt(Find(c, "HintsPanel/POP/Image/Text"), 58, Ink, outline: false, text: "OUT OF HINTS");
        Entrance(pill, UIEntrance.Mode.Pop, 0.2f, 0.4f);
        Transform video = Find(c, "HintsPanel/POP/HintsVideoButton");
        SetImage(video, Sp("button/emptyButtons/greenButton.png"));
        At(video, Center, new Vector2(0f, -140f), new Vector2(420f, 180f));
        EnsureLabel(video, "+1 HINT", 66);

        // ---- Remove ads popup (currently unused, kept consistent)
        Transform adsCard = Find(c, "RemoveAdsPanel/RemoveAdsPop");
        StylePopup(Find(c, "RemoveAdsPanel"), adsCard, "RemoveAdsText", Sp("gameUI/purpleBg.png"), new Vector2(760f, 600f), 480f);
        Transform dialog = Find(c, "RemoveAdsPanel/RemoveAdsPop/Dialog");
        At(dialog, Center, new Vector2(0f, 50f), new Vector2(640f, 150f));
        Txt(dialog, 48, Ink, outline: false);
        TextButton(Find(c, "RemoveAdsPanel/RemoveAdsPop/YesButton"), Sp("button/emptyButtons/greenButton.png"), new Vector2(-170f, -150f), new Vector2(300f, 129f), 56);
        TextButton(Find(c, "RemoveAdsPanel/RemoveAdsPop/NoButton"), Sp("button/emptyButtons/pinkButton.png"), new Vector2(170f, -150f), new Vector2(300f, 129f), 56);

        // ---- Red flash on wrong answer: fade instead of a hard cut
        Entrance(Find(c, "WrongAnswerIndicator"), UIEntrance.Mode.Fade, 0f, 0.12f);

        AddButtonFxToAll(c);
        Bubbles(c);
    }

    // ------------------------------------------------------------------ building blocks

    private static void Background(Transform canvas, Sprite sprite)
    {
        Transform bg = Find(canvas, "BG");
        if (bg == null || sprite == null) return;
        var img = bg.GetComponent<Image>();
        img.sprite = sprite;
        img.type = Image.Type.Simple;
        img.preserveAspect = false;
        img.color = Color.white;
        img.raycastTarget = false;
        Dirty(img);
        Stretch(bg, 0f, 0f, 0f, 0f);
    }

    private static void Bubbles(Transform canvas)
    {
        Transform bg = Find(canvas, "BG");
        Transform fx = Child(canvas, "FX_Bubbles");
        Stretch(fx, 0f, 0f, 0f, 0f);
        fx.SetSiblingIndex(bg != null ? bg.GetSiblingIndex() + 1 : 0);

        var bubbles = GetOrAdd<BackgroundBubbles>(fx);
        bubbles.count = 12;
        bubbles.maxAlpha = 0.16f;
        bubbles.sizeRange = new Vector2(24f, 90f);
        bubbles.speedRange = new Vector2(25f, 70f);
        bubbles.sprites = new[]
        {
            Sp("slider/Ellipse 19.png"),
            Sp("slider/Ellipse 16 (1).png"),
            Sp("gameUI/Star 1.png"),
            Sp("icons/star_big.png"),
        }.Where(s => s != null).ToArray();
        Dirty(bubbles);
    }

    private static void StylePopup(Transform root, Transform card, string titleName, Sprite ribbonSprite, Vector2 cardSize, float ribbonWidth)
    {
        if (root == null) return;
        Stretch(root, 0f, 0f, 0f, 0f);
        var overlay = root.GetComponent<Image>();
        if (overlay != null)
        {
            overlay.sprite = null;
            overlay.type = Image.Type.Simple;
            overlay.color = OverlayColor;
            Dirty(overlay);
        }
        if (card == null) return;

        At(card, Center, Vector2.zero, cardSize);
        SetSliced(card, Sp("gameUI/card_bg.png"));

        var popup = GetOrAdd<UIPopup>(root);
        popup.card = card as RectTransform;
        Dirty(popup);

        Transform title = card.Find(titleName);
        if (title == null) title = card.Find("TitleRibbon/" + titleName);
        if (title != null && ribbonSprite != null) Ribbon(card, title, ribbonSprite, ribbonWidth);
    }

    /// <summary>A pill banner straddling the top edge of the card, with the title inside it.</summary>
    private static void Ribbon(Transform card, Transform title, Sprite sprite, float width)
    {
        Transform ribbon = Child(card, "TitleRibbon", typeof(Image));
        At(ribbon, Top, new Vector2(0f, -6f), new Vector2(width, 128f));
        ribbon.SetAsLastSibling();
        SetSliced(ribbon, sprite);
        ribbon.GetComponent<Image>().raycastTarget = false;

        title.SetParent(ribbon, false);
        title.gameObject.SetActive(true);
        Stretch(title, 24f, 18f, 24f, 10f);
        Txt(title, 62, Color.white, outline: true, outlineSize: 3f);

        var entrance = Entrance(ribbon, UIEntrance.Mode.Pop, 0.12f, 0.45f);
        if (entrance != null) entrance.spinFrom = 8f;
    }

    private static void HintBadge(Transform hintButton, Transform counter)
    {
        if (hintButton == null || counter == null) return;
        Transform badge = Child(hintButton, "CounterBadge", typeof(Image));
        At(badge, TopRight, new Vector2(-14f, -14f), new Vector2(66f, 66f));
        var img = badge.GetComponent<Image>();
        img.sprite = Sp("slider/Ellipse 18.png");
        img.preserveAspect = true;
        img.raycastTarget = false;
        img.color = Color.white;
        Dirty(img);

        counter.SetParent(badge, false);
        Stretch(counter, 0f, 4f, 0f, 0f);
        Txt(counter, 40, Color.white, outline: true, outlineSize: 2f);
    }

    private static void AnswerButton(Transform button, Sprite normal)
    {
        if (button == null) return;
        SetImage(button, normal);
        var label = button.GetComponentInChildren<Text>(true);
        if (label != null)
        {
            Stretch(label.transform, 20f, 26f, 20f, 14f);
            Txt(label.transform, 84, Color.white, outline: true, outlineSize: 3f);
        }
        var fx = GetOrAdd<AnswerButtonFX>(button);
        fx.normalSprite = normal;
        fx.hintSprite = Sp("button/emptyButtons/greenButton.png");
        fx.correctSprite = Sp("button/emptyButtons/greenButton.png");
        Dirty(fx);
        ButtonFx(button);
    }

    private static void LinkAnswerFx(Transform left, Transform right)
    {
        var lm = Object.FindAnyObjectByType<LevelManager>();
        if (lm == null) return;
        var so = new SerializedObject(lm);
        if (left != null) so.FindProperty("leftButtonFX").objectReferenceValue = left.GetComponent<AnswerButtonFX>();
        if (right != null) so.FindProperty("rightButtonFX").objectReferenceValue = right.GetComponent<AnswerButtonFX>();
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void TextButton(Transform button, Sprite sprite, Vector2 pos, Vector2 size, int fontSize)
    {
        if (button == null) return;
        SetImage(button, sprite);
        At(button, Center, pos, size);
        var label = button.GetComponentInChildren<Text>(true);
        if (label != null)
        {
            label.gameObject.SetActive(true);
            Stretch(label.transform, 16f, 22f, 16f, 12f);
            Txt(label.transform, fontSize, Color.white, outline: true, outlineSize: 3f);
        }
    }

    private static void EnsureLabel(Transform button, string text, int fontSize)
    {
        if (button == null) return;
        var label = button.GetComponentInChildren<Text>(true);
        Transform t = label != null ? label.transform : Child(button, "Label", typeof(Text));
        Stretch(t, 16f, 22f, 16f, 12f);
        Txt(t, fontSize, Color.white, outline: true, outlineSize: 3f, text: text);
    }

    private static void Star(Transform star, Vector2 pos, float size, float min, float max, float delay)
    {
        if (star == null) return;
        At(star, Center, pos, new Vector2(size, size));
        var slider = star.GetComponent<Slider>();
        if (slider != null)
        {
            slider.minValue = min;
            slider.maxValue = max;
            slider.value = min;
            Dirty(slider);
        }
        SetImage(star, Sp("gameUI/Star 1.png"), preserveAspect: true, color: StarEmpty);
        Transform fill = star.Find("Star_Filler");
        Stretch(fill, 0f, 0f, 0f, 0f);
        SetImage(fill, Sp("gameUI/Star 1.png"), keepType: true, preserveAspect: true);
        var fillImg = fill ? fill.GetComponent<Image>() : null;
        if (fillImg != null)
        {
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.fillOrigin = (int)Image.OriginHorizontal.Left;
            Dirty(fillImg);
        }
        var fx = GetOrAdd<StarFX>(star);
        fx.popDelay = delay;
        Dirty(fx);
    }

    private static void AddButtonFxToAll(Transform canvas)
    {
        foreach (var b in canvas.GetComponentsInChildren<Button>(true))
            ButtonFx(b.transform);
    }

    private static UIButtonFX ButtonFx(Transform t)
    {
        if (t == null) return null;
        var fx = GetOrAdd<UIButtonFX>(t);
        Dirty(fx);
        return fx;
    }

    private static UIEntrance Entrance(Transform t, UIEntrance.Mode mode, float delay, float duration, float distance = 60f)
    {
        if (t == null) return null;
        var e = GetOrAdd<UIEntrance>(t);
        e.mode = mode;
        e.delay = delay;
        e.duration = duration;
        e.distance = distance;
        Dirty(e);
        return e;
    }

    // ------------------------------------------------------------------ layout helpers

    /// <summary>Anchor to a single point, centre pivot, explicit size, scale 1.</summary>
    private static void At(Transform t, Vector2 anchor, Vector2 pos, Vector2 size, float rotZ = 0f)
    {
        if (!(t is RectTransform rt)) return;
        rt.anchorMin = rt.anchorMax = anchor;
        rt.pivot = Center;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        rt.localScale = Vector3.one;
        rt.localEulerAngles = new Vector3(0f, 0f, rotZ);
        Dirty(rt);
    }

    /// <summary>Fill the parent with the given padding (left, bottom, right, top), scale 1.</summary>
    private static void Stretch(Transform t, float l, float b, float r, float top)
    {
        if (!(t is RectTransform rt)) return;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = Center;
        rt.offsetMin = new Vector2(l, b);
        rt.offsetMax = new Vector2(-r, -top);
        rt.localScale = Vector3.one;
        rt.localEulerAngles = Vector3.zero;
        Dirty(rt);
    }

    /// <summary>Find or create a child UI object.</summary>
    private static Transform Child(Transform parent, string name, params System.Type[] components)
    {
        Transform t = parent.Find(name);
        if (t != null) return t;
        var types = new[] { typeof(RectTransform) }.Concat(components.Length > 0 ? new[] { typeof(CanvasRenderer) } : new System.Type[0]).Concat(components).ToArray();
        var go = new GameObject(name, types);
        go.transform.SetParent(parent, false);
        return go.transform;
    }

    private static void SetImage(Transform t, Sprite sprite, bool preserveAspect = false, bool keepType = false, Color? color = null)
    {
        if (t == null || sprite == null) return;
        var img = t.GetComponent<Image>();
        if (img == null) return;
        img.sprite = sprite;
        if (!keepType) img.type = Image.Type.Simple;
        img.preserveAspect = preserveAspect;
        img.color = color ?? Color.white;
        Dirty(img);
    }

    /// <summary>9-slice the sprite and compensate for parent scaling so corners keep a consistent size.</summary>
    private static void SetSliced(Transform t, Sprite sprite)
    {
        if (t == null || sprite == null) return;
        var img = t.GetComponent<Image>();
        if (img == null) return;
        img.sprite = sprite;
        img.type = Image.Type.Sliced;
        img.fillCenter = true;
        img.color = Color.white;
        img.pixelsPerUnitMultiplier = Mathf.Max(0.01f, ScaleToCanvas(t) / 1.3f);
        Dirty(img);
    }

    private static float ScaleToCanvas(Transform t)
    {
        float s = 1f;
        for (Transform p = t; p != null && p.GetComponent<Canvas>() == null; p = p.parent)
            s *= Mathf.Abs(p.localScale.x);
        return s;
    }

    private static void ApplyFontEverywhere(Transform canvas)
    {
        if (font == null) return;
        foreach (var text in canvas.GetComponentsInChildren<Text>(true))
        {
            text.font = font;
            Dirty(text);
        }
    }

    /// <summary>Explicit font size; shrinks to fit (down to half size) when the text is long.</summary>
    private static void Txt(Transform t, int size, Color color, bool outline, float outlineSize = 2.5f, string text = null)
    {
        if (t == null) return;
        var tx = t.GetComponent<Text>();
        if (tx == null) return;
        if (font != null) tx.font = font;
        if (text != null) tx.text = text;
        tx.fontSize = size;
        tx.color = color;
        tx.alignment = TextAnchor.MiddleCenter;
        tx.horizontalOverflow = HorizontalWrapMode.Wrap;
        tx.verticalOverflow = VerticalWrapMode.Truncate;
        tx.raycastTarget = false;
        tx.resizeTextForBestFit = true;
        tx.resizeTextMaxSize = size;
        tx.resizeTextMinSize = Mathf.Max(16, size / 2);
        Dirty(tx);

        var o = t.GetComponent<Outline>();
        if (outline)
        {
            if (o == null) o = t.gameObject.AddComponent<Outline>();
            o.effectColor = OutlineColor;
            o.effectDistance = new Vector2(outlineSize, -outlineSize);
            Dirty(o);
        }
        else if (o != null)
        {
            Object.DestroyImmediate(o);
        }
    }

    private static void AddShadow(Transform t)
    {
        if (t == null) return;
        var shadows = t.GetComponents<Shadow>().Where(s => !(s is Outline)).ToArray();
        var shadow = shadows.Length > 0 ? shadows[0] : t.gameObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0.15f, 0.03f, 0.3f, 0.45f);
        shadow.effectDistance = new Vector2(0f, -10f);
        Dirty(shadow);
    }

    private static void Hide(Transform t)
    {
        if (t == null) return;
        t.gameObject.SetActive(false);
        Dirty(t.gameObject);
    }

    private static Transform Find(Transform root, string path)
    {
        Transform t = root.Find(path);
        if (t == null) Debug.LogWarning("[ColorfulUI] Not found: " + root.name + "/" + path);
        return t;
    }

    private static T GetOrAdd<T>(Transform t) where T : Component
    {
        var c = t.GetComponent<T>();
        return c != null ? c : t.gameObject.AddComponent<T>();
    }

    private static void Dirty(Object o)
    {
        if (o == null) return;
        EditorUtility.SetDirty(o);
        if (PrefabUtility.IsPartOfPrefabInstance(o))
            PrefabUtility.RecordPrefabInstancePropertyModifications(o);
    }

    private static Sprite Sp(string relative)
    {
        var s = AssetDatabase.LoadAssetAtPath<Sprite>(Pack + "sprites/" + relative);
        if (s == null) Debug.LogWarning("[ColorfulUI] Missing sprite: " + relative);
        return s;
    }

    // ------------------------------------------------------------------ assets

    /// <summary>Give the stretchable sprites 9-slice borders (left, bottom, right, top).</summary>
    private static void PrepareSprites()
    {
        var borders = new (string path, Vector4 border)[]
        {
            ("gameUI/card_bg.png", new Vector4(40, 40, 40, 40)),
            ("gameUI/purpleBg.png", new Vector4(21, 20, 21, 20)),
            ("gameUI/title_bg.png", new Vector4(14, 13, 14, 13)),
            ("gameUI/green_btn.png", new Vector4(16, 14, 16, 12)),
            ("gameUI/red_btn.png", new Vector4(16, 14, 16, 12)),
            ("button/emptyButtons/yellowButton.png", new Vector4(30, 30, 30, 30)),
            ("slider/progress_bar (1).png", new Vector4(13, 13, 13, 13)),
        };

        foreach (var (rel, border) in borders)
        {
            string path = Pack + "sprites/" + rel;
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                Debug.LogWarning("[ColorfulUI] No importer for " + path);
                continue;
            }
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            if (importer.spriteBorder == border && settings.spriteMeshType == SpriteMeshType.FullRect) continue;
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);
            importer.spriteBorder = border;
            importer.SaveAndReimport();
        }
    }

    /// <summary>Writes a smooth vertical gradient with two soft light blooms as a sprite asset.</summary>
    private static Sprite MakeGradient(string fileName, Color top, Color bottom)
    {
        if (!AssetDatabase.IsValidFolder("Assets/UI")) AssetDatabase.CreateFolder("Assets", "UI");
        if (!AssetDatabase.IsValidFolder(GeneratedDir)) AssetDatabase.CreateFolder("Assets/UI", "ColorfulTheme");
        string path = GeneratedDir + "/" + fileName;

        const int w = 270, h = 480;
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        var pixels = new Color[w * h];
        for (int y = 0; y < h; y++)
        {
            float v = y / (float)(h - 1);
            Color row = Color.Lerp(bottom, top, Mathf.SmoothStep(0f, 1f, v));
            for (int x = 0; x < w; x++)
            {
                float u = x / (float)(w - 1);
                float bloomA = Bloom(u, v, 0.15f, 0.82f, 0.35f) * 0.10f;
                float bloomB = Bloom(u, v, 0.90f, 0.25f, 0.40f) * 0.08f;
                float vignette = Mathf.Clamp01((Mathf.Abs(u - 0.5f) * 2f) - 0.5f) * 0.06f;
                Color c = row + new Color(1f, 1f, 1f, 0f) * (bloomA + bloomB) - new Color(vignette, vignette, vignette, 0f);
                c.a = 1f;
                pixels[y * w + x] = c;
            }
        }
        tex.SetPixels(pixels);
        tex.Apply();
        File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);

        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.mipmapEnabled = false;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static float Bloom(float u, float v, float cx, float cy, float radius)
    {
        float dx = (u - cx) * 0.5625f, dy = v - cy; // correct for 9:16 aspect
        float d = Mathf.Sqrt(dx * dx + dy * dy) / radius;
        return Mathf.Clamp01(1f - d * d);
    }
}
