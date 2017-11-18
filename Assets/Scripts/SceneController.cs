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
        PlayerController.HookPlayerInput.InputActive = false;
        Time.timeScale = 1.0f;
    }

    public void OnPlayerStarted()
    {
        PlayerController.HookPlayerInput.InputActive = true;
    }

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
    }

    void PlayerControllerOnPlayerDied()
    {
        UIController.EndGame();
        Time.timeScale = 0.0f;
        PlayerController.HookPlayerInput.InputActive = false;
    }

    void PlayerControllerOnPlayerWon()
    {
        // just leaving this here, not sure if the player will be able to actually win
    }

    void OnDestroy()
    {
        PlayerController.OnPlayerDied -= PlayerControllerOnPlayerDied;
    }
}
