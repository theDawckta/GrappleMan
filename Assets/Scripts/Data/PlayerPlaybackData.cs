using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;

namespace Grappler.DataModel
{
	public class PlayerPlaybackController
    {
        public static int MaxNumOfRecords{get { return _numOfRecords; } private set { }}

		private static string _playerCompletedDataLocation = string.Format("{0}/{1}", Application.persistentDataPath, "PlayerDataCompleted");
		private static string _playerDiedDataLocation = string.Format("{0}/{1}", Application.persistentDataPath, "PlayerDataDied");
		private static string[] _playerDataLocations = new string[] { _playerCompletedDataLocation, _playerDiedDataLocation };
		private static string _playerDataFileName = string.Format("/{0}_{1}_", "User", "GhostData");
		private static int _numOfRecords = 6;

		public static void Init()
        {
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

                // trim any files greater than _numOfRecords
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
        }

        public static void SavePlayerPlaybackLocal(PlayerPlaybackModel playerPlayback, bool playerCompleted)
		{
            string playerDataLocation;

            if(playerCompleted)
				playerDataLocation = _playerCompletedDataLocation;
			else
				playerDataLocation = _playerDiedDataLocation;

            List<PlayerPlaybackModel> playerPlaybackModels = GetPlayerPlaybackLocal(_numOfRecords);

            for (int i = 0; i < _numOfRecords; i++)
            {
                if (playerPlaybackModels.Count == 0)
                {
                    SavePlayerPlayback(playerPlayback, i, playerDataLocation);
                    return;
                }
                else if (i < playerPlaybackModels.Count)
                {
                    if (playerPlayback.Time < playerPlaybackModels[i].Time)
                    {
                        SavePlayerPlayback(playerPlayback, i, playerDataLocation);
                        return;
                    }
                }
                if (i == playerPlaybackModels.Count)
                {
                    SavePlayerPlayback(playerPlayback, i, playerDataLocation);
                    return;
                }
            }
	    }

        static void SavePlayerPlayback(PlayerPlaybackModel playerPlayback, int insertIndex, string playerDataLocation)
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
		public IEnumerator GetPlayerPlaybackDataServer (System.Action<string> callback)
		{
			string URLString = "http://XXXXX/Services/GetPropertyValues";
			WWWForm form = new WWWForm ();
			form.AddBinaryData ("binary", new byte[1]);
			var headers = form.headers;
			headers.Remove ("Content-Type");
			headers.Add ("appKey", "XXX-XXX-XXX");
			headers.Add ("Content-Type", "application/json");
			headers.Add ("Accept", "application/json");
			WWW www = new WWW (URLString, form.data, headers);
			yield return www;

			callback(www.text);
		}
    }
	
	[Serializable]
    public class PlayerPlaybackModel
	{
		public bool HasStates {get {return _state.Count > 0;} private set{}}
		public PlayerStateModel StartingState {get {return _startingState;} private set{}}
		public float Time {get{return _time;} private set{}}

		[SerializeField]
		private PlayerStateModel _startingState;
		[SerializeField]
		private List<PlayerStateModel> _state;
		[SerializeField]
		private float _time;

        public PlayerPlaybackModel(PlayerStateModel startingState)
        {
			_startingState = startingState;
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
        	PlayerStateModel tempPlayerState = _state[0];
			_state.RemoveAt(0);
			return tempPlayerState;
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
