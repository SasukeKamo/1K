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
    public AudioClip dealCardSound;
    public AudioClip playCardSound;

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

    public void PlayDealCardSound()
    {
        audioSource.PlayOneShot(dealCardSound);
    }

    public void PlayPlayCardSound()
    {
        audioSource.PlayOneShot(playCardSound);
    }
}