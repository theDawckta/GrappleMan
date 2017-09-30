using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneController : MonoBehaviour
{
    public PlayerController _PlayerController;
    public UIController _UIController;
    public GameObject StartPlatform;
    [HideInInspector]
    public bool GameOn = false;

    private AudioSource playerAudio;
    private AudioClip song;

    void Awake()
    {
        playerAudio = GetComponent<AudioSource>();
        song = Resources.Load("Songs/BeatOfTheTerror") as AudioClip;

        playerAudio.clip = song;
        playerAudio.loop = true;
        SetVolume(_UIController.Volume.value);
    }
    void Start()
    {
        _PlayerController.OnPlayerDied += _PlayerController_OnPlayerDied;
        _PlayerController.OnPlayerStarted += _PlayerController_OnPlayerStarted;
        Init();
    }

    void Update()
    {
    }

    public void Init()
    {
        playerAudio.Play();
        StartPlatform.SetActive(true);
        _PlayerController.Init();
        _PlayerController.HookPlayerInput.InputActive = true;
        Time.timeScale = 1.0f;
    }

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
    }

    public void PlayerReady()
    {
        _PlayerController.HookPlayerInput.InputActive = true;
        _UIController.ToggleUIController();
    }

    void _PlayerController_OnPlayerDied()
    {
        _UIController.EndGame();
        Time.timeScale = 0.0f;
        _PlayerController.HookPlayerInput.InputActive = false;
        GameOn = false;
    }

    void _PlayerController_OnPlayerStarted()
    {
        GameOn = true;
        StartPlatform.SetActive(false);
    }

    void _PlayerController_OnPlayerWon()
    {
        // just leaving this here, not sure if the player will be able to actually win
    }

    void OnDestroy()
    {
        _PlayerController.OnPlayerDied -= _PlayerController_OnPlayerDied;
        _PlayerController.OnPlayerStarted -= _PlayerController_OnPlayerStarted;
    }
}
