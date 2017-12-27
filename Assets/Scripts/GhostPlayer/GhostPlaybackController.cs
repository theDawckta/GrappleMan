using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Grappler.DataModel;
using UnityEngine;

public class GhostPlaybackController : MonoBehaviour 
{
    public delegate void OnGhostCompletedEvent(GhostPlaybackController ghost, PlayerPlaybackModel playerPlayback);
    public event OnGhostCompletedEvent OnGhostCompleted;

    private GhostController _ghostPlayer;
	private PlayerStateModel _tempPlayerState;
	private Vector3[] _lerpFromLineRendererPositions;
	private int _lerpFromLineRendererPositionCount;
	private Vector3 _lerpFromPosition;
	private Quaternion _lerpFromRotation;
	private Quaternion _lerpFromShoulderRotation;
	private Vector3 _lerpFromWallHookPosition;
	private bool _playing = false;
	private float _timePassed = 0.0f;
    private PlayerPlaybackModel _playerPlaybackModel;

	void Awake () 
	{
		_ghostPlayer = transform.GetComponent<GhostController>();
	}

    void Start()
    {
        _ghostPlayer.RopeLineRenderer.enabled = false;
    }

    public void StartPlayGhostPlayback(PlayerPlaybackModel playerPlaybackModel)
	{
         _timePassed = 0.0f;

		if(playerPlaybackModel.HasStates)
		{
            transform.position = playerPlaybackModel.StartingPosition;
			_playing = true;
            _ghostPlayer.FadeIn(0.3f);
            StartCoroutine(PlayGhostPlayback(playerPlaybackModel));
		}
		else
		{
            Debug.Log("RECIEVED EMPTY STATE");
		}
	}

	IEnumerator PlayGhostPlayback(PlayerPlaybackModel playerPlaybackModel)
	{
        _playerPlaybackModel = playerPlaybackModel;
		int previousPositionCount = _ghostPlayer.RopeLineRenderer.positionCount;
        bool RemoveLastLineRendererPosition = false;
        _tempPlayerState = playerPlaybackModel.GetNextState();

		_lerpFromPosition = _ghostPlayer.transform.position;
		_lerpFromRotation = _ghostPlayer.GhostPlayerSprite.transform.rotation;
		_lerpFromShoulderRotation = _ghostPlayer.RopeOrigin.transform.rotation;
		_lerpFromWallHookPosition = _ghostPlayer.WallHookSprite.transform.position;
		_lerpFromLineRendererPositionCount = _ghostPlayer.RopeLineRenderer.positionCount;
        _lerpFromLineRendererPositions = new Vector3[_lerpFromLineRendererPositionCount];
        _ghostPlayer.RopeLineRenderer.GetPositions(_lerpFromLineRendererPositions);
        _ghostPlayer.RopeLineRenderer.positionCount = _tempPlayerState.RopeLineRendererPositions.Length;
		
		_ghostPlayer.RopeLineRenderer.SetPositions(_tempPlayerState.RopeLineRendererPositions);

        if (_ghostPlayer.RopeLineRenderer.positionCount > 1)
			_ghostPlayer.RopeLineRenderer.enabled = true;

        //Handle lerp points for firing, bending around a corner, swinging back from a corner, and coming back to the origin
        if (previousPositionCount == 0 && _tempPlayerState.RopeLineRendererPositions.Length > 1)
        {
            _lerpFromLineRendererPositions = new Vector3[] { _ghostPlayer.RopeOrigin.transform.position, _ghostPlayer.RopeOrigin.transform.position };
        }
        else if(_tempPlayerState.RopeLineRendererPositions.Length > _lerpFromLineRendererPositionCount)
		{
            _lerpFromLineRendererPositions[_lerpFromLineRendererPositions.Length - 2] = _tempPlayerState.RopeLineRendererPositions[_tempPlayerState.RopeLineRendererPositions.Length - 2];
			_lerpFromLineRendererPositions[_lerpFromLineRendererPositions.Length - 1] = _ghostPlayer.RopeOrigin.transform.position;
		}
		else if (_tempPlayerState.RopeLineRendererPositions.Length < _lerpFromLineRendererPositionCount)
		{
            if (_lerpFromLineRendererPositionCount > 2)
            {
                List<Vector3> tempLineRendererPositions = new List<Vector3>();

                _ghostPlayer.RopeLineRenderer.positionCount = _lerpFromLineRendererPositionCount;
                tempLineRendererPositions = _tempPlayerState.RopeLineRendererPositions.ToList();
                tempLineRendererPositions.Insert(tempLineRendererPositions.Count, tempLineRendererPositions[tempLineRendererPositions.Count - 2]);
                tempLineRendererPositions.RemoveAt(tempLineRendererPositions.Count - 1);
                tempLineRendererPositions.Insert(tempLineRendererPositions.Count - 1, _lerpFromLineRendererPositions[_lerpFromLineRendererPositions.Length - 2]);
                _tempPlayerState.RopeLineRendererPositions = tempLineRendererPositions.ToArray();
                _tempPlayerState.WallHookPosition = tempLineRendererPositions[0];
                RemoveLastLineRendererPosition = true;
            }
            else if (_lerpFromLineRendererPositionCount > 1)
            {
                _ghostPlayer.RopeLineRenderer.enabled = false;
            }
        }

        _timePassed = 0.0f;
		while (_playing && _timePassed < _tempPlayerState.DeltaTime)
		{
			float percentageComplete = _timePassed / _tempPlayerState.DeltaTime;
			_ghostPlayer.transform.position = Vector3.Lerp(_lerpFromPosition, _tempPlayerState.BodyPosition, percentageComplete);
			_ghostPlayer.GhostPlayerSprite.transform.rotation = Quaternion.Lerp(_lerpFromRotation, _tempPlayerState.BodyRotation, percentageComplete);
			_ghostPlayer.RopeOrigin.transform.rotation = Quaternion.Lerp(_lerpFromShoulderRotation, _tempPlayerState.ShoulderRotation, percentageComplete);
			_ghostPlayer.WallHookSprite.transform.position = Vector3.Lerp(_lerpFromWallHookPosition, _tempPlayerState.WallHookPosition, percentageComplete);

            // lerp last 2 values of lineRenderer, lerp the first value toward the WallHookSprite
			if (_tempPlayerState.RopeLineRendererPositions.Length > 1)
            {
                _ghostPlayer.RopeLineRenderer.SetPosition(_tempPlayerState.RopeLineRendererPositions.Length - 1,
                                                          Vector3.Lerp(_lerpFromLineRendererPositions[_lerpFromLineRendererPositions.Length - 1],
                                                                       _tempPlayerState.RopeLineRendererPositions[_tempPlayerState.RopeLineRendererPositions.Length - 1],
                                                                       percentageComplete));

                _ghostPlayer.RopeLineRenderer.SetPosition(_tempPlayerState.RopeLineRendererPositions.Length - 2,
                                                          Vector3.Lerp(_lerpFromLineRendererPositions[_lerpFromLineRendererPositions.Length - 2],
                                                                       _tempPlayerState.RopeLineRendererPositions[_tempPlayerState.RopeLineRendererPositions.Length - 2],
                                                                       percentageComplete));

				if (_lerpFromLineRendererPositions[_lerpFromLineRendererPositions.Length - 2] != _ghostPlayer.RopeOrigin.transform.position)
				{
	                Quaternion grappleShoulderRotation = Quaternion.LookRotation(_lerpFromLineRendererPositions[_lerpFromLineRendererPositions.Length - 2] - _ghostPlayer.RopeOrigin.transform.position, Vector3.back);
	                grappleShoulderRotation.x = 0.0f;
	                grappleShoulderRotation.y = 0.0f;
	                _tempPlayerState.ShoulderRotation = grappleShoulderRotation;
                }

                if (_tempPlayerState.RopeLineRendererPositions.Length > 2)
                {
                    _ghostPlayer.RopeLineRenderer.SetPosition(0, Vector3.Lerp(_lerpFromWallHookPosition, _tempPlayerState.WallHookPosition, percentageComplete));
                } 
            }

            _timePassed = _timePassed + Time.deltaTime;
			yield return null;
		}

		_ghostPlayer.transform.position = _tempPlayerState.BodyPosition;
		_ghostPlayer.GhostPlayerSprite.transform.rotation = _tempPlayerState.BodyRotation;
		_ghostPlayer.WallHookSprite.transform.position = _tempPlayerState.WallHookPosition;

        if (_tempPlayerState.RopeLineRendererPositions.Length > 1)
        {
            _ghostPlayer.RopeLineRenderer.SetPosition(_ghostPlayer.RopeLineRenderer.positionCount - 1,
                                                      _tempPlayerState.RopeLineRendererPositions[_tempPlayerState.RopeLineRendererPositions.Length - 1]);
            _ghostPlayer.RopeLineRenderer.SetPosition(_ghostPlayer.RopeLineRenderer.positionCount - 2,
                                                      _tempPlayerState.RopeLineRendererPositions[_tempPlayerState.RopeLineRendererPositions.Length - 2]);
            Quaternion grappleShoulderRotation = Quaternion.LookRotation(_ghostPlayer.RopeLineRenderer.GetPosition(_ghostPlayer.RopeLineRenderer.positionCount - 2) - _tempPlayerState.RopeLineRendererPositions[_tempPlayerState.RopeLineRendererPositions.Length - 1], Vector3.back);
            grappleShoulderRotation.x = 0.0f;
            grappleShoulderRotation.y = 0.0f;
            _tempPlayerState.ShoulderRotation = grappleShoulderRotation;

            if (_tempPlayerState.RopeLineRendererPositions.Length > 2)
                _ghostPlayer.RopeLineRenderer.SetPosition(0, _tempPlayerState.WallHookPosition);
        }

        if (RemoveLastLineRendererPosition)
        {
            _ghostPlayer.RopeLineRenderer.positionCount = _ghostPlayer.RopeLineRenderer.positionCount - 1;
            _ghostPlayer.RopeLineRenderer.SetPosition(_ghostPlayer.RopeLineRenderer.positionCount - 1, _ghostPlayer.RopeOrigin.transform.position);
            RemoveLastLineRendererPosition = false;
        }

        if (playerPlaybackModel.HasStates)
            yield return StartCoroutine(PlayGhostPlayback(playerPlaybackModel));
        else
        {
            _ghostPlayer.FadeOut(1.0f);
            Invoke("GhostCompleted", 1.0f);
            _playing = false;
        }
	}

    void GhostCompleted()
    {
        if (OnGhostCompleted != null)
        {
            OnGhostCompleted(this, _playerPlaybackModel);
        }
    }
}
