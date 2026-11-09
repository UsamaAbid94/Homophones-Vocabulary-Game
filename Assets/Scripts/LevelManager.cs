using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class LevelManager : MonoBehaviour
{ [SerializeField]
    private Text questionText, rightButtonText, leftButtonText;
    [SerializeField]
    private List<Level> gameLevels = new List<Level>();
    private int currentQuestionNumber;
    private int currentLevelNumber;
    private int wrongAnswerCounter;
    private int correctAnswerCounter;
    private string correctAnswer;
    [SerializeField]
    private GameObject hintsPanel;
    [SerializeField]
    private Text hintsCounterText;

    [Header("UI FX (optional)")]
    [SerializeField]
    private AnswerButtonFX leftButtonFX;
    [SerializeField]
    private AnswerButtonFX rightButtonFX;
    [SerializeField]
    private float nextQuestionDelay = 0.35f;
    private bool inputLocked;
    private Vector3 questionBaseScale;

    // Start is called before the first frame update
    void Start()
    {
        currentQuestionNumber = 0;
        currentLevelNumber = PlayerPrefs.GetInt("LevelNo");
        questionBaseScale = questionText.transform.localScale;
        ResetHint();
        SetQuestion();
        Debug.Log("LEVEL" + currentLevelNumber);
        hintsCounterText.text = PlayerPrefs.GetInt("Hints").ToString();
        if (PlayerPrefs.GetInt("FirstTime") == 0)
        {
            PlayerPrefs.SetInt("Hints", 3);
            hintsCounterText.text = PlayerPrefs.GetInt("Hints").ToString();
            PlayerPrefs.SetInt("FirstTime", 1);
        }
    }

    public int WrongAnswerCount
    {
        set
        {
            wrongAnswerCounter = value;
        }
        get
        {
            return wrongAnswerCounter;
        }
    }

    /// <summary>Blocks answer and hint taps (used while panels are open or between questions).</summary>
    public void SetInputLocked(bool locked)
    {
        inputLocked = locked;
    }

    public void SetQuestion()
    {
        ResetHint();
        int randomAnswerGeneration = Random.Range(0, 2);
        questionText.text = gameLevels[currentLevelNumber].gameQuestions[currentQuestionNumber].gameQuestion;
        if (randomAnswerGeneration == 1)
        {
            leftButtonText.text = gameLevels[currentLevelNumber].gameQuestions[currentQuestionNumber].wrongAnswer;
            rightButtonText.text = gameLevels[currentLevelNumber].gameQuestions[currentQuestionNumber].correctAnswer;
            correctAnswer = rightButtonText.text;
        }
        else
        {
            leftButtonText.text = gameLevels[currentLevelNumber].gameQuestions[currentQuestionNumber].correctAnswer;
            rightButtonText.text = gameLevels[currentLevelNumber].gameQuestions[currentQuestionNumber].wrongAnswer;
            correctAnswer = leftButtonText.text;
        }
        AnimateQuestionIn();
    }

    private void AnimateQuestionIn()
    {
        if (questionBaseScale == Vector3.zero) questionBaseScale = questionText.transform.localScale;
        UIFx.PopFrom(questionText.transform, questionBaseScale, 0.6f, 0.4f);
        if (leftButtonFX != null) leftButtonFX.PopIn(0.08f);
        if (rightButtonFX != null) rightButtonFX.PopIn(0.16f);
    }

    public void LeftButtonPressed()
    {
        HandleAnswer(leftButtonText, leftButtonFX);
    }

    public void RightButtonPressed()
    {
        HandleAnswer(rightButtonText, rightButtonFX);
    }

    private void HandleAnswer(Text buttonText, AnswerButtonFX fx)
    {
        if (inputLocked) return;
        SoundManager.soundManager.ButtonClickSound();

        string answer = buttonText.text;
        if (answer.Equals(gameLevels[currentLevelNumber].gameQuestions[currentQuestionNumber].correctAnswer))
        {
            Debug.Log("CORRECT");
            correctAnswerCounter++;
            if (fx != null) fx.PlayCorrect();
            inputLocked = true;

            if (currentQuestionNumber < gameLevels[currentLevelNumber].gameQuestions.Count - 1)
            {
                // Reset the timer right away so it can't run out during the transition.
                GameManager.gameManager.GetTimeBar().fillAmount = 1f;
                StartCoroutine(NextQuestionAfterDelay());
            }
            else
            {
                GameManager.gameManager.LevelCompleted();
            }
        }
        else
        {
            GameManager.gameManager.CrossImages()[wrongAnswerCounter].SetActive(true);
            StartCoroutine(GameManager.gameManager.WrongAnswerRedIndicator());
            if (fx != null) fx.PlayWrong();
            wrongAnswerCounter++;
            if (wrongAnswerCounter >= 3)
            {
                GameManager.gameManager.LevelFailed();
            }
        }
        Debug.Log(wrongAnswerCounter);
        Debug.Log(currentQuestionNumber);
    }

    private IEnumerator NextQuestionAfterDelay()
    {
        yield return new WaitForSecondsRealtime(nextQuestionDelay);
        currentQuestionNumber++;
        GameManager.gameManager.GetTimeBar().fillAmount = 1f;
        SetQuestion();
        inputLocked = false;
    }

    public int GetCorrectAnswers()
    {
        return correctAnswerCounter;
    }
    public int GetWrongAnswers()
    {
        return wrongAnswerCounter;
    }

    public void IncreaseQuestionNumber()
    {
        currentQuestionNumber++;
    }

    public void HintButtonClicked()
    {
        if (inputLocked) return;
        SoundManager.soundManager.ButtonClickSound();
        if (PlayerPrefs.GetInt("Hints") > 0)
        {
            if (rightButtonText.text.Equals(correctAnswer))
            {
                HighlightHint(rightButtonText, rightButtonFX);
            }
            else if (leftButtonText.text.Equals(correctAnswer))
            {
                HighlightHint(leftButtonText, leftButtonFX);
            }
            PlayerPrefs.SetInt("Hints", PlayerPrefs.GetInt("Hints") - 1);
            hintsCounterText.text = PlayerPrefs.GetInt("Hints").ToString();
        }
        else
        {
            hintsPanel.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    private void HighlightHint(Text buttonText, AnswerButtonFX fx)
    {
        if (fx != null) fx.ShowHint();
        else buttonText.transform.parent.GetComponent<Image>().color = Color.green;
    }

    public void GetMoreHints()
    {
        Time.timeScale = 1f;
        PlayerPrefs.SetInt("Hints", PlayerPrefs.GetInt("Hints") + 1);
        hintsCounterText.text = PlayerPrefs.GetInt("Hints").ToString();
        hintsPanel.SetActive(false);

    }

    public void WatchVideoForHints()
    {
        SoundManager.soundManager.ButtonClickSound();
        //AdsManager.instance.ShowRewardedAd();
        GetMoreHints();
    }
    public void ResetHint()
    {
        if (rightButtonFX != null) rightButtonFX.ResetVisual();
        else rightButtonText.transform.parent.GetComponent<Image>().color = Color.white;
        if (leftButtonFX != null) leftButtonFX.ResetVisual();
        else leftButtonText.transform.parent.GetComponent<Image>().color = Color.white;
    }


    [System.Serializable]
    public class Level
    {
        public string levelName;
        public List<Question> gameQuestions = new List<Question>();
    }

    [System.Serializable]
    public class Question
    {
        public string questionNo;
        public string gameQuestion;
        public string correctAnswer;
        public string wrongAnswer;

    }




}
