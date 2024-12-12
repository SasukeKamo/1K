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

    public AudioSource audioSource;

    public AudioClip winSound;
    public AudioClip invalidMoveSound;
    public AudioClip playCardSound;
    public AudioClip selectCardSound;
    public AudioClip trumpSound;
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

        audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void PlayWinSound()
    {
        audioSource.PlayOneShot(winSound);
    }

    public void PlayTrumpSound()
    {
        audioSource.PlayOneShot(trumpSound);
    }

    public void PlaySelectCardSound()
    {
        audioSource.PlayOneShot(selectCardSound);
    }

    public void PlayInvalidMoveSound()
    {
        audioSource.PlayOneShot(invalidMoveSound);
    }

    public void PlayPlayCardSound()
    {
        audioSource.PlayOneShot(playCardSound);
    }

    public void PlayMenuSong()
    {
        audioSource.Stop();
        audioSource.clip = menuMusic;
        audioSource.Play();
    }

    public void PlayGamesceneSong()
    {
        audioSource.Stop();
        audioSource.clip = gamesceneMusic;
        audioSource.Play();
    }
}