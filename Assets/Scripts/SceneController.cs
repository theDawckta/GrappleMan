using System.Collections.Generic;
using Grappler;
using Grappler.Data;
using Grappler.Util;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public PlayerController Player;
    public UIController GrappleUI;
    public PlayerRecorderController PlayerRecorder;
	public GhostPlaybackController GhostPlayback;
    public WaypointController Waypoint;
	public GameObject GhostHolder;
	public GameObject LevelHolder;

    private AudioSource _playerAudio;
    private AudioClip _song;
    private Camera _mainCamera;
    private Vector3 _mainCameraStartPosition;
    private List<GhostPlaybackController> _ghostPlaybacks = new List<GhostPlaybackController>();
    private bool _gameOn = false;
	private string _username = "User";
	private GameObject _currentLevel;
	private string _levelName;

    void Awake()
    {
        Application.targetFrameRate = 60;
        _playerAudio = GetComponent<AudioSource>();
        _song = Resources.Load("Songs/BeatOfTheTerror") as AudioClip;
        _playerAudio.clip = _song;
        _playerAudio.loop = true;
        _mainCamera = Camera.main;
        _mainCameraStartPosition = _mainCamera.transform.position;
        PlayerReplay.InitLocalRecords();
		_username = PlayerPrefs.GetString(Constants.USERNAME_KEY);
    }

    void Start()
    {
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
			GrappleUI.NoUsernameScreen.SetActive(true);
        else
        {
			GrappleUI.UserName.text = _username;
        	GrappleUI.StartScreen.SetActive(true);
        }
        //_playerAudio.Play();
    }

	private void InitPlayerRankScreen(string levelName)
	{
        _levelName = levelName;

        GrappleServerData.Instance.StartCoroutine(GrappleServerData.Instance.AddLevel(_levelName, (NewLevel) => {
            PlayerReplay.Instance.StartCoroutine(PlayerReplay.Instance.GetPlayerReplays(levelName, (replays)=> {
		        ReplaysRecieved(replays);
	        }));
        }));
	}

	void ReplaysRecieved(List<PlayerReplayModel> replays)
	{
		int tempNumOfGhosts = (replays.Count < PlayerPrefs.GetInt(Constants.GHOSTS)) ? replays.Count : PlayerPrefs.GetInt(Constants.GHOSTS);
		_currentLevel = (GameObject)Instantiate(Resources.Load ("Levels/" + _levelName));

		foreach (Transform child in LevelHolder.transform) 
			GameObject.Destroy(child.gameObject);
		
		_currentLevel.transform.SetParent (LevelHolder.transform, false);

        for (int i = 0; i < _ghostPlaybacks.Count; i++)
        {
            if (_ghostPlaybacks[i] != null)
            {
                Destroy(_ghostPlaybacks[i].gameObject);
            }
        }

        _ghostPlaybacks = new List<GhostPlaybackController>();

		for (int i = 0; i < tempNumOfGhosts; i++)
		{
			GhostPlaybackController ghostPlayback = (GhostPlaybackController)Instantiate(GhostPlayback);
			ghostPlayback.transform.SetParent(GhostHolder.transform);
			ghostPlayback.SetPlayerReplayModel(replays[i]);
			_ghostPlaybacks.Add(ghostPlayback);
		}

		GrappleUI.InitPlayerRanksScreen (replays);
		GrappleUI.StartButton.gameObject.SetActive (true);
	}

	private void StartGame()
	{
        for (int i = 0; i < _ghostPlaybacks.Count; i++)
            _ghostPlaybacks[i].StartPlayGhostPlayback();

        Player.Init();
        _mainCamera.transform.position = _mainCameraStartPosition;
        Waypoint.Init(Player.transform.position, 1);
        Player.SetArrowDestination(Waypoint.GateCollider.transform.position);
        _gameOn = true;
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

    void PlayerFinished()
    {
        if (_gameOn)
        {
            PlayerReplayModel playerReplay = Player.PlayerCompleted();
            Player.Disable();
            GrappleUI.EndGame();
            _gameOn = false;
            playerReplay.LevelName = _levelName;
            PlayerReplay.Instance.StartCoroutine(PlayerReplay.SavePlayerPlayback(playerReplay, (Success) => {
                // placeholder if needed
            }));
        }
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

    void ProcessNewUsername(string username)
    {
        if(username == string.Empty || username == "")
        {
            GrappleUI.SetErrorText("ENTER SOMETHING PLEASE...");
            return;
        }
            
        GrappleServerData.Instance.StartCoroutine(GrappleServerData.Instance.AddUser(username, (Success) => {
            if(Success)
            {
                _username = username;
                GrappleUI.NoUsernameScreen.SetActive(false);
                GrappleUI.StartScreen.SetActive(true);
                GrappleUI.UserEdit.SetActive(true);
                GrappleUI.UserInput.gameObject.SetActive(false);
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

    void UsernameProcessed(string username)
    {
		GrappleUI.NoUsernameScreen.SetActive(false);
		GrappleUI.StartScreen.SetActive(true);
		if(username != string.Empty)
		{
			_username = username;
			GrappleUI.UserName.text = _username;
			PlayerPrefs.SetString(Constants.USERNAME_KEY, _username);
		}

		GrappleUI.UserEdit.SetActive(true);
		GrappleUI.UserInput.gameObject.SetActive(false);
		GrappleUI.UserSave.SetActive(false);
		GrappleUI.UserCancel.SetActive(false);
    }

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
    }

    void OnGatesPassed()
    {
        Player.SetArrowDestination(Waypoint.GateCollider.transform.position);
    }

    void OnWaypointHidden()
    {
        Player.PlayerArrow.SetActive(true);
    }

    void OnWaypointVisible()
    {
        Player.PlayerArrow.SetActive(false);
    }

    void OnEnable()
	{
		GrappleUI.OnStartButtonClicked += StartGame;
		GrappleUI.OnLevelSelectButtonClicked += InitPlayerRankScreen;
		GrappleUI.OnResetButtonClicked += ResetGame;
        GrappleUI.OnResetDataButtonClicked += ResetData;
        GrappleUI.OnGhostsValueChanged += GhostsValueChanged;
        GrappleUI.OnGhostRecordsValueChanged += GhostRecordsValueChanged;
		GrappleUI.OnUserSaveButtonClicked += ProcessNewUsername;
        Waypoint.OnGatesPassed += OnGatesPassed;
        Waypoint.OnWaypointVisible += OnWaypointVisible;
        Waypoint.OnWaypointHidden += OnWaypointHidden;
        Waypoint.OnGatesFinished += PlayerFinished;
    }

	void OnDisable()
	{
        GrappleUI.OnStartButtonClicked -= StartGame;
		GrappleUI.OnLevelSelectButtonClicked -= InitPlayerRankScreen;
        GrappleUI.OnResetButtonClicked -= ResetGame;
        GrappleUI.OnResetDataButtonClicked -= ResetData;
        GrappleUI.OnGhostsValueChanged -= GhostsValueChanged;
        GrappleUI.OnGhostRecordsValueChanged -= GhostRecordsValueChanged;
		GrappleUI.OnUserSaveButtonClicked -= ProcessNewUsername;
        Waypoint.OnGatesPassed -= OnGatesPassed;
        Waypoint.OnWaypointVisible -= OnWaypointVisible;
        Waypoint.OnWaypointHidden -= OnWaypointHidden;
        Waypoint.OnGatesFinished -= PlayerFinished;
    }
}
