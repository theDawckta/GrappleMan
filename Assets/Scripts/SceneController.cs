using System.Collections.Generic;
using Grappler.DataModel;
using UnityEngine;
using UnityEngine.SceneManagement;
using Grappler.Constants;

public class SceneController : MonoBehaviour
{
    public PlayerController Player;
    public UIController GrappleUI;
    public PlayerRecorderController PlayerRecorder;
	public GhostPlaybackController GhostPlayback;
	public GameObject GhostHolder;
	public GrappleDataController GrappleData;

    private AudioSource _playerAudio;
    private AudioClip _song;
    private Camera _mainCamera;
    private Vector3 _mainCameraStartPosition;
    private List<PlayerReplayModel> _playerPlaybacks = new List<PlayerReplayModel>();
    private List<bool> _playerPlaybackInUse = new List<bool>();
    private List<GhostPlaybackController> _ghostPlaybacks = new List<GhostPlaybackController>();
    private int _numberOfGhosts = 6;
    private int _playerPlaybackIndex = 0;
    private bool _gameOn = false;
	private string _username = "";

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
        PlayerReplayController.Init();
		_username = PlayerPrefs.GetString(Constants.USERNAME_KEY);
    }

    void Start()
    {
        if (PlayerPrefs.HasKey(Constants.GHOSTS))
        {
            GrappleUI.GhostsInput.text = PlayerPrefs.GetInt(Constants.GHOSTS).ToString();
            _numberOfGhosts = PlayerPrefs.GetInt(Constants.GHOSTS);
        }  
        else
            PlayerPrefs.SetInt(Constants.GHOSTS, _numberOfGhosts);

        GrappleUI.GhostRecordsInput.text = PlayerReplayController.MaxNumOfRecords.ToString();
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
        for (int i = 0; i < _ghostPlaybacks.Count; i++)
        {
            _playerPlaybacks[i].SetStateIndex(0);
            _ghostPlaybacks[i].StartPlayGhostPlayback(_playerPlaybacks[_playerPlaybackIndex]);
            _playerPlaybackInUse[i] = true;
            _playerPlaybackIndex++;
            if (_playerPlaybackIndex >= _playerPlaybacks.Count)
                _playerPlaybackIndex = 0;
        }

        Player.Init();
        _mainCamera.transform.position = _mainCameraStartPosition;
		_gameOn = true;
	}

    void InitGhosts()
    {
        _playerPlaybackIndex = 0;
        int tempNumOfGhosts = _numberOfGhosts;

        for (int i = 0; i < _ghostPlaybacks.Count; i++)
        {
            _ghostPlaybacks[i].OnGhostCompleted -= GhostCompleted;
            _ghostPlaybacks[i].StopAllCoroutines();
            Destroy(_ghostPlaybacks[i].gameObject);
        }
        _ghostPlaybacks = new List<GhostPlaybackController>();

        _playerPlaybacks = PlayerReplayController.GetPlayerPlaybackLocal(_numberOfGhosts);
        tempNumOfGhosts = (_playerPlaybacks.Count < _numberOfGhosts) ? _playerPlaybacks.Count : _numberOfGhosts;

        for (int i = 0; i < tempNumOfGhosts; i++)
        {
            if (_playerPlaybacks[i] != null)
            {
                if (_playerPlaybacks[i].ReplayTime > 0.0f)
                {
                    GhostPlaybackController ghostPlayback = (GhostPlaybackController)Instantiate(GhostPlayback);
                    ghostPlayback.transform.SetParent(GhostHolder.transform);
                    ghostPlayback.OnGhostCompleted += GhostCompleted;
                    _ghostPlaybacks.Add(ghostPlayback);
                }
            }
        }

        for (int i = 0; i < _playerPlaybacks.Count; i++)
        {
            _playerPlaybackInUse.Add(false);
        }
    }

	void ResetGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void ResetData()
    {
        PlayerReplayController.ClearData();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void GhostCompleted(GhostPlaybackController ghost, PlayerReplayModel playerPlayback)
    {
        // find the playerPlaybackModel this ghost was using and release it, add ghost to que to be restarted
        for (int i = 0; i < _playerPlaybacks.Count; i++)
        {
            if (playerPlayback == _playerPlaybacks[i])
            {
                _playerPlaybackInUse[i] = false;
                _playerPlaybacks[i].SetStateIndex(0);
                break;
            }
        }
        GetNextPlayerPlaybackIndex();
        ghost.StartPlayGhostPlayback(_playerPlaybacks[_playerPlaybackIndex]);
        _playerPlaybackInUse[_playerPlaybackIndex] = true;
    }

    public void GetNextPlayerPlaybackIndex()
    {
        // recursive, keeps incrementing _playerPlaybackIndex until it finds one that isn't being used
        _playerPlaybackIndex++;
        if (_playerPlaybackIndex >= _playerPlaybacks.Count)
            _playerPlaybackIndex = 0;

        if (_playerPlaybackInUse[_playerPlaybackIndex])
            GetNextPlayerPlaybackIndex();
    }

    void GhostsValueChanged(int value)
    {
        _numberOfGhosts = value;
        PlayerPrefs.SetInt(Constants.GHOSTS, _numberOfGhosts);
        InitGhosts();
    }

    void GhostRecordsValueChanged(int value)
    {
        PlayerReplayController.SetNumOfRecords(value);
        PlayerReplayController.Init();
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
			PlayerReplayController.ProcessPlayerPlayback(playerPlayback, playerCompleted);

            InitGhosts();

            _gameOn = false;
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
