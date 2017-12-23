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
		private static string _playerCompletedDataLocation = string.Format("{0}/{1}", Application.persistentDataPath, "PlayerDataCompleted");
		private static string _playerDiedDataLocation = string.Format("{0}/{1}", Application.persistentDataPath, "PlayerDataDied");
		private static string _playerDataFileName = string.Format("/{0}_{1}_", "User", "GhostData");
		private static int _numOfRecords = 6;

		public static void SavePlayerPlaybackLocal(PlayerPlaybackModel playerPlayback, bool playerCompleted)
		{
			List<PlayerPlaybackModel> playerPlaybackModels = GetPlayerPlaybackLocal(_numOfRecords);

            if(playerPlaybackModels.Count == 0)
                SavePlayerPlayback(playerPlayback, playerPlaybackModels.Count, playerCompleted);
            else
            {
                for (int i = 0; i < _numOfRecords && i < playerPlaybackModels.Count; i++)
                {
                    if(i > playerPlaybackModels.Count - 1)
                        SavePlayerPlayback(playerPlayback, i, playerCompleted);
                    else if (playerPlayback.Time < playerPlaybackModels[i].Time)
                        SavePlayerPlayback(playerPlayback, i, playerCompleted);
					else
                        SavePlayerPlayback(playerPlayback, i + 1, playerCompleted);
                }
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

		static void SavePlayerPlayback(PlayerPlaybackModel playerPlayback, int insertIndex, bool playerCompleted)
	    {
            Debug.Log("SAVING AT INDEX: " + insertIndex);
			string lastItemFilePath;
			string nextToLastFilePath;
			var bytes = System.Text.Encoding.UTF8.GetBytes (JsonUtility.ToJson(playerPlayback));
			string[] tempFiles = Directory.GetFiles(_playerCompletedDataLocation, "*.json", SearchOption.TopDirectoryOnly);
			int numOfExistingModels = tempFiles.Length;

			if(playerCompleted)
			{
				lastItemFilePath = _playerCompletedDataLocation + _playerDataFileName + insertIndex + ".json";
				Directory.CreateDirectory(_playerCompletedDataLocation);
			}
			else
			{
				lastItemFilePath = _playerDiedDataLocation + _playerDataFileName + insertIndex + ".json";
				Directory.CreateDirectory(_playerDiedDataLocation);
			}

			try
			{
                for (int i = numOfExistingModels - 1; i > numOfExistingModels; i--)
				{
					lastItemFilePath = _playerCompletedDataLocation + _playerDataFileName + i + ".json";
					nextToLastFilePath = _playerCompletedDataLocation + _playerDataFileName + (i - 1) + ".json";
                    if (!File.Exists(nextToLastFilePath))
                        File.WriteAllBytes(_playerCompletedDataLocation + _playerDataFileName + insertIndex + ".json", bytes);
                    else
					    System.IO.File.Copy(nextToLastFilePath, lastItemFilePath, true);
//					if(!File.Exists(lastItemFilePath) && File.Exists(nextFilePath))
//					{
//						File.WriteAllBytes (nextFilePath, bytes);
//					}
				}
			}
			catch(Exception e)
			{
				Debug.LogException(e);
			}
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
		[SerializeField]
		private bool _playerWon;

        public PlayerPlaybackModel(PlayerStateModel startingState)
        {
			_startingState = startingState;
        	_state = new List<PlayerStateModel>();
        }

        public void AddPlayerState(PlayerStateModel playerState, bool final = false)
        {
			_state.Add(playerState);
			if(final)
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
