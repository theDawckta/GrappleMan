using System;
using System.Linq;
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
		_ghostPlayer.RopeLineRenderer.enabled = false;
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
		int previousPositionCount = _ghostPlayer.RopeLineRenderer.positionCount;
		bool lastFrame = false;
		_tempPlayerState = _playerPlayback.GetNextState();

		_currentPosition = _ghostPlayer.transform.position;
		_currentRotation = _ghostPlayer.GhostPlayerSprite.transform.rotation;
		_currentShoulderRotation = _ghostPlayer.RopeOrigin.transform.rotation;
		_currentWallHookPosition = _ghostPlayer.WallHookSprite.transform.position;
		_currentLineRendererPositionCount = _ghostPlayer.RopeLineRenderer.positionCount;
		_currentLineRendererPositions = new Vector3[_tempPlayerState.RopeLineRendererPositions.Length];
		_ghostPlayer.RopeLineRenderer.positionCount = _tempPlayerState.RopeLineRendererPositions.Length;
		_ghostPlayer.RopeLineRenderer.GetPositions(_currentLineRendererPositions);
		_ghostPlayer.RopeLineRenderer.SetPositions(_tempPlayerState.RopeLineRendererPositions);

		if(_ghostPlayer.RopeLineRenderer.positionCount > 1)
			_ghostPlayer.RopeLineRenderer.enabled = true;

		// Handle lerp points for firing, bending around a corner, swinging back from a corner, and coming back to the origin
		if(previousPositionCount == 0 && _tempPlayerState.RopeLineRendererPositions.Length > 1)
		{
			_currentLineRendererPositions[_currentLineRendererPositions.Length - 2] = _ghostPlayer.RopeOrigin.transform.position;
			_currentLineRendererPositions[_currentLineRendererPositions.Length - 1] = _ghostPlayer.RopeOrigin.transform.position;
		}
		else if(_tempPlayerState.RopeLineRendererPositions.Length > _currentLineRendererPositionCount)
		{
			_currentLineRendererPositions[_currentLineRendererPositions.Length - 2] = _tempPlayerState.RopeLineRendererPositions[_tempPlayerState.RopeLineRendererPositions.Length - 2];
			_currentLineRendererPositions[_currentLineRendererPositions.Length - 1] = _ghostPlayer.RopeOrigin.transform.position;
		}
		else if(previousPositionCount > _tempPlayerState.RopeLineRendererPositions.Length && _ghostPlayer.RopeLineRenderer.positionCount > 0)
		{
			Debug.Log("HIT UNRAVE:");
//			List<Vector3> tempLineRendererPositions = new List<Vector3>();
//
//			_ghostPlayer.RopeLineRenderer.positionCount = _currentLineRendererPositionCount;
//			_ghostPlayer.RopeLineRenderer.SetPositions(_currentLineRendererPositions);
//			tempLineRendererPositions = _tempPlayerState.RopeLineRendererPositions.ToList();
//			tempLineRendererPositions.Insert(tempLineRendererPositions.Count, tempLineRendererPositions[tempLineRendererPositions.Count - 2]);
//			tempLineRendererPositions.RemoveAt(tempLineRendererPositions.Count - 1);
//			tempLineRendererPositions.Insert(tempLineRendererPositions.Count - 1,  _currentLineRendererPositions[_currentLineRendererPositions.Length - 2]);
//			_tempPlayerState.RopeLineRendererPositions = tempLineRendererPositions.ToArray();
		}
		else if(previousPositionCount > 0 && _tempPlayerState.RopeLineRendererPositions.Length == 0)
		{
			_ghostPlayer.RopeLineRenderer.positionCount = 2;
			_currentLineRendererPositions = new Vector3[]{_ghostPlayer.WallHookSprite.transform.position, _ghostPlayer.RopeOrigin.transform.position};
			_ghostPlayer.RopeLineRenderer.SetPositions(_currentLineRendererPositions);
			_tempPlayerState.RopeLineRendererPositions = new Vector3[] {_tempPlayerState.WallHookPosition, _tempPlayerState.WallHookPosition};
			lastFrame = true;
		}

		_timePassed = 0.0f;
		while(_playing && _timePassed < _tempPlayerState.DeltaTime)
		{
			float percentageComplete = _timePassed / _tempPlayerState.DeltaTime;
			_ghostPlayer.transform.position = Vector3.Lerp(_currentPosition, _tempPlayerState.BodyPosition, percentageComplete);
			_ghostPlayer.GhostPlayerSprite.transform.rotation = Quaternion.Lerp(_currentRotation, _tempPlayerState.BodyRotation, percentageComplete);
			_ghostPlayer.RopeOrigin.transform.rotation = Quaternion.Lerp(_currentShoulderRotation, _tempPlayerState.ShoulderRotation, percentageComplete);
			_ghostPlayer.WallHookSprite.transform.position = Vector3.Lerp(_currentWallHookPosition, _tempPlayerState.WallHookPosition, percentageComplete);

            // lerp last 2 values of lineRenderer, lerp the first value toward the WallHookSprite
			if (_tempPlayerState.RopeLineRendererPositions.Length > 1)
            {
                _ghostPlayer.RopeLineRenderer.SetPosition(_tempPlayerState.RopeLineRendererPositions.Length - 1,
                                                          Vector3.Lerp(_currentLineRendererPositions[_currentLineRendererPositions.Length - 1],
                                                                       _tempPlayerState.RopeLineRendererPositions[_tempPlayerState.RopeLineRendererPositions.Length - 1],
                                                                       percentageComplete));

                _ghostPlayer.RopeLineRenderer.SetPosition(_tempPlayerState.RopeLineRendererPositions.Length - 2,
                                                          Vector3.Lerp(_currentLineRendererPositions[_currentLineRendererPositions.Length - 2],
                                                                       _tempPlayerState.RopeLineRendererPositions[_tempPlayerState.RopeLineRendererPositions.Length - 2],
                                                                       percentageComplete));

				_ghostPlayer.RopeLineRenderer.SetPosition(0, Vector3.Lerp(_currentWallHookPosition, _tempPlayerState.WallHookPosition, percentageComplete));
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

		if(lastFrame)
		{
			_ghostPlayer.RopeLineRenderer.positionCount = 0;
			_ghostPlayer.RopeLineRenderer.enabled = false;
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
