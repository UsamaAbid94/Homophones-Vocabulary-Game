using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour
{
    public static GameManager gameManager;
    [SerializeField]
    private Image timeBar;
    [SerializeField]
    private GameObject wrongAnswerIndicator;
    [SerializeField]
    private GameObject levelCompletePanel, levelFailedPanel, levelPausePanel;
    [SerializeField]
    private LevelManager levelManager;
    [SerializeField]
    private GameObject[] crossImages;
    [SerializeField]
    private Text levelBarText;
    private int wrongAnswers;
    [SerializeField]
    private AudioSource audioSource;
    [SerializeField]
    private AudioClip failSound, winSound,scoreCounterSound,startGameClip;
    [SerializeField]
    private Image timeSlider;
    [SerializeField]
    private Button tryAgainButton, restartButton;
    private bool isGameFail,islevelCompleted;
    [SerializeField]
    private Text gameCoinsText;



    //Level Completed Panel 
    [SerializeField]
    private Text correctAnswersText;
    [SerializeField]
    private Text wrongAnswersText;
    [SerializeField]
    private Text coinsRewardText;
    [SerializeField]
    private Slider[] levelCompletedStars;
    [SerializeField]
    private ParticleSystem levelCompletedParticle;
    [SerializeField]
    private GameObject completeButtonsHolder;
    private float correctAnswersCount;
    private float wrongAnswersCount;
    private float levelRewardCount;

    //Screen Handler
    [SerializeField]
    private RectTransform screenFader;
    [SerializeField]
    private GameObject removeAdsPop;


    private void Awake()
    {
        if (gameManager == null)
        {
            gameManager = this;
            Time.timeScale = 1.0f;
        }
        else
        {
            Destroy(gameManager);
        }
    }
  
    
    // Start is called before the first frame update
    void Start()
    {
        
        levelBarText.text = "LEVEL "+(PlayerPrefs.GetInt("LevelNo") + 1).ToString();
      
        islevelCompleted = false;
        gameCoinsText.text = PlayerPrefs.GetInt("Level_Coins").ToString();
        screenFader.gameObject.SetActive(true);
        LeanTween.scale(screenFader,new Vector3(1,1,1),0).setEase(LeanTweenType.easeInCirc);
        LeanTween.scale(screenFader,Vector3.zero,0.5f).setEase(LeanTweenType.easeInCirc).setOnComplete(()=>
        {
            screenFader.gameObject.SetActive(false);
        });
        audioSource.clip = startGameClip;
        audioSource.Play();
        
    }

    // Update is called once per frame
    void Update()
    {
      timeBar.fillAmount -= Time.deltaTime * 0.03f;
        if (timeBar.fillAmount <=0f)
        {
            levelManager.IncreaseQuestionNumber();
            levelManager.SetQuestion();
            timeBar.fillAmount = 1f;
            crossImages[wrongAnswers].SetActive(true);
            wrongAnswers++;
            if (wrongAnswers >= 3)
            {
              LevelFailed();
            }
        }

        if (isGameFail)
        { 
            timeSlider.fillAmount -= Time.deltaTime *0.1f;
          
            if (timeSlider.fillAmount <= 0f)
            {
                timeSlider.gameObject.SetActive(false);
                tryAgainButton.gameObject.SetActive(false);
                restartButton.gameObject.SetActive(true);
  
            }
        }
        if (islevelCompleted) {
            float answersValue = (float)levelManager.GetCorrectAnswers() -(float)levelManager.GetWrongAnswers();
            for (int i = 0; i < levelCompletedStars.Length; i++)
            {
                levelCompletedStars[i].value = Mathf.Lerp(levelCompletedStars[i].value,answersValue , 1.5f*Time.deltaTime);
                
            }
        }
       
    }

    public LevelManager GetLevelManager()
    {
        return levelManager;
    }

    public Image GetTimeBar()
    {
        return timeBar;
    }
    public GameObject[] CrossImages()
    {
        return crossImages;
    }
    public void LevelCompleted()
    {

        //correctAnswersText.text = "CORRECT ANSWERS : "+ levelManager.GetCorrectAnswers();
        //wrongAnswersText.text = "WRONG ANSWERS : " + levelManager.GetWrongAnswers();
        //coinsRewardText.text = "COINS REWARD : "+PlayerPrefs.GetInt("Level_Coins").ToString();
       
        StartCoroutine(CountCorrectAnswers());
      
        levelCompletedParticle.gameObject.SetActive(true);
        levelCompletedParticle.Play();
        StartCoroutine(ShowCompletePanel());
        PlayerPrefs.SetInt("Level_Coins", PlayerPrefs.GetInt("Level_Coins") + 100);
       
    }

    IEnumerator CountCorrectAnswers()
    {
        audioSource.clip = scoreCounterSound;
        audioSource.Play();
        correctAnswersText.gameObject.SetActive(true);
        while (correctAnswersCount < levelManager.GetCorrectAnswers())
        {
            correctAnswersCount += Time.deltaTime *7f; // or whatever to get the speed you like
            correctAnswersCount = Mathf.Clamp(correctAnswersCount, 0f, levelManager.GetCorrectAnswers());
            correctAnswersText.text = "CORRECT ANSWERS : " + correctAnswersCount.ToString("0");
            correctAnswersText.transform.localScale = Vector2.Lerp(correctAnswersText.transform.localScale,new Vector2(1.2f,1.2f),Time.deltaTime*2f);
          
            yield return null;
        }
        wrongAnswersText.gameObject.SetActive(true);
        StartCoroutine(CountWrongAnswers());
    }
    IEnumerator CountWrongAnswers()
    {
        audioSource.clip = scoreCounterSound;
        audioSource.Play();
        while (wrongAnswersCount < levelManager.GetWrongAnswers())
        {
            wrongAnswersCount += Time.deltaTime * 7f; // or whatever to get the speed you like
            wrongAnswersCount = Mathf.Clamp(wrongAnswersCount, 0f, levelManager.GetWrongAnswers());
            wrongAnswersText.text = "WRONG TRIES : " + wrongAnswersCount.ToString("0");
            wrongAnswersText.transform.localScale = Vector2.Lerp(wrongAnswersText.transform.localScale, new Vector2(1.2f, 1.2f), Time.deltaTime * 2f);
           
            yield return null;
        }
        coinsRewardText.gameObject.SetActive(true);
        StartCoroutine(CountLevelReward());
    }
    IEnumerator CountLevelReward()
    {
        audioSource.clip = scoreCounterSound;
        audioSource.Play();
        while (levelRewardCount < 100f)
        {
            levelRewardCount += Time.deltaTime * 50f; // or whatever to get the speed you like
            levelRewardCount = Mathf.Clamp(levelRewardCount, 0f,100f);
            coinsRewardText.text = "COINS REWARD : " + levelRewardCount.ToString("0");
            coinsRewardText.transform.localScale = Vector2.Lerp(coinsRewardText.transform.localScale, new Vector2(1.2f, 1.2f), Time.deltaTime * 2f);
            completeButtonsHolder.GetComponent<RectTransform>().localPosition = Vector2.Lerp(completeButtonsHolder.GetComponent<RectTransform>().localPosition, new Vector2(0f,-10f), Time.deltaTime * 2f);
            yield return null;
        }
        PlayWinSound();
       
      
    } 

    IEnumerator ShowCompletePanel()
    {
        yield return new WaitForSeconds(0.2f);
        islevelCompleted = true;
        levelCompletePanel.SetActive(true);
       // ShowRemoveAdsPopup();
        
    }
    public void LevelFailed()
    {
        
        wrongAnswerIndicator.SetActive(false);
        levelFailedPanel.SetActive(true);
        isGameFail = true;
    }

    public void PauseLevel()
    {
        Time.timeScale = 0f;
        levelPausePanel.SetActive(true);
      
    }

    public void ResumeLevel()
    {
        Time.timeScale = 1f;
        levelPausePanel.SetActive(false);
        SoundManager.soundManager.ButtonClickSound();
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Game");
        SoundManager.soundManager.ButtonClickSound();
        LeanTween.scale(screenFader, Vector3.zero, 0);
        LeanTween.scale(screenFader, new Vector3(1,1,1), 0.5f).setOnComplete(() =>
        {
            screenFader.gameObject.SetActive(false);
        });
    }

    public void BackToHome()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
        SoundManager.soundManager.ButtonClickSound();
        AdsManager.instance.ShowInterstitialAd();
    
    }

    public void NextLevel()
    {
        Time.timeScale = 1f;
        if (PlayerPrefs.GetInt("LevelNo") <19)
        {
            PlayerPrefs.SetInt("LevelNo", PlayerPrefs.GetInt("LevelNo") + 1);
            PlayerPrefs.SetInt("Hints", PlayerPrefs.GetInt("Hints") + 1);
        }
        else
        {
            PlayerPrefs.SetInt("LevelNo",0);
        }
        SceneManager.LoadScene("Game");
        SoundManager.soundManager.ButtonClickSound();
        AdsManager.instance.ShowInterstitialAd();
      
    }

    public void TryAgainWithVideo()
    {

        AdsManager.instance.ShowRewardedAd();
        TryAgain();
    }
    public void TryAgain()
    {
        Time.timeScale = 1f;
        levelFailedPanel.SetActive(false);
        levelManager.WrongAnswerCount = 0;
        for (int i = 0; i < crossImages.Length; i++)
        {
            crossImages[i].SetActive(false);
        }
     
    }

   public  IEnumerator  WrongAnswerRedIndicator()
    {
        PlayFailSound();
        wrongAnswerIndicator.SetActive(true);
        yield return new WaitForSeconds(0.4f);
        wrongAnswerIndicator.SetActive(false);
    }
    public void PlayFailSound()
    {
        audioSource.clip = failSound;
        audioSource.Play();
    }

    public void PlayWinSound()
    {
        audioSource.clip = winSound;
        audioSource.Play();
    }

    //public void ShowRemoveAdsPopup() {
    //    if (PlayerPrefs.GetInt("LevelNo") % 3 == 0 && !PlayerPrefs.GetString("RemoveAds").Equals("true"))
    //    {
    //        removeAdsPop.SetActive(true);
    //    }
      
    //}

    public void YesButtonClicked()
    {
      //  FindObjectOfType<InAppPurchaseManager>().RemoveAds();
    }

   
}
