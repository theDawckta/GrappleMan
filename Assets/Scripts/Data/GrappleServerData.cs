using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;	
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SimpleJSON;
using Grappler.Data;

public class GrappleServerData : Singleton<GrappleServerData>
{
    private string privateKey = "d41d8cd98f00b204e9800998ecf8427e";
    private string GetReplaysUrl = "http://www.zekelasater.com/Grappler/GetReplays.php?";
    private string AddUserURL = "http://www.zekelasater.com/Grappler/AddUser.php?";
    private string AddLevelURL = "http://www.zekelasater.com/Grappler/AddLevel.php?";
    private string AddReplayURL = "http://www.zekelasater.com/Grappler/AddReplay.php?";

    //private string GetReplaysUrl = "http://10.0.1.14:8888/GetReplays.php?";
    //private string AddUserURL = "http://10.0.1.14:8888/AddUser.php?";
    //private string AddLevelURL = "http://10.0.1.14:8888/AddLevel.php?";
    //private string AddReplayURL = "http://10.0.1.14:8888/AddReplay.php?";

    public IEnumerator AddUser(string username, Action<bool, string> action)
    {
    	username = username.ToUpper();
        string hash = Md5Sum(username + privateKey);

        WWW NewUserPost = new WWW(AddUserURL + "userName=" + WWW.EscapeURL(username) + "&hash=" + hash);
        yield return NewUserPost;

        if (!string.IsNullOrEmpty(NewUserPost.error))
            action(false, NewUserPost.error);
        else if (NewUserPost.text == "")
            action(false, "");
		else
            action(true, NewUserPost.text);
    }

    public IEnumerator AddLevel(string levelName, Action<bool, string> action)
    {
        string hash = Md5Sum(levelName + privateKey);

        WWW NewLevelPost = new WWW(AddLevelURL + "levelName=" + WWW.EscapeURL(levelName) + "&hash=" + hash);
        yield return NewLevelPost;

        if (!string.IsNullOrEmpty(NewLevelPost.error))
            action(false, NewLevelPost.error);
        else if (NewLevelPost.text == "")
            action(false, "");
        else
            action(true, NewLevelPost.text);
    }
    
	public IEnumerator AddReplay(PlayerReplayModel playerReplay, Action<bool> action)
    {
		string hash = Md5Sum(playerReplay.UserName + privateKey);
		var encoding = new System.Text.UTF8Encoding();
		string playerReplayJson = JsonUtility.ToJson(playerReplay);
		JSONNode replayJsonNode = JSON.Parse(playerReplayJson);
		byte[] postData = encoding.GetBytes(replayJsonNode["ReplayData"].ToString());

		WWW ReplayPost = new WWW(AddReplayURL + "userName=" + WWW.EscapeURL(playerReplay.UserName) + "&hash=" + hash + "&levelName=" + playerReplay.LevelName + "&replayTime=" + playerReplay.ReplayTime, postData);
        yield return ReplayPost;

        if (ReplayPost.error == null)
        {
			action (true);
        }
        else
        {
			action (false);
        }
    }

    //SELECT Users.UserName, Levels.LevelName, ReplayTime, ReplayData FROM Replays 
    //INNER JOIN Users ON Users.Id = Replays.UserId 
    //INNER JOIN Levels ON Levels.Id = Replays.LevelId 
    //WHERE Levels.LevelName = 'test' ORDER BY ReplayTime ASC LIMIT 26;
    public IEnumerator GetPlayerReplaysServer(string levelName, int numOfReplays, Action<List<PlayerReplayModel>> action)
    {
		string hash = Md5Sum(levelName + privateKey);

		WWW GetReplaysPost = new WWW(GetReplaysUrl + "levelName=" + WWW.EscapeURL(levelName) + "&hash=" + hash + "&numOfReplays=" + numOfReplays);
		yield return GetReplaysPost;

		if (GetReplaysPost.error == null)
		{
			List<PlayerReplayModel> replays = new List<PlayerReplayModel>();
			foreach(JSONNode replay in JSON.Parse(GetReplaysPost.text))
			{
                string playerReplayJson = replay.ToString().Replace("\\", String.Empty);
                playerReplayJson = playerReplayJson.ToString().Replace("\"[{", "[{");
                playerReplayJson = playerReplayJson.ToString().Replace("}]\"", "}]");
                PlayerReplayModel newPlayerReplayModel = JsonUtility.FromJson<PlayerReplayModel> (playerReplayJson);
				replays.Add(newPlayerReplayModel);
			}

			action (replays);
		}
		else
		{
			//Handle error
		}
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

	public static Dictionary<K,V> HashtableToDictionary<K,V> (Hashtable table)
	{
		return table
			.Cast<DictionaryEntry> ()
			.ToDictionary (kvp => (K)kvp.Key, kvp => (V)kvp.Value);
	}
}