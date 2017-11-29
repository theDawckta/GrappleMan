using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneController : MonoBehaviour
{
    public PlayerController PlayerController;
    public UIController UIController;

    private AudioSource _playerAudio;
    private AudioClip _song;

    void Awake()
    {
        Application.targetFrameRate = 60;
        _playerAudio = GetComponent<AudioSource>();
        _song = Resources.Load("Songs/BeatOfTheTerror") as AudioClip;

        _playerAudio.clip = _song;
        _playerAudio.loop = true;
        SetVolume(UIController.Volume.value);
    }

    public void Init()
    {
        _playerAudio.Play();
        PlayerController.Init();
        Time.timeScale = 1.0f;
    }

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
    }
}
