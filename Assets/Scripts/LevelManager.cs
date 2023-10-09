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
    // Start is called before the first frame update
    void Start()
    {
        currentQuestionNumber = 0;
        currentLevelNumber = PlayerPrefs.GetInt("LevelNo");
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
    void Update()
    {

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

    }

    public void LeftButtonPressed()
    {
        string answer = leftButtonText.text.ToString();
        if (answer.Equals(gameLevels[currentLevelNumber].gameQuestions[currentQuestionNumber].correctAnswer))
        {
            Debug.Log("CORRECT");
            if (currentQuestionNumber < gameLevels[currentLevelNumber].gameQuestions.Count - 1)
            {
                currentQuestionNumber++;
                GameManager.gameManager.GetTimeBar().fillAmount = 1f;
            }
            else
            {
                GameManager.gameManager.LevelCompleted();
            }

            SetQuestion();
            correctAnswerCounter++;
        }
        else
        {
            GameManager.gameManager.CrossImages()[wrongAnswerCounter].SetActive(true);
            StartCoroutine(GameManager.gameManager.WrongAnswerRedIndicator());
            wrongAnswerCounter++;
            if (wrongAnswerCounter < 3)
            {

                //  currentQuestionNumber++;
                //  SetQuestion();

            }
            else
            {
                GameManager.gameManager.LevelFailed();
            }
        }
        SoundManager.soundManager.ButtonClickSound();
        Debug.Log(wrongAnswerCounter);
        Debug.Log(currentQuestionNumber);
    }

    public void RightButtonPressed()
    {
        string answer = rightButtonText.text.ToString();
        if (answer.Equals(gameLevels[currentLevelNumber].gameQuestions[currentQuestionNumber].correctAnswer))
        {
            Debug.Log("CORRECT");

            if (currentQuestionNumber < gameLevels[currentLevelNumber].gameQuestions.Count - 1)
            {
                currentQuestionNumber++;
                GameManager.gameManager.GetTimeBar().fillAmount = 1f;
            }
            else
            {
                GameManager.gameManager.LevelCompleted();
            }
            SetQuestion();
            correctAnswerCounter++;
        }
        else
        {
            GameManager.gameManager.CrossImages()[wrongAnswerCounter].SetActive(true);
            StartCoroutine(GameManager.gameManager.WrongAnswerRedIndicator());
            wrongAnswerCounter++;
            //  SetQuestion();
            if (wrongAnswerCounter < 3)
            {

                //    currentQuestionNumber++;
                //   SetQuestion();

            }
            else
            {
                GameManager.gameManager.LevelFailed();
            }


        }
        SoundManager.soundManager.ButtonClickSound();
        Debug.Log(wrongAnswerCounter);
        Debug.Log(currentQuestionNumber);
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
        SoundManager.soundManager.ButtonClickSound();
        if (PlayerPrefs.GetInt("Hints") > 0)
        {
            if (rightButtonText.text.Equals(correctAnswer))
            {
                rightButtonText.gameObject.transform.parent.gameObject.GetComponent<Image>().color = Color.green;
            }
            else if (leftButtonText.text.Equals(correctAnswer))
            {
                leftButtonText.gameObject.transform.parent.gameObject.GetComponent<Image>().color = Color.green;
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
        AdsManager.instance.ShowRewardedAd();
        GetMoreHints();
    }
    public void ResetHint()
    {
        rightButtonText.gameObject.transform.parent.gameObject.GetComponent<Image>().color = Color.white;
        leftButtonText.gameObject.transform.parent.gameObject.GetComponent<Image>().color = Color.white;
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
