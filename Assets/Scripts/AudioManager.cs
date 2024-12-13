using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    private static AudioManager _instance;

    public static AudioManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("AudioManager");
                _instance = go.AddComponent<AudioManager>();
            }
            return _instance;
        }
    }

    public AudioSource musicSource;
    public AudioSource sfxSource;

    public Slider musicSlider;
    public Slider sfxSlider;

    public AudioClip winSound;
    public AudioClip invalidMoveSound;
    public AudioClip playCardSound;
    public AudioClip selectCardSound;
    public AudioClip trumpSound;
    public AudioClip burnSound;
    public AudioClip gamesceneMusic;
    public AudioClip menuMusic;

    private float defaultVolumeValue = 0.5f;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        LoadSlidersValues();
    }

    public void PlayWinSound()
    {
        sfxSource.PlayOneShot(winSound);
    }

    public void PlayBurnSound()
    {
        sfxSource.PlayOneShot(burnSound);
    }

    public void PlayTrumpSound()
    {
        sfxSource.PlayOneShot(trumpSound);
    }

    public void PlaySelectCardSound()
    {
        sfxSource.PlayOneShot(selectCardSound);
    }

    public void PlayInvalidMoveSound()
    {
        sfxSource.PlayOneShot(invalidMoveSound);
    }

    public void PlayPlayCardSound()
    {
        sfxSource.PlayOneShot(playCardSound);
    }

    public void PlayMenuSong()
    {
        musicSource.Stop();
        musicSource.clip = menuMusic;
        musicSource.Play();
    }

    public void PlayGamesceneSong()
    {
        musicSource.Stop();
        musicSource.clip = gamesceneMusic;
        musicSource.Play();
    }

    public void SaveMusicSliderValue(float value)
    {
        PlayerPrefs.SetFloat("MusicVolume", value);
        PlayerPrefs.Save();
    }

    public void SaveSFXSliderValue(float value)
    {
        PlayerPrefs.SetFloat("SFXVolume", value);
        PlayerPrefs.Save();
    }

    private void LoadSlidersValues()
    {
        if (PlayerPrefs.HasKey("MusicVolume"))
        {
            float savedValue = PlayerPrefs.GetFloat("MusicVolume");
            musicSlider.value = savedValue;
            musicSource.volume = savedValue;
        }
        else
        {
            musicSlider.value = defaultVolumeValue;
            musicSource.volume = defaultVolumeValue;
        }
        if (PlayerPrefs.HasKey("SFXVolume"))
        {
            float savedValue = PlayerPrefs.GetFloat("SFXVolume");
            sfxSlider.value = savedValue;
            sfxSource.volume = savedValue;
        }
        else
        {
            sfxSlider.value = defaultVolumeValue;
            sfxSource.volume = defaultVolumeValue;
        }
    }
}