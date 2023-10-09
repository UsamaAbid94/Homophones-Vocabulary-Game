using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class MenuController : MonoBehaviour
{
    //  public GameObject levelSelectionPanel;
    public Button removeAdsButton;
    public Slider musicSlider, soundSlider;
    private float musicVolume = 1f, soundVolume = 1f;
    private void Start()
    {
      //  removeAdsButton.onClick.AddListener(() => InAppPurchaseManager.instance.RemoveAds());
        //OMTestSuite.Show("ElIiyqBnhXxziG6ogThYd13odD2k7lfl");
    
    }

    private void Update()
    {
       SoundManager.soundManager.soundAudioSource.volume = soundVolume;
       SoundManager.soundManager.musicAudioSource.volume = musicVolume;
    }
    public void PlayGame()
    {
        SceneManager.LoadScene("Game");
      
    }

    public void SettingsButtonClicked()
    {
        SoundManager.soundManager.ButtonClickSound();

    }

    public void ChangeSoundVolume(float soundVolume)
    {
        this.soundVolume = soundVolume;
    }

    public void ChangeMusicVolume(float musicVolume)
    {
        this.musicVolume = musicVolume;
    }
}
