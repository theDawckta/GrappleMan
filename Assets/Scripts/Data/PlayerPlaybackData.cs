﻿using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using Grappler.Constants;

namespace Grappler.DataModel
{
    public class PlayerPlaybackController
    {
        public static int MaxNumOfRecords { get { return _numOfRecords; } private set { } }

        private static string _playerCompletedDataLocation = string.Format("{0}/{1}", Application.persistentDataPath, "PlayerDataCompleted");
        private static string _playerDiedDataLocation = string.Format("{0}/{1}", Application.persistentDataPath, "PlayerDataDied");
        private static string[] _playerDataLocations = new string[] { _playerCompletedDataLocation, _playerDiedDataLocation };
        private static string _playerDataFileName = string.Format("/{0}_{1}_", "User", "GhostData");
        private static int _numOfRecords = 6;

        public static void Init()
        {
            if (PlayerPrefs.HasKey(Constants.Constants.GHOST_RECORDS))
                _numOfRecords = PlayerPrefs.GetInt(Constants.Constants.GHOST_RECORDS);
            else
                PlayerPrefs.SetInt(Constants.Constants.GHOST_RECORDS, _numOfRecords);

            for (int i = 0; i < _playerDataLocations.Length; i++)
            {
                Directory.CreateDirectory(_playerDataLocations[i]);

                // trim any files greater than _numOfRecords
                int tempNumOfRecords = _numOfRecords;
                while (File.Exists(_playerDataLocations[i] + _playerDataFileName + tempNumOfRecords + ".json"))
                {
                    File.Delete(_playerDataLocations[i] + _playerDataFileName + tempNumOfRecords + ".json");
                    tempNumOfRecords = tempNumOfRecords + 1;
                }
            }
        }

        public static void ClearData()
        {
            for (int i = 0; i < _playerDataLocations.Length; i++)
            {
                Directory.CreateDirectory(_playerDataLocations[i]);

                // delete all files
                int tempNumOfRecords = 0;
                while (File.Exists(_playerDataLocations[i] + _playerDataFileName + tempNumOfRecords + ".json"))
                {
                    File.Delete(_playerDataLocations[i] + _playerDataFileName + tempNumOfRecords + ".json");
                    tempNumOfRecords = tempNumOfRecords + 1;
                }
            }
        }

        public static void SetNumOfRecords(int numOfRecords)
        {
            _numOfRecords = numOfRecords;
            PlayerPrefs.SetInt(Constants.Constants.GHOST_RECORDS, _numOfRecords);
        }

        public static void ProcessPlayerPlayback(PlayerPlaybackModel playerPlayback, bool playerCompleted)
        {
            string playerDataLocation;

            if (playerCompleted)
                playerDataLocation = _playerCompletedDataLocation;
            else
                playerDataLocation = _playerDiedDataLocation;

            List<PlayerPlaybackModel> playerPlaybackModels = GetPlayerPlaybackLocal(_numOfRecords);

            for (int i = 0; i < _numOfRecords; i++)
            {
                if (playerPlaybackModels.Count == 0)
                {
                    SavePlayerPlaybackLocal(playerPlayback, i, playerDataLocation);
                    return;
                }
                else if (i < playerPlaybackModels.Count)
                {
                    if (playerPlayback.PlaybackTime < playerPlaybackModels[i].PlaybackTime)
                    {
                        SavePlayerPlaybackLocal(playerPlayback, i, playerDataLocation);
                        return;
                    }
                }
                if (i == playerPlaybackModels.Count)
                {
                    SavePlayerPlaybackLocal(playerPlayback, i, playerDataLocation);
                    return;
                }
            }
        }

        static void SavePlayerPlaybackLocal(PlayerPlaybackModel playerPlayback, int insertIndex, string playerDataLocation)
        {
            string lastItemFilePath;
            string nextToLastFilePath;
            string[] tempFiles;
            int numOfExistingModels;

            // serialize playerPlayback
            var bytes = System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(playerPlayback));

            // find out how many files we currently have
            tempFiles = Directory.GetFiles(playerDataLocation, "*.json", SearchOption.TopDirectoryOnly);
            numOfExistingModels = tempFiles.Length;

            try
            {
                // copy files down from insertIndex to make room for insertIndex item
                for (int i = numOfExistingModels; i > insertIndex; i--)
                {
                    lastItemFilePath = playerDataLocation + _playerDataFileName + (i - 1) + ".json";
                    nextToLastFilePath = playerDataLocation + _playerDataFileName + i + ".json";
                    System.IO.File.Copy(lastItemFilePath, nextToLastFilePath, true);
                }
                // copy new file to insertIndex
                File.WriteAllBytes(playerDataLocation + _playerDataFileName + insertIndex + ".json", bytes);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        static void SavePlayerPlaybackServer(PlayerPlaybackModel playerPlayback)
        {
            // serialize playerPlayback
            var bytes = System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(playerPlayback));

            
        }

        public static List<PlayerPlaybackModel> GetPlayerPlaybackLocal(int numOfRecords)
        {
            List<PlayerPlaybackModel> playerPlaybacks = new List<PlayerPlaybackModel>();

            for (int i = 0; i < numOfRecords; i++)
            {
                string playerDataFilePath = _playerCompletedDataLocation + _playerDataFileName + i + ".json";

                if (File.Exists(playerDataFilePath))
                    playerPlaybacks.Add(JsonUtility.FromJson<PlayerPlaybackModel>(File.ReadAllText(playerDataFilePath)));
                else
                    break;
            }

            return playerPlaybacks;
        }

        // Implement server data, example call below
        //		string jsonPlayerPlayback;
        //		StartCoroutine (GetPlayerPlaybackDataServer (_thingName, (value)=>{returnData = value} ));
        // Function below is just example
        public IEnumerator GetPlayerPlaybackDataServer(System.Action<string> callback)
        {
            string URLString = "http://XXXXX/Services/GetPropertyValues";
            WWWForm form = new WWWForm();
            form.AddBinaryData("binary", new byte[1]);
            var headers = form.headers;
            headers.Remove("Content-Type");
            headers.Add("appKey", "XXX-XXX-XXX");
            headers.Add("Content-Type", "application/json");
            headers.Add("Accept", "application/json");
            WWW www = new WWW(URLString, form.data, headers);
            yield return www;

            callback(www.text);
        }
    }

    [Serializable]
    public class PlayerPlaybackModel
    {
        public bool HasStates { get { return _stateIndex < _state.Count; } private set { } }
        public Vector3 StartingPosition  { get {  return (_state.Count > 0) ? _state[0].BodyPosition : Vector3.zero; } private set{} }
		public float PlaybackTime { get { return _time; } private set {} }
        
		[SerializeField]
		private List<PlayerStateModel> _state;
		[SerializeField]
		private float _time;

        private int _stateIndex = 0;

        public PlayerPlaybackModel()
        {
        	_state = new List<PlayerStateModel>();
        }

        public void AddPlayerState(PlayerStateModel playerState, bool final = false)
        {
			_state.Add(playerState);
			if (final)
			{
				for (int i = 0; i < _state.Count; i++)
					_time = _time + _state[i].DeltaTime;
			}
        }

        public PlayerStateModel GetNextState()
        {
        	PlayerStateModel tempPlayerState = _state[_stateIndex];
            _stateIndex++;
            if (_stateIndex > _state.Count)
                _stateIndex = 0;
			return tempPlayerState;
        }

        public void SetStateIndex(int stateIndex)
        {
            _stateIndex = stateIndex;
        }
    }

	[Serializable]
    public class PlayerStateModel
	{
		public Vector3 BodyPosition;
		public Quaternion BodyRotation;
		public Quaternion ShoulderRotation;
		public Vector3 WallHookPosition;	
		public Vector3[] RopeLineRendererPositions;
    	public float DeltaTime;

    	public PlayerStateModel()
    	{
			BodyPosition = Vector3.zero;
			BodyRotation = Quaternion.identity;
			ShoulderRotation = Quaternion.identity;
			WallHookPosition = Vector3.zero;
    		DeltaTime = 0.0f;
    	}

		public PlayerStateModel(Vector3 bodyPosition, Quaternion bodyRotation, Quaternion shoulderRotation, Vector3 wallHookPosition, LineRenderer lineRenderer, float time)
    	{
			BodyPosition = bodyPosition;
			BodyRotation = bodyRotation;
			ShoulderRotation = shoulderRotation;
			WallHookPosition = wallHookPosition;		
			RopeLineRendererPositions = new Vector3[lineRenderer.positionCount];
			lineRenderer.GetPositions(RopeLineRendererPositions);
			DeltaTime = time;
    	}
    }
}
