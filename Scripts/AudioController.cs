using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioController : MonoBehaviour
{
    public static AudioController Instance { get; private set; }
    public AudioClip explosionSound;
    public AudioClip squareLetGoSound;
    public AudioClip merge2Sound;
    public AudioClip merge3Sound;
    public AudioClip merge4Sound;
    public AudioClip merge5Sound;
    public AudioClip gameEndWarningSound;
    public Toggle audioToggle;
    private AudioSource audioSource;
    private const string isOnMute = "isOnssMute";


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
            audioSource = GetComponent<AudioSource>();
            audioSource.mute = Convert.ToBoolean(PlayerPrefs.GetInt(isOnMute));
            if (audioSource.mute) audioToggle.isOn = false;
        }
    }

    public void PlayExplosionSound()
    {
        audioSource.PlayOneShot(explosionSound);
    }

    public void PlaySquareLetGoSound()
    {
        audioSource.PlayOneShot(squareLetGoSound);
    }

    public void PlayGameEndWarningSound()
    {
        audioSource.PlayOneShot(gameEndWarningSound);
    }


    /// <summary>
    /// Accepts values 2 to 5.
    /// </summary>
    /// <param name="value">Value of square after merge.</param>
    public void PlayMergeSound(int value)
    {
        switch (value)
        {
            case int n when (n == 2):
                audioSource.PlayOneShot(merge2Sound);
                break;
            case int n when (n == 3):
                audioSource.PlayOneShot(merge3Sound);
                break;
            case int n when (n == 4):
                audioSource.PlayOneShot(merge4Sound);
                break;
            case int n when (n == 5):
                audioSource.PlayOneShot(merge5Sound);
                break;
        }
    }

    public void MuteSounds()
    {
        audioSource.mute = !audioToggle.isOn;
        PlayerPrefs.SetInt(isOnMute, Convert.ToInt16(!audioToggle.isOn));
    }
}
