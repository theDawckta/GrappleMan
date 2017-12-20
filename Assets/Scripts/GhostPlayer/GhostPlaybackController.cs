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
	private Vector3[] _currentGhostLineRendererPositions;
	private Vector3 _currentGhostPosition;
	private Quaternion _currentGhostRotation;
	private Quaternion _currentGhostShoulderRotation;
	private Vector3 _currentGhostWallHookPosition;
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

		_currentGhostPosition = _ghostPlayer.transform.position;
		_currentGhostRotation = _ghostPlayer.GhostPlayerSprite.transform.localRotation;
		_currentGhostShoulderRotation = _ghostPlayer.RopeOrigin.transform.localRotation;
		_currentGhostWallHookPosition = _ghostPlayer.WallHookSprite.transform.position;
		_ghostPlayer.RopeLineRenderer.enabled = _tempPlayerState.RopeLineRendererEnabled;
		_currentGhostLineRendererPositions = new Vector3[_tempPlayerState.RopeLineRendererPositions.Length];
		_ghostPlayer.RopeLineRenderer.positionCount = _tempPlayerState.RopeLineRendererPositions.Length;
		_ghostPlayer.RopeLineRenderer.GetPositions(_currentGhostLineRendererPositions);
		_ghostPlayer.RopeLineRenderer.SetPositions(_currentGhostLineRendererPositions);

		_timePassed = 0.0f;
		Debug.Log("starting loop");
		while(_playing && _timePassed < _tempPlayerState.DeltaTime)
		{
			//Debug.Log("TOTAL TIME PASSED: " + _totalTimePassed + "    SYSTEM TIME PASSED: " + Time.fixedTime);
			float percentageComplete = _timePassed / _tempPlayerState.DeltaTime;
			Debug.Log(_ghostPlayer.transform.position);
			_ghostPlayer.transform.position = Vector3.Lerp(_currentGhostPosition, _tempPlayerState.BodyPosition, percentageComplete);
			_ghostPlayer.GhostPlayerSprite.transform.localRotation = Quaternion.Lerp(_currentGhostRotation, _tempPlayerState.BodyRotation, percentageComplete);
			_ghostPlayer.RopeOrigin.transform.localRotation = Quaternion.Lerp(_currentGhostShoulderRotation, _tempPlayerState.ShoulderRotation, percentageComplete);
			_ghostPlayer.WallHookSprite.transform.position = Vector3.Lerp(_currentGhostWallHookPosition, _tempPlayerState.WallHookPosition, percentageComplete);

			// lerp last 2 values of lineRenderer
			if(_tempPlayerState.RopeLineRendererPositions.Length > 1)
			{
				_ghostPlayer.RopeLineRenderer.SetPosition(_tempPlayerState.RopeLineRendererPositions.Length - 1,
														  Vector3.Lerp(_currentGhostLineRendererPositions[_currentGhostLineRendererPositions.Length - 1], 
													                   _tempPlayerState.RopeLineRendererPositions[_tempPlayerState.RopeLineRendererPositions.Length - 1], 
													                   percentageComplete));

				_ghostPlayer.RopeLineRenderer.SetPosition(_tempPlayerState.RopeLineRendererPositions.Length - 2,
														  Vector3.Lerp(_currentGhostLineRendererPositions[_currentGhostLineRendererPositions.Length - 2], 
													                   _tempPlayerState.RopeLineRendererPositions[_tempPlayerState.RopeLineRendererPositions.Length - 2], 
													                   percentageComplete));
			}

			_timePassed = _timePassed + Time.deltaTime;
			Debug.Log("while loop ran once");
			yield return null;
		}

		_ghostPlayer.transform.position = _tempPlayerState.BodyPosition;
		_ghostPlayer.GhostPlayerSprite.transform.localRotation = _tempPlayerState.BodyRotation;
		_ghostPlayer.RopeOrigin.transform.localRotation = _tempPlayerState.ShoulderRotation;
		_ghostPlayer.WallHookSprite.transform.position = _tempPlayerState.WallHookPosition;

		if(_tempPlayerState.RopeLineRendererPositions.Length > 1)
		{
			_ghostPlayer.RopeLineRenderer.SetPosition(_ghostPlayer.RopeLineRenderer.positionCount - 1,
													  _tempPlayerState.RopeLineRendererPositions[_tempPlayerState.RopeLineRendererPositions.Length - 1]);
			_ghostPlayer.RopeLineRenderer.SetPosition(_ghostPlayer.RopeLineRenderer.positionCount - 2,
													  _tempPlayerState.RopeLineRendererPositions[_tempPlayerState.RopeLineRendererPositions.Length - 2]);
		}

		if(_playerPlayback.HasStates)
		{
			Debug.Log("ending loop");
			yield return StartCoroutine(PlayGhostPlayback());
		}
		else
			_playing = false;
		Debug.Log("playback ended");
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
