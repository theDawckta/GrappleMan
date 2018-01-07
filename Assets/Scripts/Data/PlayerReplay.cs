using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;
using Grappler;
using Grappler.Util;

namespace Grappler.Data
{
	public class PlayerReplay : Singleton<PlayerReplay>
    {
        public static int NumOfCompletedRecords { get; private set;}

        private static string _playerCompletedDataLocation = string.Format("{0}/{1}", Application.persistentDataPath, "PlayerDataCompleted");
        private static string _playerDiedDataLocation = string.Format("{0}/{1}", Application.persistentDataPath, "PlayerDataDied");
        private static string[] _playerDataLocations = new string[] { _playerCompletedDataLocation, _playerDiedDataLocation };
        private static string _playerDataFileName = string.Format("/{0}_{1}_", "User", "GhostData");
		private static GrappleServerData _dataController;

		void Awake()
		{
			_dataController = GrappleServerData.Instance;
		}

        public static void Init()
        {
            for (int i = 0; i < _playerDataLocations.Length; i++)
            {
                Directory.CreateDirectory(_playerDataLocations[i]);

				// trim any files greater than PlayerPrefs Ghost_RECORDS
				int tempNumOfRecords = PlayerPrefs.GetInt(Constants.GHOST_RECORDS);
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

        public static void SavePlayerPlayback(PlayerReplayModel playerPlayback, bool playerCompleted)
        {
            string playerDataLocation;

			// Save to run to server
			if (playerCompleted)
			{
				CheckConnection.Instance.StartCoroutine(CheckConnection.Instance.CheckInternetConnection((isConnected)=>{
					_dataController.StartAddReplay (playerPlayback);
				}));
			}

            if (playerCompleted)
                playerDataLocation = _playerCompletedDataLocation;
            else
                playerDataLocation = _playerDiedDataLocation;

			List<PlayerReplayModel> playerReplayModels = GetPlayerReplaysLocal(PlayerPrefs.GetInt(Constants.GHOST_RECORDS));

            if (playerReplayModels.Count == 0)
            {
				SavePlayerReplayLocal(playerPlayback, playerReplayModels.Count, playerDataLocation);
                return;
            }

			for (int i = 0; i < playerReplayModels.Count; i++)
			{
                if (i < playerReplayModels.Count)
                {
                    if (playerPlayback.ReplayTime < playerReplayModels[i].ReplayTime)
                    {
                        SavePlayerReplayLocal(playerPlayback, i, playerDataLocation);
                        return;
                    }
                }
            }

			if (playerReplayModels.Count < PlayerPrefs.GetInt(Constants.GHOST_RECORDS))
            {
				// recursive call
				SavePlayerReplayLocal(playerPlayback, playerReplayModels.Count, playerDataLocation);
                return;
            }
        }

		static void SavePlayerReplayLocal(PlayerReplayModel playerReplay, int insertIndex, string playerDataLocation)
        {
            string lastItemFilePath;
            string nextToLastFilePath;
            string[] tempFiles;
            int numOfExistingModels;

            // serialize playerPlayback
            var bytes = System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(playerReplay));

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

		public IEnumerator GetPlayerReplays(Action<List<PlayerReplayModel>> action)
		{
			CheckConnection.Instance.StartCoroutine(CheckConnection.Instance.CheckInternetConnection((isConnected)=>{
				if(isConnected)
				{
					_dataController.StartCoroutine(_dataController.GetPlayerReplaysServer(SceneManager.GetActiveScene().name, Constants.GHOST_COMPETITORS, (replays) =>{
						action(replays);
					}));
				}
				else
					action(GetPlayerReplaysLocal(Constants.GHOST_COMPETITORS));
			}));

			yield return null;
		}

        private static List<PlayerReplayModel> GetPlayerReplaysLocal(int numOfRecords)
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
    }

    [Serializable]
    public class PlayerReplayModel
    {
		public bool HasStates { get { return _stateIndex < ReplayData.Count; } private set { } }
		public Vector3 StartingPosition  { get {  return (ReplayData.Count > 0) ? ReplayData[0].BodyPosition : Vector3.zero; } private set{} }
		public string UserName;
		public string LevelName;	
		public float ReplayTime;
		public List<PlayerStateModel> ReplayData;

        private int _stateIndex = 0;

		public PlayerReplayModel()
		{
			ReplayData = new List<PlayerStateModel>();
		}

		public PlayerReplayModel(string userName, string levelName, float replayTime, List<PlayerStateModel> replayData)
        {
			UserName = userName;
			LevelName = levelName;
			ReplayTime = replayTime;
			ReplayData = replayData;
        }

//		public PlayerReplayModel(string userName, string levelName)
//		{
//			_userName = userName;
//			_levelName = levelName;
//		}

        public void AddPlayerState(PlayerStateModel playerState, bool final = false)
        {
			ReplayData.Add(playerState);
			if (final)
			{
				for (int i = 0; i < ReplayData.Count; i++)
					ReplayTime = ReplayTime + ReplayData[i].DeltaTime;
				Debug.Log (ReplayTime);
			}
        }

        public PlayerStateModel GetNextState()
        {
			PlayerStateModel tempPlayerState = ReplayData[_stateIndex];
            _stateIndex++;
			if (_stateIndex > ReplayData.Count)
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
