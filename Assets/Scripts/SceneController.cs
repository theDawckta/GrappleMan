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

    private AudioSource _playerAudio;
    private AudioClip _song;
    private Camera _mainCamera;
    private Vector3 _mainCameraStartPosition;
    private List<PlayerReplayModel> _playerPlaybacks = new List<PlayerReplayModel>();
    private List<GhostPlaybackController> _ghostPlaybacks = new List<GhostPlaybackController>();
    private int _playerPlaybackIndex = 0;
    private bool _gameOn = false;
	private string _username = "User";

    void Awake()
    {
       //PlayerPrefs.DeleteAll();
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

	  	InitGhosts();

        if(PlayerPrefs.GetString(Constants.USERNAME_KEY) == "")
			GrappleUI.NoUsernameScreen.SetActive(true);
        else
        {
			GrappleUI.UserName.text = _username;
        	GrappleUI.StartScreen.SetActive(true);
        }
        //_playerAudio.Play();
    }

    private void StartGame()
    {
		GrappleServerData.Instance.StartAddLevel (SceneManager.GetActiveScene().name);

        for (int i = 0; i < _ghostPlaybacks.Count; i++)
        {
            _playerPlaybacks[i].SetStateIndex(0);
			_playerPlaybacks[i].InUse = true;
			_ghostPlaybacks[i].OnGhostCompleted += GhostCompleted;


			Debug.Log(_playerPlaybacks[_playerPlaybackIndex].ReplayTime);

            _playerPlaybackIndex++;
            if (_playerPlaybackIndex >= _playerPlaybacks.Count)
                _playerPlaybackIndex = 0;
        }

		if (_ghostPlaybacks [0] == _ghostPlaybacks [1])
			Debug.Log ("ghosts equal");

		if (_playerPlaybacks [0] == _playerPlaybacks [1])
			Debug.Log ("playback equal");

		_ghostPlaybacks[0].StartPlayGhostPlayback(_playerPlaybacks[0]);
		_ghostPlaybacks[1].StartPlayGhostPlayback(_playerPlaybacks[1]);

		Player.Init(_username, SceneManager.GetActiveScene().name);
        _mainCamera.transform.position = _mainCameraStartPosition;
		_gameOn = true;
	}

    void InitGhosts()
    {
        _playerPlaybackIndex = 0;
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

		PlayerReplay.Instance.StartCoroutine(PlayerReplay.Instance.GetPlayerReplays((replays)=>{
			ReplaysRecieved(replays);
		}));
    }

	void ReplaysRecieved(List<PlayerReplayModel> replays)
	{
		int tempNumOfGhosts = (replays.Count < PlayerPrefs.GetInt(Constants.GHOSTS)) ? replays.Count : PlayerPrefs.GetInt(Constants.GHOSTS);
		_playerPlaybacks = replays;

		for (int i = 0; i < tempNumOfGhosts; i++)
		{
			if (replays[i] != null)
			{
				if (replays[i].ReplayTime > 0.0f)
				{
					GhostPlaybackController ghostPlayback = (GhostPlaybackController)Instantiate(GhostPlayback);
					ghostPlayback.transform.SetParent(GhostHolder.transform);
					_ghostPlaybacks.Add(ghostPlayback);
				}
			}
		}
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
        for (int i = 0; i < _playerPlaybacks.Count; i++)
        {
            if (playerPlayback == _playerPlaybacks[i])
            {
				_playerPlaybacks[i].InUse = false;
                _playerPlaybacks[i].SetStateIndex(0);
                break;
            }
        }
        GetNextPlayerPlaybackIndex();
        ghost.StartPlayGhostPlayback(_playerPlaybacks[_playerPlaybackIndex]);
		_playerPlaybacks[_playerPlaybackIndex].InUse = true;
    }

    public void GetNextPlayerPlaybackIndex()
    {
        // recursive, keeps incrementing _playerPlaybackIndex until it finds one that isn't being used
        _playerPlaybackIndex++;
        if (_playerPlaybackIndex >= _playerPlaybacks.Count)
            _playerPlaybackIndex = 0;

		if (_playerPlaybacks[_playerPlaybackIndex].InUse)
            GetNextPlayerPlaybackIndex();
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
            GrappleUI.ToggleStartScreen();
			_gameOn = false;
			PlayerReplay.Instance.StartCoroutine(PlayerReplay.SavePlayerPlayback(playerPlayback,(Success)=>{
            	InitGhosts();
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
