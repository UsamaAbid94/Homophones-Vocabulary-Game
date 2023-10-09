using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class SoundManager : MonoBehaviour
{
    public static SoundManager soundManager;
    public AudioSource soundAudioSource;
    public AudioSource musicAudioSource;
    public AudioClip buttonClickClip;
   
    public MenuController menuController;
   
    private void Awake()
    {
        if (soundManager != null && soundManager != this)
        {
            Destroy(this.gameObject);
            return;//Avoid doing anything else
        }
       
        soundManager = this;
        DontDestroyOnLoad(this.gameObject);
    }
    
    public void ButtonClickSound()
    {
        soundAudioSource.clip = buttonClickClip;
        soundAudioSource.Play();
    }
}
