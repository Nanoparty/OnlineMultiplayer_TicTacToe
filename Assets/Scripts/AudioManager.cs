using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager am;
    public AudioSource audioSource;
    public AudioSource musicPlayer;

    public AudioClip buttonClick1;
    public AudioClip buttonClick2;

    public AudioClip win;
    public AudioClip lose;
    public AudioClip tie;

    public AudioClip menuMusic;
    public AudioClip gameMusic;

    private void Awake()
    {
        if (am != this && am != null)
        {
            GameObject.Destroy(am.gameObject);
        }
        am = this;
        DontDestroyOnLoad(this);
    }

    public void PlayClick1()
    {
        audioSource.PlayOneShot(buttonClick1);
    }

    public void PlayClick2()
    {
        audioSource.PlayOneShot(buttonClick2);
    }

    public void PlayWin()
    {
        audioSource.PlayOneShot(win);
    }

    public void PlayLose()
    {
        audioSource.PlayOneShot(lose);
    }

    public void PlayTie()
    {
        audioSource.PlayOneShot(tie);
    }

    public void PlayMenuMusic()
    {
        if (musicPlayer.clip == menuMusic && musicPlayer.isPlaying) return;

        musicPlayer.clip = menuMusic;
        musicPlayer.loop = true;
        musicPlayer.Play();
    }

    public void PlayGameMusic()
    {
        if (musicPlayer.clip == gameMusic && musicPlayer.isPlaying) return;

        musicPlayer.clip = gameMusic;
        musicPlayer.loop = true;
        musicPlayer.Play();
    }
}
