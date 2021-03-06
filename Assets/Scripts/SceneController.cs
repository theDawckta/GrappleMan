﻿using System;
using System.Collections;
using System.Collections.Generic;
using Grappler;
using Grappler.Data;
using Grappler.Util;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using Cinemachine;

public class SceneController : MonoBehaviour
{
    public PlayerController Player;
    public UIController GrappleUI;
    
	public GhostPlaybackController GhostPlayback;
	public GameObject GhostHolder;
    public BugController Bug;
    public LevelController Level;
    public CinemachineSmoothPath SmoothPath;
    public CinemachineVirtualCamera VirtualCamera;
    public CinemachineVirtualCamera VirtualBackgroundCamera;

    private PlayerRecorderController _playerRecorder;
    private AudioSource _playerAudio;
    private AudioClip _song;
    private Camera _mainCamera;
    private Vector3 _mainCameraStartPosition;
    private List<PlayerReplayModel> _replays = new List<PlayerReplayModel>();
    private List<GhostPlaybackController> _ghostPlaybacks = new List<GhostPlaybackController>();
    private bool _gameOn = false;
	private string _username = "User";
	private GameObject _currentLevel;
	private string _levelName;
    private string _startLevelName = "StartSection";
    //private bool _playerMoved = false;
    private int _startGhostIndex;
    private float _ghostReleaseInterval = 2.0f;
    private float UICameraOffset = 0.75f;

    void Awake()
    {
        Application.targetFrameRate = 60;
        _playerRecorder = Player.GetComponentInChildren<PlayerRecorderController>();
        _playerAudio = GetComponent<AudioSource>();
        _song = Resources.Load("Songs/BeatOfTheTerror") as AudioClip;
        _playerAudio.clip = _song;
        _playerAudio.loop = true;
        _mainCamera = Camera.main;
        _mainCameraStartPosition = _mainCamera.transform.position;
        PlayerReplay.InitLocalRecords();
		_username = PlayerPrefs.GetString(Constants.USERNAME_KEY);
        _levelName = _startLevelName;
    }

    void Start()
    {
        // PlayerPrefs.DeleteAll();
    	// first run init
		if (!PlayerPrefs.HasKey(Constants.GHOSTS) || !PlayerPrefs.HasKey(Constants.NUM_OF_LOCAL_GHOST_RECORDS))
        {
			PlayerPrefs.SetInt(Constants.GHOSTS, Constants.GHOST_COMPETITORS);
			PlayerPrefs.SetInt(Constants.NUM_OF_LOCAL_GHOST_RECORDS, Constants.GHOST_COMPETITORS);
        }
       	
		GrappleUI.GhostsInput.text = PlayerPrefs.GetInt(Constants.GHOSTS).ToString();
		GrappleUI.GhostRecordsInput.text = PlayerPrefs.GetInt(Constants.NUM_OF_LOCAL_GHOST_RECORDS).ToString();

        GrappleUI.TotalGhostRecordsLocal.text = PlayerReplay.NumOfLocalRecords.ToString();

        if (PlayerPrefs.GetString(Constants.USERNAME_KEY) == "")
        {
            GrappleUI.NoUsernameScreen.SetActive(true);
            GrappleUI.StartScreen.SetActive(false);
        }
        else
        {
            GrappleUI.UserName.text = _username;
            GrappleUI.StartScreen.SetActive(true);
        }
        
        Player.Enable();
        Level.SetSectionCreator(Player.gameObject);
        Bug.Init();

        //GrappleServerData.Instance.StartCoroutine(GrappleServerData.Instance.AddLevel(_levelName, (Success, ReturnString) =>
        //{
        //    if (!Success && ReturnString != "")
        //    {
        //        Debug.Log("Server Error: " + ReturnString);
        //        return;
        //    }
        //    PlayerReplay.Instance.StartCoroutine(PlayerReplay.Instance.GetPlayerReplays(_levelName, (replays) =>
        //    {
        //        ReplaysRecieved(replays);
        //        //if (replays.Count > 0)
        //            //StartCoroutine("ReleaseGhosts");
        //    }));
        //}));

        //StartCoroutine("WaitForPlayerInput");
        StartCoroutine("WaitForOpenUI");

        //_playerAudio.Play();
    }

    void Update()
    {
        if(Level.PressurePlaying)
        {
            Bug.Bug.transform.position = Level.Pressure.transform.position;
            Bug.Bug.transform.rotation = Level.Pressure.transform.rotation;
        }
    }

    IEnumerator WaitForOpenUI()
    {
        VirtualCamera.GetCinemachineComponent<CinemachineComposer>().m_ScreenX = UICameraOffset;
        VirtualBackgroundCamera.GetCinemachineComponent<CinemachineComposer>().m_ScreenX = UICameraOffset;
        yield return new WaitForSeconds(0.5f);
        GrappleUI.Show();
    }

    IEnumerator ReleaseGhosts()
    {
        while (true)
        {
            GhostPlaybackController ghostPlayback = (GhostPlaybackController)Instantiate(GhostPlayback);
            ghostPlayback.transform.SetParent(GhostHolder.transform);
            ghostPlayback.SetPlayerReplayModel(_replays[0]);
            _ghostPlaybacks.Add(ghostPlayback);
            ghostPlayback.StartPlayGhostPlayback();

            PlayerReplayModel tempPlayerReplayModel = new PlayerReplayModel (_replays[0].UserName, _replays[0].LevelName, _replays[0].ReplayTime, _replays[0].ReplayData);
            _replays.RemoveAt(0);
            _replays.Add(tempPlayerReplayModel);

            yield return new WaitForSeconds(_ghostReleaseInterval);
        }
    }

    //IEnumerator WaitForPlayerInput()
    //{
    //    while(!Player.HookPlayerInput.TouchStarted && _playerMoved == false)
    //    {
    //        yield return null;
    //    }

    //    Player.Enable(true);
    //    _playerMoved = true;
    //}

    void ReplaysRecieved(List<PlayerReplayModel> replays)
	{
        StopCoroutine("ReleaseGhosts");
        _replays = replays;
		int tempNumOfGhosts = (replays.Count < PlayerPrefs.GetInt(Constants.GHOSTS)) ? replays.Count : PlayerPrefs.GetInt(Constants.GHOSTS);

        for (int i = 0; i < _ghostPlaybacks.Count; i++)
        {
            if (_ghostPlaybacks[i] != null)
            {
                Destroy(_ghostPlaybacks[i].gameObject);
            }
        }
	}

    private void StartGame()
	{
        //if (_playerMoved == true)
        //{
        //    Player.PlayerCompleted(_levelName);
        //    _playerMoved = false;
        //}
        

        _levelName = Level.GetSeed();
        
        GrappleServerData.Instance.StartCoroutine(GrappleServerData.Instance.AddLevel(_levelName, (Success, ReturnString) => {
            if (!Success && ReturnString != "")
            {
                Debug.Log("Server Error: " + ReturnString);
            }
            PlayerReplay.Instance.StartCoroutine(PlayerReplay.Instance.GetPlayerReplays(_levelName, (replays) => {
                ReplaysRecieved(replays);

                _ghostPlaybacks = new List<GhostPlaybackController>();

                for (int i = 0; i < _replays.Count; i++)
                {
                    GhostPlaybackController ghostPlayback = (GhostPlaybackController)Instantiate(GhostPlayback);
                    ghostPlayback.transform.SetParent(GhostHolder.transform);
                    ghostPlayback.SetPlayerReplayModel(_replays[i]);
                    _ghostPlaybacks.Add(ghostPlayback);
                }
                
                //Player.ResetPlayer();
                GrappleUI.Hide();
                VirtualCamera.GetCinemachineComponent<CinemachineComposer>().m_ScreenX = 0.5f;
                VirtualBackgroundCamera.GetCinemachineComponent<CinemachineComposer>().m_ScreenX = 0.5f;
                Player.EnableFullScreenInput();
                StartCoroutine(StartCountDown());
            }));
        }));
    }

    IEnumerator StartCountDown()
    {
        int counter = 3;

        Player.Enable(true);
        for (int i = 0; i < _ghostPlaybacks.Count; i++)
            _ghostPlaybacks[i].StartPlayGhostPlayback();

        GrappleUI.ShowCountdown();

        while (counter > 0)
        {
            yield return new WaitForSeconds(1);
            counter--;
            GrappleUI.UpdateCountdown(counter);
        }

        GrappleUI.HideCountdown();

        Level.HideBarrier();
        Level.StartDeathMarch();
        _mainCamera.transform.position = _mainCameraStartPosition;
        _gameOn = true;

        yield return null;
    }

    void PlayerCaught()
    {
        if (_gameOn)
        {
            _gameOn = false;
            Level.SetSectionCreator(Bug.Bug);
            PlayerReplayModel currentReplay = Player.PlayerCompleted(_levelName, GrappleUI.Timer);
            GrappleUI.InitPlayerRanksScreen(_replays, currentReplay);
            GrappleUI.StopTimer();
            Invoke("ShowEndScreen", 2.0f);
        }
    }

    void ShowEndScreen()
    {
        VirtualCamera.GetCinemachineComponent<CinemachineComposer>().m_ScreenX = UICameraOffset;
        VirtualBackgroundCamera.GetCinemachineComponent<CinemachineComposer>().m_ScreenX = UICameraOffset;
        GrappleUI.EndGame();
    }

    void GameFinished()
    {
        Level.StopDeathMarch();
        Level.ShowBarrier();
        Player.ResetPlayer();
        _levelName = _startLevelName;
        Level.SetSectionCreator(Player.gameObject);
        Level.Reset();

        for (int i = 0; i < GhostHolder.transform.childCount; i++)
            Destroy(GhostHolder.transform.GetChild(i).gameObject);

        //StartCoroutine("WaitForPlayerInput");

        //GrappleServerData.Instance.StartCoroutine(GrappleServerData.Instance.AddLevel(_levelName, (Success, ReturnString) =>
        //{
        //    if (!Success && ReturnString != "")
        //    {
        //        Debug.Log("Server Error: " + ReturnString);
        //        return;
        //    }
        //    PlayerReplay.Instance.StartCoroutine(PlayerReplay.Instance.GetPlayerReplays(_levelName, (replays) =>
        //    {
        //        ReplaysRecieved(replays);
        //        if (replays.Count > 0)
        //            StartCoroutine("ReleaseGhosts");
        //    }));
        //}));
    }

    void ProcessNewUsername(string username)
    {
        if(username == string.Empty || username == "")
        {
            GrappleUI.SetErrorText("ENTER SOMETHING PLEASE...");
            return;
        }
            
        GrappleServerData.Instance.StartCoroutine(GrappleServerData.Instance.AddUser(username, (Success, ReturnString) => {
            if(!Success && !string.IsNullOrEmpty(ReturnString))
            {
                Debug.Log("Server Error: " + ReturnString);
                _username = username;
                GrappleUI.NoUsernameScreen.SetActive(false);
                GrappleUI.StartScreen.SetActive(true);
                GrappleUI.ConfigScreen.SetActive(false);
                GrappleUI.UserName.text = _username;
                GrappleUI.UserEdit.SetActive(true);
                GrappleUI.UserInput.gameObject.SetActive(false);
                GrappleUI.UserInput.text = "";
                GrappleUI.UserSave.SetActive(false);
                GrappleUI.UserCancel.SetActive(false);
                GrappleUI.SetErrorText("");
            }
            else if(Success)
            {
                _username = username;
                GrappleUI.NoUsernameScreen.SetActive(false);
                GrappleUI.StartScreen.SetActive(true);
                GrappleUI.ConfigScreen.SetActive(false);
                GrappleUI.UserName.text = _username;
                GrappleUI.UserEdit.SetActive(true);
                GrappleUI.UserInput.gameObject.SetActive(false);
                GrappleUI.UserInput.text = "";
                GrappleUI.UserSave.SetActive(false);
                GrappleUI.UserCancel.SetActive(false);
                GrappleUI.SetErrorText("");
                PlayerPrefs.SetString(Constants.USERNAME_KEY, _username);
            }
            else
            {
                GrappleUI.SetErrorText("USERNAME HAS ALREADY BEEN SELECTED, PLEASE CHOOSE AGAIN...");
            }
        }));
    }

    void GhostsValueChanged(int value)
    {
        PlayerPrefs.SetInt(Constants.GHOSTS, value);
    }

    void GhostRecordsValueChanged(int value)
    {
        PlayerPrefs.SetInt(Constants.NUM_OF_LOCAL_GHOST_RECORDS, value);
        PlayerReplay.InitLocalRecords();
        GrappleUI.TotalGhostRecordsLocal.text = PlayerReplay.NumOfLocalRecords.ToString();
    }

    void CameraWaypointsChanged(List<CinemachineSmoothPath.Waypoint> waypoints)
    {
        SmoothPath.m_Waypoints = waypoints.ToArray();
        SmoothPath.InvalidateDistanceCache();
    }

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
    }

    void ResetGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void ResetData()
    {
        PlayerReplay.ClearData();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void OnEnable()
	{
		GrappleUI.OnStartButtonClicked += StartGame;
        GrappleUI.OnDoneButtonClicked += GameFinished;
        GrappleUI.OnResetButtonClicked += ResetGame;
        GrappleUI.OnResetDataButtonClicked += ResetData;
        GrappleUI.OnGhostsValueChanged += GhostsValueChanged;
        GrappleUI.OnGhostRecordsValueChanged += GhostRecordsValueChanged;
		GrappleUI.OnUserSaveButtonClicked += ProcessNewUsername;
        Level.OnCameraWaypointsChanged += CameraWaypointsChanged;
        Player.OnPlayerCaught += PlayerCaught;
    }

    void OnDisable()
	{
        GrappleUI.OnStartButtonClicked -= StartGame;
        GrappleUI.OnDoneButtonClicked -= GameFinished;
        GrappleUI.OnResetButtonClicked -= ResetGame;
        GrappleUI.OnResetDataButtonClicked -= ResetData;
        GrappleUI.OnGhostsValueChanged -= GhostsValueChanged;
        GrappleUI.OnGhostRecordsValueChanged -= GhostRecordsValueChanged;
		GrappleUI.OnUserSaveButtonClicked -= ProcessNewUsername;
        Level.OnCameraWaypointsChanged -= CameraWaypointsChanged;
        Player.OnPlayerCaught += PlayerCaught;
    }
}
