using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;

namespace Grappler.DataModel
{
	[Serializable]
    public class PlayerPlayback
	{
		public bool HasStates {get {return _state.Count > 0;} private set{}}
		public PlayerState StartingState {get {return _startingState;} private set{}}

		[SerializeField]
		private PlayerState _startingState;
		[SerializeField]
		private List<PlayerState> _state;

        public PlayerPlayback(PlayerState startingState)
        {
			_startingState = startingState;
        	_state = new List<PlayerState>();
        }

        public void AddPlayerState(PlayerState playerState)
        {
			_state.Add(playerState);
        }

        public PlayerState GetNextState()
        {
        	PlayerState tempPlayerState = _state[0];
			_state.RemoveAt(0);
			return tempPlayerState;
        }
    }

	[Serializable]
    public class PlayerState
	{
		public Vector3 BodyPosition;
		public Quaternion BodyRotation;
		public Quaternion ShoulderRotation;
		public Vector3 WallHookPosition;	
		public bool RopeLineRendererEnabled;
		public Vector3[] RopeLineRendererPositions;
    	public float DeltaTime;

    	public PlayerState()
    	{
			BodyPosition = Vector3.zero;
			BodyRotation = Quaternion.identity;
			ShoulderRotation = Quaternion.identity;
			WallHookPosition = Vector3.zero;		
			RopeLineRendererEnabled = false;
    		DeltaTime = 0.0f;
    	}

		public PlayerState(Vector3 bodyPosition, Quaternion bodyRotation, Quaternion shoulderRotation, Vector3 wallHookPosition, LineRenderer lineRenderer, float time)
    	{
			BodyPosition = bodyPosition;
			BodyRotation = bodyRotation;
			ShoulderRotation = shoulderRotation;
			WallHookPosition = wallHookPosition;		
			RopeLineRendererEnabled = lineRenderer.enabled;
			RopeLineRendererPositions = new Vector3[lineRenderer.positionCount];
			lineRenderer.GetPositions(RopeLineRendererPositions);
			DeltaTime = time;
    	}
    }
}
