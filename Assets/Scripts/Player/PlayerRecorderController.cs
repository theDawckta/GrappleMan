﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using Grappler.DataModel;
using UnityEngine;

public class PlayerRecorderController : MonoBehaviour
{
	public PlayerPlaybackModel PlayerPlaybackData {get {return _playerPlayback;} set{}}

    private PlayerController _player;
    private PlayerPlaybackModel _playerPlayback;
    private float _pollRate = 0.05f; 
    private bool _recording = false;
    private float _timePassed = 0.0f;

    void Awake()
    {
    	_player = transform.parent.GetComponent<PlayerController>();
    }

    public void StartRecording()
    {
		PlayerStateModel tempPlayerState = new PlayerStateModel(_player.gameObject.transform.position, 
													  _player.PlayerSprite.transform.rotation, 
													  _player.RopeOrigin.transform.rotation,
													  _player.WallHookSprite.transform.position,
													  _player.RopeLineRenderer, 
													  0.0f);

		_playerPlayback = new PlayerPlaybackModel(tempPlayerState);
        _recording = true;
        StartCoroutine("Record");
    }

    public void PauseRecording()
    {
        _recording = false;
    }

    public void ResumeRecording()
    {
    	_recording = true;
    }

	IEnumerator Record()
    {
        _timePassed = 0.0f;

        while(_recording)
        {
            if(_timePassed > _pollRate)
            {
				AddState(_timePassed);
				_timePassed = 0.0f;
            }
            else
				_timePassed = _timePassed + Time.deltaTime;

            yield return null;
        }
        yield return null;
    }

    public void DoneRecording()
	{
		AddState(_timePassed, true);
		_recording = false;
		StopCoroutine("Record");
    }

    void AddState(float deltaTime, bool lastState = false)
    {
		PlayerStateModel tempPlayerState = new PlayerStateModel(_player.gameObject.transform.position, 
													  _player.PlayerSprite.transform.rotation, 
													  _player.RopeOrigin.transform.rotation,
													  _player.WallHookSprite.transform.position,
													  _player.RopeLineRenderer, 
													  deltaTime);

		_playerPlayback.AddPlayerState(tempPlayerState, lastState);
    }
}
