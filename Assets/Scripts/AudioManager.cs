using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public AudioClip winSound;
    public AudioClip invalidMoveSound;
    public AudioClip playCardSound;
    public AudioClip selectCardSound;
    public AudioClip trumpSound;
    public AudioClip burnSound;
    public AudioClip gamesceneMusic;
    public AudioClip menuMusic;

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
}