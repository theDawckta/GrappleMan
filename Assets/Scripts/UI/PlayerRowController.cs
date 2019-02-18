using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerRowController : MonoBehaviour 
{
	public Text PlayerName;
	public Text PlayerTime;

	public void SetPlayerRow(string playerName, string playerTime)
	{
		PlayerName.text = playerName;
		PlayerTime.text = playerTime;
	}

    public void MarkRowAsPlayer()
    {
        PlayerName.color = Color.blue;
    }
}
