using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using Grappler.Constants;

namespace Grappler.DataModel
{
    public class PlayerReplayController
    {
        public static int NumOfCompletedRecords { get; private set;}

        private static string _playerCompletedDataLocation = string.Format("{0}/{1}", Application.persistentDataPath, "PlayerDataCompleted");
        private static string _playerDiedDataLocation = string.Format("{0}/{1}", Application.persistentDataPath, "PlayerDataDied");
        private static string[] _playerDataLocations = new string[] { _playerCompletedDataLocation, _playerDiedDataLocation };
        private static string _playerDataFileName = string.Format("/{0}_{1}_", "User", "GhostData");

        public static void Init()
        {
            for (int i = 0; i < _playerDataLocations.Length; i++)
            {
                Directory.CreateDirectory(_playerDataLocations[i]);

				// trim any files greater than PlayerPrefs Ghost_RECORDS
				int tempNumOfRecords = PlayerPrefs.GetInt(Constants.Constants.GHOST_RECORDS);
                while (File.Exists(_playerDataLocations[i] + _playerDataFileName + tempNumOfRecords + ".json"))
                {
                    File.Delete(_playerDataLocations[i] + _playerDataFileName + tempNumOfRecords + ".json");
                    tempNumOfRecords = tempNumOfRecords + 1;
                }
            }

			NumOfCompletedRecords = Directory.GetFiles(_playerDataLocations[0], "*.json").Length;
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

        public static void ProcessPlayerPlayback(PlayerReplayModel playerPlayback, bool playerCompleted)
        {
            string playerDataLocation;

            if (playerCompleted)
                playerDataLocation = _playerCompletedDataLocation;
            else
                playerDataLocation = _playerDiedDataLocation;

			List<PlayerReplayModel> playerPlaybackModels = GetPlayerPlaybackLocal(PlayerPrefs.GetInt(Constants.Constants.GHOST_RECORDS));

            if (playerPlaybackModels.Count == 0)
            {
				SavePlayerPlaybackLocal(playerPlayback, playerPlaybackModels.Count, playerDataLocation);
                return;
            }

			for (int i = 0; i < playerPlaybackModels.Count; i++)
			{
                if (i < playerPlaybackModels.Count)
                {
                    if (playerPlayback.ReplayTime < playerPlaybackModels[i].ReplayTime)
                    {
                        SavePlayerPlaybackLocal(playerPlayback, i, playerDataLocation);
                        return;
                    }
                }
            }

			if (playerPlaybackModels.Count < PlayerPrefs.GetInt(Constants.Constants.GHOST_RECORDS))
            {
				SavePlayerPlaybackLocal(playerPlayback, playerPlaybackModels.Count, playerDataLocation);
                return;
            }
        }

        static void SavePlayerPlaybackLocal(PlayerReplayModel playerPlayback, int insertIndex, string playerDataLocation)
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

            // set NumOfCompletedRecords
			NumOfCompletedRecords = Directory.GetFiles(_playerDataLocations[0], "*.json").Length;
        }

        static void SavePlayerPlaybackServer(PlayerReplayModel playerPlayback)
        {
            // serialize playerPlayback
            var bytes = System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(playerPlayback));
        }

        public static List<PlayerReplayModel> GetPlayerPlaybackLocal(int numOfRecords)
        {
            List<PlayerReplayModel> playerPlaybacks = new List<PlayerReplayModel>();

            for (int i = 0; i < numOfRecords; i++)
            {
                string playerDataFilePath = _playerCompletedDataLocation + _playerDataFileName + i + ".json";

                if (File.Exists(playerDataFilePath))
                    playerPlaybacks.Add(JsonUtility.FromJson<PlayerReplayModel>(File.ReadAllText(playerDataFilePath)));
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
    public class PlayerReplayModel
    {
        public bool HasStates { get { return _stateIndex < _state.Count; } private set { } }
        public Vector3 StartingPosition  { get {  return (_state.Count > 0) ? _state[0].BodyPosition : Vector3.zero; } private set{} }
		public float ReplayTime { get { return _replayTime; } private set {} }
		public string UserName { get { return _userName; } private set {} }
        
		[SerializeField]
		private List<PlayerStateModel> _state;
		[SerializeField]
		private float _replayTime;
		[SerializeField]
		private string _userName;

        private int _stateIndex = 0;

		public PlayerReplayModel(string userName, float replayTime, List<PlayerStateModel> replayData)
        {
			_userName = userName;
			_replayTime = replayTime;
			_state = replayData;
        }

        public PlayerReplayModel()
        {
        	_state = new List<PlayerStateModel>();
        }

        public void AddPlayerState(PlayerStateModel playerState, bool final = false)
        {
			_state.Add(playerState);
			if (final)
			{
				for (int i = 0; i < _state.Count; i++)
					_replayTime = _replayTime + _state[i].DeltaTime;
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
