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
	public GameObject GhostHolder;
	public GrappleServerData GrappleData;
	public GameObject LevelHolder;

    private AudioSource _playerAudio;
    private AudioClip _song;
    private Camera _mainCamera;
    private Vector3 _mainCameraStartPosition;
	private List<PlayerReplayModel> _playerReplays = new List<PlayerReplayModel>();
    private List<GhostPlaybackController> _ghostPlaybacks = new List<GhostPlaybackController>();
	private int _playerReplayIndex = 0;
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
        PlayerReplay.Init();
		_username = PlayerPrefs.GetString(Constants.USERNAME_KEY);
    }

    void Start()
    {
    	// first run init
		if (!PlayerPrefs.HasKey(Constants.GHOSTS) || !PlayerPrefs.HasKey(Constants.GHOST_RECORDS))
        {
			PlayerPrefs.SetInt(Constants.GHOSTS, Constants.GHOST_COMPETITORS);
			PlayerPrefs.SetInt(Constants.GHOST_RECORDS, Constants.GHOST_COMPETITORS);
        }
       	
		GrappleUI.GhostsInput.text = PlayerPrefs.GetInt(Constants.GHOSTS).ToString();
		GrappleUI.GhostRecordsInput.text = PlayerPrefs.GetInt(Constants.GHOST_RECORDS).ToString();

        if(PlayerPrefs.GetString(Constants.USERNAME_KEY) == "")
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
		
		PlayerReplay.Instance.StartCoroutine(PlayerReplay.Instance.GetPlayerReplays(levelName, (replays)=>{
			ReplaysRecieved(replays);
		}));
	}

	void ReplaysRecieved(List<PlayerReplayModel> replays)
	{
		int tempNumOfGhosts = (replays.Count < PlayerPrefs.GetInt(Constants.GHOSTS)) ? replays.Count : PlayerPrefs.GetInt(Constants.GHOSTS);
		_playerReplays = replays;
		_currentLevel = (GameObject)Instantiate(Resources.Load ("Levels/" + _levelName));

		foreach (Transform child in LevelHolder.transform) 
			GameObject.Destroy(child.gameObject);
		
		_currentLevel.transform.SetParent (LevelHolder.transform, false);

		_playerReplays = replays;

		InitGhosts ();

		for (int i = 0; i < tempNumOfGhosts; i++)
		{
			GhostPlaybackController ghostPlayback = (GhostPlaybackController)Instantiate(GhostPlayback);
			ghostPlayback.transform.SetParent(GhostHolder.transform);
			ghostPlayback.OnGhostCompleted += GhostCompleted;
			ghostPlayback.SetPlayerReplayModel(_playerReplays[_playerReplayIndex]);
			_ghostPlaybacks.Add(ghostPlayback);
			if(i < tempNumOfGhosts - 1)
				SetNextPlayerPlaybackIndex();
		}

		GrappleUI.InitPlayerRanksScreen (replays);
		GrappleUI.StartButton.gameObject.SetActive (true);
	}

	private void StartGame()
	{
		for (int j = 0; j < _ghostPlaybacks.Count; j++) 
			_ghostPlaybacks [j].StartPlayGhostPlayback ();

		Player.Init(_username);
		_mainCamera.transform.position = _mainCameraStartPosition;
		_gameOn = true;
		GrappleServerData.Instance.StartAddLevel(_levelName);
	}

	void InitGhosts()
	{
		_playerReplayIndex = 0;
		GrappleUI.TotalGhostRecordsLocal.text = PlayerReplay.NumOfCompletedRecords.ToString();
		GrappleUI.TotalGhostRecordsServer.text = PlayerReplay.NumOfCompletedRecords.ToString();

		// destroy current ghostPlaybacks
		for (int i = 0; i < _ghostPlaybacks.Count; i++)
		{
			_ghostPlaybacks[i].OnGhostCompleted -= GhostCompleted;
			_ghostPlaybacks[i].StopAllCoroutines();
			Destroy(_ghostPlaybacks[i].gameObject);
		}
		_ghostPlaybacks = new List<GhostPlaybackController>();
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

    void GhostCompleted(GhostPlaybackController ghost, PlayerReplayModel playerPlayback)
    {
        // find the playerPlaybackModel this ghost was using and release it, add ghost to que to be restarted
        for (int i = 0; i < _playerReplays.Count; i++)
        {
            if (playerPlayback == _playerReplays[i])
            {
				_playerReplays[i].InUse = false;
                _playerReplays[i].SetStateIndex(0);
                break;
            }
        }
        SetNextPlayerPlaybackIndex();
		ghost.SetPlayerReplayModel(_playerReplays [_playerReplayIndex]);
        ghost.StartPlayGhostPlayback();
    }

    public void SetNextPlayerPlaybackIndex()
    {
        // recursive, keeps incrementing _playerPlaybackIndex until it finds one that isn't being used
        _playerReplayIndex++;
        if (_playerReplayIndex >= _playerReplays.Count)
            _playerReplayIndex = 0;

		if (_playerReplays [_playerReplayIndex].InUse)
			SetNextPlayerPlaybackIndex ();
		else
			_playerReplays [_playerReplayIndex].InUse = true;
    }

	public void SetPlayerReplayModel(List<PlayerReplayModel> playerReplayModel)
	{
		_playerReplays = playerReplayModel;
	}

    void GhostsValueChanged(int value)
    {
		PlayerPrefs.SetInt(Constants.GHOSTS, value);
        InitGhosts();
    }

    void GhostRecordsValueChanged(int value)
    {
		PlayerPrefs.SetInt(Constants.GHOST_RECORDS, value);
        PlayerReplay.Init();
        InitGhosts();
    }

    void ProcessNewUsername(string newUsername)
    {
		GrappleData.StartAddUser(newUsername);
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

	void PlayerFinished(PlayerReplayModel playerPlayback, bool playerCompleted)
    {
		if(_gameOn)
		{
			GrappleUI.EndGame();

			_gameOn = false;
			playerPlayback.LevelName = _levelName;
			PlayerReplay.Instance.StartCoroutine(PlayerReplay.SavePlayerPlayback(playerPlayback,(Success)=>{
            	// placeholder if needed
			}));
		}
    }

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
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
		GrappleData.OnUsernameProcessed += UsernameProcessed;
        Player.OnPlayerCompleted += PlayerFinished;
		Player.OnPlayerDied += PlayerFinished;
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
		GrappleData.OnUsernameProcessed -= UsernameProcessed;
        Player.OnPlayerCompleted -= PlayerFinished;
		Player.OnPlayerDied -= PlayerFinished;
    }
}
