﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using Grappler;
using Grappler.Data;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerRecorderController : MonoBehaviour
{
	public PlayerReplayModel PlayerPlaybackData { get { return _playerPlayback; } private set{} }
    public bool Recording { get { return _recording; } private set{} }

    private PlayerController _player;
	private PlayerReplayModel _playerPlayback;
    private float _pollRate = 0.1f; 
    private bool _recording = false;
    private float _timeStarted = 0.0f;
    private float _timePassed = 0.0f;

    void Awake()
    {
    	_player = transform.parent.GetComponent<PlayerController>();
		_playerPlayback = new PlayerReplayModel ();
    }

    void Update()
    {
        if (_recording)
        {
            _timePassed = _timePassed + Time.deltaTime;

            if (_timePassed > _pollRate)
            {
                AddState(_timePassed);
                _timePassed = 0.0f;
            }
        }
    }

	public void StartRecording()
    {
        _playerPlayback = new PlayerReplayModel();
        _timeStarted = Time.time;
        _timePassed = 0.0f;
        _recording = true;
        AddState(_timePassed);
    }

    public void PauseRecording()
    {
        _recording = false;
    }

    public void ResumeRecording()
    {
    	_recording = true;
    }

    public PlayerReplayModel DoneRecording(string levelName, float replayTime)
	{
		AddState(_timePassed);
		_recording = false;
        PlayerReplayModel playerReplayModel = new PlayerReplayModel (PlayerPrefs.GetString(Constants.USERNAME_KEY), levelName, replayTime, _playerPlayback.ReplayData);
        PlayerReplay.Instance.StartCoroutine(PlayerReplay.SavePlayerPlayback(playerReplayModel, (Success) => {
            // placeholder if needed
        }));

        return playerReplayModel;
    }

    void AddState(float deltaTime)
    {
		PlayerStateModel tempPlayerState = new PlayerStateModel(_player.gameObject.transform.position, 
													  _player.PlayerSprite.transform.rotation, 
													  _player.RopeOrigin.transform.rotation,
													  _player.WallHookSprite.transform.position,
													  _player.RopeLineRenderer, 
													  deltaTime);

		_playerPlayback.AddPlayerState(tempPlayerState);
    }
}
