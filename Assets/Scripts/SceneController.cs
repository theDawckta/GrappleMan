using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SceneController : MonoBehaviour
{
    public PlayerController PlayerController;
    public UIController UIController;
    public PlayerRecorderController PlayerRecorder;
	public GhostPlaybackController GhostPlayback;

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

    void Start()
    {
        PlayerRecorder.StartRecording();
		GhostPlayback.StartPlayGhostPlayback();
    }

	public void Init()
    {
        _playerAudio.Play();
        PlayerController.Init();
        Time.timeScale = 1.0f;
	}

	void PlayerCompleted ()
    {
		PlayerRecorder.DoneRecording();
    }

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
    }

    void OnEnable()
    {
    	PlayerController.OnPlayerCompleted += PlayerCompleted;
    }

	void OnDisable()
    {
    	PlayerController.OnPlayerCompleted -= PlayerCompleted;
    }
}
