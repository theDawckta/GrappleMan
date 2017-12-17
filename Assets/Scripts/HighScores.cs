using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class HighScores : MonoBehaviour
{
    ///Fill in your server data here.
    private string privateKey = "SOMESECRETKEY";
    private string TopScoresURL = "http://localhost/TopScores.php";

    //Don't forget the question marks!
    private string AddScoreURL = "http://localhost/AddScores.php?";
    private string RankURL = "http://localhost/GetRank.php?";

    //The score and username we submit
    private int highscore;
    private string username;
    private int rank;

    ///Our public access functions
    public void Setscore(int givenscore)
    {
        highscore = givenscore;
    }

    public void SetName(string givenname)
    {
        username = givenname;
    }

    //Our standard Unity functions
    //Called as soon as the class is activated.
    void OnEnable()
    {
        StartCoroutine(AddScore("Cunty", 10)); // We post our scores.
    }

    ///Our encryption function: http://wiki.unity3d.com/index.php?title=MD5
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

    ///Our Error function
    void Error()
    {
        //guiText.enabled = true;
        //guiText.text = "Connection error.";
        //guiText.fontSize = (int)(guiText.fontSize * 0.6f);
    }

    ///Our IEnumerators
    IEnumerator AddScore(string name, int score)
    {
        string hash = Md5Sum(name + score + privateKey);

        WWW ScorePost = new WWW(AddScoreURL + "name=" + WWW.EscapeURL(name) + "&score=" + score + "&hash=" + hash); //Post our score
        yield return ScorePost; // The function halts until the score is posted.

        if (ScorePost.error == null)
        {
            print("Added score apaz");
            StartCoroutine(GrabRank(name)); // If the post is successful, the rank gets grabbed next.
        }
        else
        {
            Error(); // Otherwise we use our error protocol
        }
    }

    IEnumerator GrabRank(string name)
    {
        //Try and grab the Rank
        WWW RankGrabAttempt = new WWW(RankURL + "name=" + WWW.EscapeURL(name));

        yield return RankGrabAttempt;

        if (RankGrabAttempt.error == null)
        {
            bool result;
            int number;

            result = int.TryParse(RankGrabAttempt.text, out number);
            if (result)
            {
                rank = number;
            }
            StartCoroutine(GetTopScores()); // Get our top scores
        }
        else
        {
            Error();
        }
    }

    IEnumerator GetTopScores()
    {
        WWW GetScoresAttempt = new WWW(TopScoresURL);
        yield return GetScoresAttempt;

        if (GetScoresAttempt.error != null)
        {
            Error();
        }
        else
        {
            print("Top scores apaz");
            //Collect up all our data
            string[] textlist = GetScoresAttempt.text.Split(new string[] { "\n", "\t" }, System.StringSplitOptions.RemoveEmptyEntries);

            //Split it into two smaller arrays
            string[] Names = new string[Mathf.FloorToInt(textlist.Length / 2)];
            string[] Scores = new string[Names.Length];
            for (int i = 0; i < textlist.Length; i++)
            {
                if (i % 2 == 0)
                {
                    Names[Mathf.FloorToInt(i / 2)] = textlist[i];
                }
                else Scores[Mathf.FloorToInt(i / 2)] = textlist[i];
            }
        }
    }

}