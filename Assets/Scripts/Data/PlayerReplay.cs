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
        public static int NumOfLocalRecords { get; private set;}

        private static string _playerCompletedDataLocation = string.Format("{0}/{1}", Application.persistentDataPath, "PlayerDataCompleted");
        private static string _playerDataFileName = string.Format("/{0}_{1}_", "User", "GhostData");
		private static GrappleServerData _dataController;

		void Awake()
		{
			_dataController = GrappleServerData.Instance;
		}

        public static void InitLocalRecords()
        {
			Directory.CreateDirectory(_playerCompletedDataLocation);

			// trim any files greater than PlayerPrefs Ghost_RECORDS
			int tempNumOfRecords = PlayerPrefs.GetInt(Constants.NUM_OF_LOCAL_GHOST_RECORDS);
			while (File.Exists(_playerCompletedDataLocation + _playerDataFileName + tempNumOfRecords + ".json"))
            {
				File.Delete(_playerCompletedDataLocation + _playerDataFileName + tempNumOfRecords + ".json");
                tempNumOfRecords = tempNumOfRecords + 1;
            }

			NumOfLocalRecords = Directory.GetFiles(_playerCompletedDataLocation, "*.json").Length;
        }

        public static void ClearData()
        {
			Directory.CreateDirectory(_playerCompletedDataLocation);

            // delete all files
            int tempNumOfRecords = 0;
			while (File.Exists(_playerCompletedDataLocation + _playerDataFileName + tempNumOfRecords + ".json"))
            {
				File.Delete(_playerCompletedDataLocation + _playerDataFileName + tempNumOfRecords + ".json");
                tempNumOfRecords = tempNumOfRecords + 1;
            }
        }

		public static IEnumerator SavePlayerPlayback(PlayerReplayModel playerPlayback, Action<bool> action)
        {
			CheckConnection.Instance.StartCoroutine(CheckConnection.Instance.CheckInternetConnection((isConnected)=>{
				if(isConnected)
				{
					_dataController.StartCoroutine(_dataController.AddReplay (playerPlayback, (success) =>{
						if(success)
							action(success);
					}));
				}
			}));

			List<PlayerReplayModel> playerReplayModels = GetPlayerReplaysLocal(playerPlayback.LevelName, PlayerPrefs.GetInt(Constants.NUM_OF_LOCAL_GHOST_RECORDS));

			if (playerReplayModels.Count == 0)
			{
				SavePlayerReplayLocal(playerPlayback, playerReplayModels.Count, _playerCompletedDataLocation);
				yield break;
			}

			for (int i = 0; i < playerReplayModels.Count; i++)
			{
				if (i < playerReplayModels.Count)
				{
					if (playerPlayback.ReplayTime < playerReplayModels[i].ReplayTime)
					{
						SavePlayerReplayLocal(playerPlayback, i, _playerCompletedDataLocation);
						yield break;
					}
				}
			}

			if (playerReplayModels.Count < PlayerPrefs.GetInt(Constants.NUM_OF_LOCAL_GHOST_RECORDS))
				SavePlayerReplayLocal(playerPlayback, playerReplayModels.Count, _playerCompletedDataLocation);
			
			action (true);
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
			NumOfLocalRecords = Directory.GetFiles(_playerCompletedDataLocation, "*.json").Length;
        }

		public IEnumerator GetPlayerReplays(string levelName, Action<List<PlayerReplayModel>> action)
		{
			CheckConnection.Instance.StartCoroutine(CheckConnection.Instance.CheckInternetConnection((isConnected)=>{
				if(isConnected)
				{
					_dataController.StartCoroutine(_dataController.GetPlayerReplaysServer(levelName, PlayerPrefs.GetInt(Constants.GHOSTS), (replays) =>{
						action(replays);
					}));
				}
				else
					action(GetPlayerReplaysLocal(levelName, PlayerPrefs.GetInt(Constants.GHOSTS)));
			}));

			yield return null;
		}

        private static List<PlayerReplayModel> GetPlayerReplaysLocal(string levelName, int numOfRecords)
        {
            List<PlayerReplayModel> playerPlaybacks = new List<PlayerReplayModel>();

            for (int i = 0; i < numOfRecords; i++)
            {
                string playerDataFilePath = _playerCompletedDataLocation + _playerDataFileName + i + ".json";

				if (File.Exists (playerDataFilePath)) 
				{
					PlayerReplayModel playerReplayModel = JsonUtility.FromJson<PlayerReplayModel> (File.ReadAllText (playerDataFilePath));

					if(playerReplayModel.LevelName == levelName)
						playerPlaybacks.Add (JsonUtility.FromJson<PlayerReplayModel> (File.ReadAllText (playerDataFilePath)));
				}
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
		public bool InUse  { get; set; }
		public string UserName;
		public string LevelName;	
		public float ReplayTime;
		public List<PlayerStateModel> ReplayData;

        private int _stateIndex = 0;

		public PlayerReplayModel()
		{
			ReplayData = new List<PlayerStateModel>();
			InUse = false;
		}

		public PlayerReplayModel(string userName, string levelName, float replayTime, List<PlayerStateModel> replayData)
        {
			UserName = userName;
			LevelName = levelName;
			ReplayTime = replayTime;
			ReplayData = replayData;
			InUse = false;
        }

        public void AddPlayerState(PlayerStateModel playerState, bool final = false)
        {
			ReplayData.Add(playerState);
			if (final)
			{
				for (int i = 0; i < ReplayData.Count; i++)
					ReplayTime = ReplayTime + ReplayData[i].DeltaTime;
			}
        }

        public PlayerStateModel GetNextState()
        {
			PlayerStateModel tempPlayerState = ReplayData[_stateIndex];
            _stateIndex++;
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
