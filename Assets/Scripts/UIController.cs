﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Grappler;
using Grappler.Data;
using DG.Tweening;

public class UIController : MonoBehaviour
{
	public delegate void StartButtonClicked();
	public event StartButtonClicked OnStartButtonClicked;

	public delegate void LevelSelectButtonClicked(string levelName);
	public event LevelSelectButtonClicked OnLevelSelectButtonClicked;

    public delegate void ResetDataButtonClicked();
    public event ResetDataButtonClicked OnResetDataButtonClicked;

	public delegate void ResetButtonClicked();
    public event ResetButtonClicked OnResetButtonClicked;

    public delegate void GhostValueChanged(int value);
    public event GhostValueChanged OnGhostsValueChanged;

    public delegate void GhostRecordsValueChanged(int value);
    public event GhostRecordsValueChanged OnGhostRecordsValueChanged;

	public delegate void UserSaveButtonClicked(string value);
	public event UserSaveButtonClicked OnUserSaveButtonClicked;

    public RectTransform UIPanel;
    public InputField SeedInputField;
    public Text FPSText;
    public Text TimerText;
	public GameObject NoUsernameScreen;
    public GameObject StartScreen;
	public GameObject ConfigScreen;
    public InputField GhostsInput;
    public InputField GhostRecordsInput;
    public int SeedLength;
    public Button StartButton;
	public InputField NewUserInput;
    public InputField UserInput;
	public GameObject UserEdit;
    public GameObject UserCancel;
    public GameObject UserSave;
	public Text UserName;
	public Text ErrorText;
	public Text TotalGhostRecordsLocal;
	public GameObject PlayerRanksScreen;
	public PlayerRowController PlayerRow;

    private String seed;
    private float _timer;
    private bool _timeStarted;

    void Start()
    {
        StartCoroutine(FPS());
        seed = RandomString(SeedLength);
		//UserEdit.SetActive(true);
        UserInput.gameObject.SetActive(false);
		UserSave.SetActive(false);
		UserCancel.SetActive(false);
		ErrorText.text = "";
        // SeedInputField.text = seed;
        // init levelgenerator here when ready
        // LevelGenerator.Init(seed);
    }

    void Update()
    {
    	if (_timeStarted == true)
    	{
            _timer = _timer + Time.deltaTime;
            TimerText.text = GetTimeText(_timer);
		}
    }

	public void InitPlayerRanksScreen(List<PlayerReplayModel> playerReplays)
	{
		for (int i = 0; i < playerReplays.Count; i++)
		{
			PlayerRowController playerRow = (PlayerRowController)Instantiate(PlayerRow);
            playerRow.SetPlayerRow((i + 1) + ". " + playerReplays[i].UserName, GetTimeText(playerReplays[i].ReplayTime));

			playerRow.transform.SetParent(PlayerRanksScreen.transform.Find("Players"), false);
		}

		if (playerReplays.Count == 0)
			UIStartButtonClicked ();
		else
		{
			StartButton.gameObject.SetActive(true);
			PlayerRanksScreen.SetActive(true);
		}
	}

	public void UILevelButtonClicked(string levelName)
	{
        StartScreen.SetActive(false);
        OnLevelSelectButtonClicked(levelName);
	}

	public void UIStartButtonClicked()
    {
		StartButton.gameObject.SetActive(false);
		PlayerRanksScreen.SetActive(false);
		if(OnStartButtonClicked != null)
			OnStartButtonClicked();
        _timer = 0.0f;
		_timeStarted = true;
    }

	public void UIUserEditButtonClicked()
    {
        UserEdit.SetActive(false);
        UserInput.gameObject.SetActive(true);
		UserSave.SetActive(true);
		UserCancel.SetActive(true);
    }

	public void UIUserCancelButtonClicked()
    {
		UserEdit.SetActive(true);
        UserInput.gameObject.SetActive(false);
		UserSave.SetActive(false);
		UserCancel.SetActive(false);
    }

	public void UINewUserSaveButtonClicked()
    {
		OnUserSaveButtonClicked(NewUserInput.text);
    }

	public void UIUserSaveButtonClicked()
    {
		if(UserInput.text.Length > 0)
		{
			if(UserInput.text.ToUpper() == UserName.text)
			{
				UserEdit.SetActive(true);
        		UserInput.gameObject.SetActive(false);
				UserSave.SetActive(false);
				UserCancel.SetActive(false);
			}
			else
			{
				if (OnUserSaveButtonClicked != null)
					OnUserSaveButtonClicked(UserInput.text);
			}
		}
		else
		{
			UserEdit.SetActive(true);
        	UserInput.gameObject.SetActive(false);
			UserSave.SetActive(false);
			UserCancel.SetActive(false);
		}
    }

    public void UIResetDataButtonClicked()
    {
        if (OnResetDataButtonClicked != null)
            OnResetDataButtonClicked();
    }

	public void UIResetButtonClicked()
    {
        if (OnResetButtonClicked != null)
            OnResetButtonClicked();
    }

    public void UIGhostsValueChanged(string value)
    {
        if (OnGhostsValueChanged != null)
        {
            OnGhostsValueChanged(Int32.Parse(GhostsInput.text));
            PlayerPrefs.SetInt(Constants.GHOSTS, Int32.Parse(GhostsInput.text));
        }
    }

    public void UIGhostRecordsValueChanged(string value)
    {
        if (OnGhostRecordsValueChanged != null)
        {
            OnGhostRecordsValueChanged(Int32.Parse(GhostRecordsInput.text));
            PlayerPrefs.SetInt(Constants.NUM_OF_LOCAL_GHOST_RECORDS, Int32.Parse(GhostRecordsInput.text));
        }
    }

	public void UIDeletePlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void EndGame()
    {
		StartScreen.SetActive (true);
		foreach(Transform playerRow in PlayerRanksScreen.transform.Find("Players")) 
		{
			Destroy(playerRow.gameObject);
		}
        Debug.Log(_timer);
        _timeStarted = false;
    }

    public void SetSeed()
    {
        if (SeedInputField.text != "")
        {
            seed = SeedInputField.text;

            // init levelgenerator here when ready
            // LevelGenerator.Init(seed);
        }
    }

    public void RandomSeed()
    {
        seed = RandomString(SeedLength);
        SeedInputField.text = seed;

        // init levelgenerator here when ready
        // LevelGenerator.Init(seed);
    }

    public static string RandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new System.Random();
        return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
    }

	public void ToggleConfigScreen()
    {
		ConfigScreen.gameObject.SetActive(!ConfigScreen.gameObject.activeSelf);
    }

    string GetTimeText (float time)
	{
		return string.Format("{0:00}:{1:00}.{2:00}",
						     Mathf.Floor(time / 60),
							 Mathf.Floor(time) % 60,
							 Mathf.Floor((time * 100) % 100));
    }

    IEnumerator FPS()
    {
        for (;;)
        {
            int lastFrameCount = Time.frameCount;
            float lastTime = Time.realtimeSinceStartup;
            yield return new WaitForSeconds(0.5f);
            float timeSpan = Time.realtimeSinceStartup - lastTime;
            int frameCount = Time.frameCount - lastFrameCount;
			FPSText.text = "FPS: " + Mathf.RoundToInt(frameCount / timeSpan);
        }
    }

    public void SetErrorText(string errorText)
    {
		ErrorText.text = errorText;
		ErrorText.DOFade(1.0f, 0.5f);
		ErrorText.DOFade(0.0f, 0.5f).SetDelay(3.0f).OnComplete(() => {
            ErrorText.text = "";
         });
    }
}
