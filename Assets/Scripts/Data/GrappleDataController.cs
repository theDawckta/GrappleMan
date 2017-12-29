using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

namespace Grappler.DataModel
{
    public class GrappleDataController : MonoBehaviour
    {
        private string privateKey = "SOMESECRETKEY";
        private string GetSomethingUrl = "http://localhost/GetSomething.php";

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
            StartAddUser("theDawckta");
            StartAddLevel("Test");
            StartAddReplay("theDawckta", "Test", 20.0f, "replay data" );
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
    }
}