using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Grappler.DataModel;
using UnityEngine;

public class GhostPlaybackController : MonoBehaviour 
{
	private GhostController _ghostPlayer;
	private PlayerPlayback _playerPlayback;
	private PlayerState _tempPlayerState;
	private Vector3[] _currentLineRendererPositions;
	private int _currentLineRendererPositionCount;
	private Vector3 _currentPosition;
	private Quaternion _currentRotation;
	private Quaternion _currentShoulderRotation;
	private Vector3 _currentWallHookPosition;
	private bool _playing = false;
	private float _timePassed = 0.0f;
	private float _totalTimePassed = 0.0f;

	void Awake () 
	{
		_ghostPlayer = transform.parent.GetComponent<GhostController>();
		_playerPlayback = LoadPlayerPlaybackData();
	}

	public void StartPlayGhostPlayback()
	{
		if(_playerPlayback.HasStates)
		{
			_ghostPlayer.gameObject.SetActive(true);
			_ghostPlayer.transform.position = _playerPlayback.StartingState.BodyPosition;
			_ghostPlayer.GhostPlayerSprite.transform.rotation = _playerPlayback.StartingState.BodyRotation;
			_playing = true;
			StartCoroutine(PlayGhostPlayback());
		}
		else
		{
			_ghostPlayer.gameObject.SetActive(false);
			gameObject.SetActive(false);
		}
	}

	IEnumerator PlayGhostPlayback()
	{
		_tempPlayerState = _playerPlayback.GetNextState();

		_currentPosition = _ghostPlayer.transform.position;
		_currentRotation = _ghostPlayer.GhostPlayerSprite.transform.rotation;
		_currentShoulderRotation = _ghostPlayer.RopeOrigin.transform.rotation;
		_currentWallHookPosition = _ghostPlayer.WallHookSprite.transform.position;
		_ghostPlayer.RopeLineRenderer.enabled = _tempPlayerState.RopeLineRendererEnabled;
		_currentLineRendererPositionCount = _ghostPlayer.RopeLineRenderer.positionCount;
		_currentLineRendererPositions = new Vector3[_tempPlayerState.RopeLineRendererPositions.Length];
		_ghostPlayer.RopeLineRenderer.positionCount = _tempPlayerState.RopeLineRendererPositions.Length;
		_ghostPlayer.RopeLineRenderer.GetPositions(_currentLineRendererPositions);
		_ghostPlayer.RopeLineRenderer.SetPositions(_tempPlayerState.RopeLineRendererPositions);

		if(_tempPlayerState.RopeLineRendererPositions.Length > 2)
			Debug.Log("STOP");
		if(_tempPlayerState.RopeLineRendererPositions.Length > _currentLineRendererPositionCount)
		{
			int difference = _tempPlayerState.RopeLineRendererPositions.Length - _currentLineRendererPositionCount;
			for (int i = _ghostPlayer.RopeLineRenderer.positionCount - difference; i < _tempPlayerState.RopeLineRendererPositions.Length; i++)
				_currentLineRendererPositions[i] = _tempPlayerState.RopeLineRendererPositions[i];
		}

		_timePassed = 0.0f;
		while(_playing && _timePassed < _tempPlayerState.DeltaTime)
		{
			float percentageComplete = _timePassed / _tempPlayerState.DeltaTime;
			Debug.Log(_ghostPlayer.transform.position);
			_ghostPlayer.transform.position = Vector3.Lerp(_currentPosition, _tempPlayerState.BodyPosition, percentageComplete);
			_ghostPlayer.GhostPlayerSprite.transform.rotation = Quaternion.Lerp(_currentRotation, _tempPlayerState.BodyRotation, percentageComplete);
			_ghostPlayer.RopeOrigin.transform.rotation = Quaternion.Lerp(_currentShoulderRotation, _tempPlayerState.ShoulderRotation, percentageComplete);
			_ghostPlayer.WallHookSprite.transform.position = Vector3.Lerp(_currentWallHookPosition, _tempPlayerState.WallHookPosition, percentageComplete);

            // lerp last 2 values of lineRenderer
            if (_tempPlayerState.RopeLineRendererPositions.Length > 1)
            {
				Debug.Log(_currentLineRendererPositions[_currentLineRendererPositions.Length - 1]);
                _ghostPlayer.RopeLineRenderer.SetPosition(_tempPlayerState.RopeLineRendererPositions.Length - 1,
                                                          Vector3.Lerp(_currentLineRendererPositions[_currentLineRendererPositions.Length - 1],
                                                                       _tempPlayerState.RopeLineRendererPositions[_tempPlayerState.RopeLineRendererPositions.Length - 1],
                                                                       percentageComplete));

                _ghostPlayer.RopeLineRenderer.SetPosition(_tempPlayerState.RopeLineRendererPositions.Length - 2,
                                                          Vector3.Lerp(_currentLineRendererPositions[_currentLineRendererPositions.Length - 2],
                                                                       _tempPlayerState.RopeLineRendererPositions[_tempPlayerState.RopeLineRendererPositions.Length - 2],
                                                                       percentageComplete));
            }

            _timePassed = _timePassed + Time.deltaTime;
			yield return null;
		}

		_ghostPlayer.transform.position = _tempPlayerState.BodyPosition;
		_ghostPlayer.GhostPlayerSprite.transform.rotation = _tempPlayerState.BodyRotation;
		_ghostPlayer.RopeOrigin.transform.rotation = _tempPlayerState.ShoulderRotation;
		_ghostPlayer.WallHookSprite.transform.position = _tempPlayerState.WallHookPosition;

        if (_tempPlayerState.RopeLineRendererPositions.Length > 1)
        {
            _ghostPlayer.RopeLineRenderer.SetPosition(_ghostPlayer.RopeLineRenderer.positionCount - 1,
                                                      _tempPlayerState.RopeLineRendererPositions[_tempPlayerState.RopeLineRendererPositions.Length - 1]);
            _ghostPlayer.RopeLineRenderer.SetPosition(_ghostPlayer.RopeLineRenderer.positionCount - 2,
                                                      _tempPlayerState.RopeLineRendererPositions[_tempPlayerState.RopeLineRendererPositions.Length - 2]);
        }

        if (_playerPlayback.HasStates)
			yield return StartCoroutine(PlayGhostPlayback());
		else
			_playing = false;

		yield return null;
	}

	private PlayerPlayback LoadPlayerPlaybackData()
    {
		string playerDataFilePath = Path.Combine(Application.persistentDataPath, "PlayerData/playerGhostData.json");

		if(File.Exists(playerDataFilePath))
			return JsonUtility.FromJson<PlayerPlayback>(File.ReadAllText(playerDataFilePath));
        else
            return new PlayerPlayback(new PlayerState());
    }
}
