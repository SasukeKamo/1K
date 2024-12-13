using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SettingsMenu : MonoBehaviour
{
    [SerializeField]
    MainMenu MainMenu;

    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnEnable()
    {
        if (AudioManager.Instance.musicSource != null)
        {
            musicSlider.value = AudioManager.Instance.musicSource.volume;
        }

        if (AudioManager.Instance.sfxSource != null)
        {
            sfxSlider.value = AudioManager.Instance.sfxSource.volume;
        }
    }

    public void OnMusicSliderChanged()
    {
        AudioManager.Instance.musicSource.volume = musicSlider.value;
        AudioManager.Instance.SaveMusicSliderValue(musicSlider.value);
    }

    public void OnSFXSliderChanged()
    {
        AudioManager.Instance.sfxSource.volume = sfxSlider.value;
        AudioManager.Instance.SaveSFXSliderValue(sfxSlider.value);
    }

    public void OnEnter()
    {
        gameObject.SetActive(true);
    }

    public void OnExit()
    {
        gameObject.SetActive(false);
    }

    public void Back()
    {
        MainMenu.OnEnter();
        OnExit();
    }
}

