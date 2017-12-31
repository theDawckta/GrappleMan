using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using System;

namespace Grappler.DataModel
{
    public class GrappleDataController : MonoBehaviour
    {
        private string privateKey = "SOMESECRETKEY";
        private string GetReplaysUrl = "http://localhost/GetReplays.php?";
        private string AddUserURL = "http://localhost/AddUser.php?";
        private string AddLevelURL = "http://localhost/AddLevel.php?";
        private string AddReplayURL = "http://localhost/AddReplay.php?";

       
        private string _userName;

        public void SetName(string userName)
        {
            _userName = userName;
        }
        
        void Start()
        {
//			for(int i = 0; i < 5; i++)
//            {
//				StartAddUser("User" + i);
//            }
//
//			for(int j = 0; j < 5; j++)
//            {
//				for(int k = 0; k < 20; k++)
//	            {
//					StartAddReplay("User" + j, "Test", 20.0f + j + k, "replay data" + j + "_" + k );
//	            }
//            }
			//StartGetReplays("Test");
        }

        ///Encryption function: http://wiki.unity3d.com/index.php?title=MD5
        private string Md5Sum(string strToEncrypt)
        {
            System.Text.UTF8Encoding ue = new System.Text.UTF8Encoding();
            byte[] bytes = ue.GetBytes(strToEncrypt);

            System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] hashBytes = md5.ComputeHash(bytes);

            string hashString = "";

            for (int i = 0; i < hashBytes.Length; i++)
            {
                hashString += System.Convert.ToString(hashBytes[i], 16).PadLeft(2, '0');
            }

            return hashString.PadLeft(32, '0');
        }

        public void StartAddUser(string userName)
        {
            StartCoroutine(AddUser(userName));
        }
        
        IEnumerator AddUser(string userName)
        {
            string hash = Md5Sum(userName + privateKey);

            WWW NewUserPost = new WWW(AddUserURL + "userName=" + WWW.EscapeURL(userName) + "&hash=" + hash);
            yield return NewUserPost;

            if (NewUserPost.error == null && NewUserPost.text == userName)
            {
                Debug.Log("NEW USER ADDED");
            }
            else
            {
                Debug.Log("USER ALREADY EXISTS");
            }
        }

        public void StartAddLevel(string levelName)
        {
            StartCoroutine(AddLevel(levelName));
        }

        IEnumerator AddLevel(string levelName)
        {
            string hash = Md5Sum(levelName + privateKey);

            WWW NewLevelPost = new WWW(AddLevelURL + "levelName=" + WWW.EscapeURL(levelName) + "&hash=" + hash);
            yield return NewLevelPost;

            if (NewLevelPost.error == null && NewLevelPost.text == levelName)
            {
                Debug.Log("NEW LEVEL ADDED");
            }
            else
            {
                Debug.Log("LEVEL ALREADY EXISTS");
            }
        }

        public void StartAddReplay(string userName, string levelName, float replayTime, string replayData)
        {
            StartCoroutine(AddReplay(userName, levelName, replayTime, replayData));
        }
        
        IEnumerator AddReplay(string userName, string levelName, float replayTime, string replayData)
        {
            string hash = Md5Sum(userName + privateKey);

            WWW ReplayPost = new WWW(AddReplayURL + "userName=" + WWW.EscapeURL(userName) + "&hash=" + hash + "&levelName=" + levelName + "&replayTime=" + replayTime + "&replayData=" + replayData);
            yield return ReplayPost;

            if (ReplayPost.error == null)
            {
                Debug.Log(ReplayPost.text);
            }
            else
            {
                //Handle error
            }
        }

		public void StartGetReplays(string levelName)
        {
			StartCoroutine(GetReplays(levelName, 5));
        }
        
        IEnumerator GetReplays(string levelName, int numOfReplays)
        {
			string hash = Md5Sum(levelName + privateKey);

			WWW GetReplaysPost = new WWW(GetReplaysUrl + "levelName=" + WWW.EscapeURL(levelName) + "&hash=" + hash + "&numOfReplays=" + numOfReplays);
			yield return GetReplaysPost;

			if (GetReplaysPost.error == null)
            {
            	List<PlayerReplayModel> replays = new List<PlayerReplayModel>();
				foreach(JSONNode replay in JSON.Parse(GetReplaysPost.text))
				{
					string userName = replay["UserName"];
					float replayTime = (float)replay["ReplayTime"];
					List<PlayerStateModel> states = new List<PlayerStateModel>();
					foreach(JSONNode state in JSON.Parse(replay["ReplayData"]))
						states.Add(JsonUtility.FromJson<PlayerStateModel>(state.ToString()));

					replays.Add(new PlayerReplayModel(userName, replayTime, states));
				}
				Debug.Log(replays.Count);
            }
            else
            {
                //Handle error
            }
        }
    }
}