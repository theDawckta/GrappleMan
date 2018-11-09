using System.Collections.Generic;
using Grappler;
using Grappler.Data;
using Grappler.Util;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class SceneController : MonoBehaviour
{
    public PlayerController Player;
    public UIController GrappleUI;
    public PlayerRecorderController PlayerRecorder;
	public GhostPlaybackController GhostPlayback;
	public GameObject GhostHolder;
	public GameObject LevelHolder;

    private WaypointController _currentLevelWaypoint;
    private AudioSource _playerAudio;
    private AudioClip _song;
    private Camera _mainCamera;
    private Vector3 _mainCameraStartPosition;
    private List<GhostPlaybackController> _ghostPlaybacks = new List<GhostPlaybackController>();
    private bool _gameOn = false;
	private string _username = "User";
	private GameObject _currentLevel;
	private string _levelName;
    private Renderer[] _levelRenderers;
    private float _hideShowTime = 0.5f;

    void Awake()
    {
        _levelRenderers = LevelHolder.GetComponentsInChildren<Renderer>();
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
            GrappleUI.Show();
        }

        Player.Enable();
        //_playerAudio.Play();
    }

	private void InitPlayerRankScreen(string levelName)
	{
        _levelName = levelName;

        LoadLevel(_levelName);

        Player.Init();

        GrappleServerData.Instance.StartCoroutine(GrappleServerData.Instance.AddLevel(_levelName, (NewLevel) => {
            PlayerReplay.Instance.StartCoroutine(PlayerReplay.Instance.GetPlayerReplays(levelName, (replays)=> {
		        ReplaysRecieved(replays);
	        }));
        }));
	}

    private void LoadLevel(string levelName)
    {
        HideLevel();
    }

    void HideLevel()
    {
        for (int i = 0; i < _levelRenderers.Length; i++)
        {
            Color endColor = new Color(_levelRenderers[i].material.color.r, _levelRenderers[i].material.color.g, _levelRenderers[i].material.color.b, 0.0f);
            if(i < _levelRenderers.Length - 1)
                _levelRenderers[i].material.DOColor(endColor, _hideShowTime);
            else
            {
                _levelRenderers[i].material.DOColor(endColor, _hideShowTime).OnComplete(() => {
                    foreach (Transform child in LevelHolder.transform)
                        GameObject.Destroy(child.gameObject);

                    if (_currentLevelWaypoint != null)
                    {
                        _currentLevelWaypoint.OnGatesPassed -= OnGatesPassed;
                        _currentLevelWaypoint.OnWaypointVisible -= OnWaypointVisible;
                        _currentLevelWaypoint.OnWaypointHidden -= OnWaypointHidden;
                        _currentLevelWaypoint.OnGatesFinished -= PlayerFinished;
                    }
                });
            } 
        }

        Invoke("ShowLevel", 1.5f);
    }

    public void ShowLevel()
    {
        _currentLevel = (GameObject)Instantiate(Resources.Load("Levels/" + _levelName));
        _currentLevel.SetActive(false);
        _currentLevel.transform.SetParent(LevelHolder.transform, false);
        _levelRenderers = LevelHolder.GetComponentsInChildren<Renderer>(true);

        Color hideColor = new Color(_levelRenderers[0].material.color.r, _levelRenderers[0].material.color.g, _levelRenderers[0].material.color.b, 0.0f);
        Color showColor = new Color(_levelRenderers[0].material.color.r, _levelRenderers[0].material.color.g, _levelRenderers[0].material.color.b, 1.0f);

        for (int i = 0; i < _levelRenderers.Length; i++)
        {
            _levelRenderers[i].material.color = hideColor;
        }

        _currentLevelWaypoint = _currentLevel.GetComponentInChildren<WaypointController>();
        _currentLevelWaypoint.OnGatesPassed += OnGatesPassed;
        _currentLevelWaypoint.OnWaypointVisible += OnWaypointVisible;
        _currentLevelWaypoint.OnWaypointHidden += OnWaypointHidden;
        _currentLevelWaypoint.OnGatesFinished += PlayerFinished;

        _currentLevel.SetActive(true);

        for (int j = 0; j < _levelRenderers.Length; j++)
        {
            _levelRenderers[j].material.DOColor(showColor, _hideShowTime).SetEase(Ease.Linear);
        }
    }

    void ReplaysRecieved(List<PlayerReplayModel> replays)
	{
		int tempNumOfGhosts = (replays.Count < PlayerPrefs.GetInt(Constants.GHOSTS)) ? replays.Count : PlayerPrefs.GetInt(Constants.GHOSTS);

        for (int i = 0; i < _ghostPlaybacks.Count; i++)
        {
            if (_ghostPlaybacks[i] != null)
            {
                _ghostPlaybacks[i].Kill();
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
	}

	private void StartGame()
	{
        for (int i = 0; i < _ghostPlaybacks.Count; i++)
            _ghostPlaybacks[i].StartPlayGhostPlayback();

        Player.Enable(true);
        _currentLevelWaypoint.Init(Player.transform.position);
        Player.SetArrowDestination(_currentLevelWaypoint.GateCollider.transform.position);
        _mainCamera.transform.position = _mainCameraStartPosition;

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
            Player.DisableLeftScreenInput();
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
                GrappleUI.LevelSelectScreen.SetActive(true);
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
		GrappleUI.LevelSelectScreen.SetActive(true);
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
        Player.SetArrowDestination(_currentLevelWaypoint.GateCollider.transform.position);
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
    }
}
