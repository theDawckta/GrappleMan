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
	public delegate void UsernameProcessed(string userName);
	public event UsernameProcessed OnUsernameProcessed;

    private string privateKey = "d41d8cd98f00b204e9800998ecf8427e";
    private string GetReplaysUrl = "http://www.thedawckta.com/grappler/GetReplays.php?";
    private string AddUserURL = "http://www.thedawckta.com/grappler/AddUser.php?";
    private string AddLevelURL = "http://www.thedawckta.com/grappler/AddLevel.php?";
    private string AddReplayURL = "http://www.thedawckta.com/grappler/AddReplay.php?";

    public void StartAddUser(string username)
    {
        StartCoroutine(AddUser(username));
    }
    
    IEnumerator AddUser(string username)
    {
    	username = username.ToUpper();
        string hash = Md5Sum(username + privateKey);

        WWW NewUserPost = new WWW(AddUserURL + "userName=" + WWW.EscapeURL(username) + "&hash=" + hash);
        yield return NewUserPost;

		if(OnUsernameProcessed != null)
			OnUsernameProcessed(NewUserPost.text);
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