using System.Collections;
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
    private List<PlayerReplayModel> _replays = new List<PlayerReplayModel>();
    private List<GhostPlaybackController> _ghostPlaybacks = new List<GhostPlaybackController>();
    private bool _gameOn = false;
	private string _username = "User";
	private GameObject _currentLevel;
	private string _levelName = "Start";
    private Renderer[] _levelRenderers;
    private float _hideShowTime = 1.0f;
    private bool _playerMoved = false;
    private int _startGhostIndex;
    private float _ghostReleaseInterval = 2.0f;

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
            GrappleUI.LevelSelectScreen.SetActive(true);
        }
        
        Player.Enable();
        Player.Show();
        ShowLevel();
        StartCoroutine("WaitForPlayerInput");
        StartCoroutine("WaitForOpenUI");

        GrappleServerData.Instance.StartCoroutine(GrappleServerData.Instance.AddLevel(_levelName, (Success, ReturnString) => {
            if (!Success && ReturnString != "")
            {
                Debug.Log("Server Error: " + ReturnString);
                return;
            }
            PlayerReplay.Instance.StartCoroutine(PlayerReplay.Instance.GetPlayerReplays(_levelName, (replays) => {
                ReplaysRecieved(replays);
                if(replays.Count > 0)
                    StartCoroutine("ReleaseGhosts");
            }));
        }));
        //_playerAudio.Play();
    }

    IEnumerator WaitForOpenUI()
    {
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

    IEnumerator WaitForPlayerInput()
    {
        while(!Player.HookPlayerInput.TouchStarted && _playerMoved == false)
        {
            yield return null;
        }

        Player.Enable(true);
        _playerMoved = true;
    }

	private void GetReplays(string levelName)
	{
        if(_playerMoved == true)
        {
            Player.PlayerCompleted(_levelName);
            _playerMoved = false;
        }

        _levelName = levelName;

        HideOldLevel();

        Player.Init();

        GrappleServerData.Instance.StartCoroutine(GrappleServerData.Instance.AddLevel(_levelName, (Success, ReturnString) => {
            if(!Success && ReturnString != "")
            {
                Debug.Log("Server Error: " + ReturnString);
                StartGame();
                //GrappleUI.InitPlayerRanksScreen(new List<PlayerReplayModel>());
                return;
            }
            PlayerReplay.Instance.StartCoroutine(PlayerReplay.Instance.GetPlayerReplays(_levelName, (replays)=> {
		        ReplaysRecieved(replays);

                _ghostPlaybacks = new List<GhostPlaybackController>();

                for (int i = 0; i < _replays.Count; i++)
                {
                    GhostPlaybackController ghostPlayback = (GhostPlaybackController)Instantiate(GhostPlayback);
                    ghostPlayback.transform.SetParent(GhostHolder.transform);
                    ghostPlayback.SetPlayerReplayModel(_replays[i]);
                    _ghostPlaybacks.Add(ghostPlayback);
                }

                StartGame();
                //GrappleUI.InitPlayerRanksScreen(replays);
            }));
        }));
	}

    void HideOldLevel()
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

        Invoke("ShowLevel", 2.0f);
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
        StopCoroutine("ReleaseGhosts");
        _replays = replays;
		int tempNumOfGhosts = (replays.Count < PlayerPrefs.GetInt(Constants.GHOSTS)) ? replays.Count : PlayerPrefs.GetInt(Constants.GHOSTS);

        for (int i = 0; i < _ghostPlaybacks.Count; i++)
        {
            if (_ghostPlaybacks[i] != null)
            {
                _ghostPlaybacks[i].Kill();
            }
        }
	}

	private void StartGame()
	{
        GrappleUI.Hide();
        StartCoroutine(StartCountDown());
    }

    IEnumerator StartCountDown()
    {
        int counter = 3;

        Player.Disable();
        GrappleUI.ShowCountdown();

        while (counter > 0)
        {
            yield return new WaitForSeconds(1);
            counter--;
            GrappleUI.UpdateCountdown(counter);
        }

        GrappleUI.HideCountdown();

        for (int i = 0; i < _ghostPlaybacks.Count; i++)
            _ghostPlaybacks[i].StartPlayGhostPlayback();

        Player.Enable(true);
        _currentLevelWaypoint.Init(Player.transform.position);
        Player.SetArrowDestination(_currentLevelWaypoint.GateCollider.transform.position);
        _mainCamera.transform.position = _mainCameraStartPosition;

        _gameOn = true;

        yield return null;
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
            Player.DisableLeftScreenInput();
            PlayerReplayModel currentReplay = Player.PlayerCompleted(_levelName);

            GrappleUI.InitPlayerRanksScreen(_replays, currentReplay);

            GrappleUI.EndGame();
            _gameOn = false;
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
            
        GrappleServerData.Instance.StartCoroutine(GrappleServerData.Instance.AddUser(username, (Success, ReturnString) => {
            if(!Success && !string.IsNullOrEmpty(ReturnString))
            {
                Debug.Log("Server Error: " + ReturnString);
                _username = username;
                GrappleUI.NoUsernameScreen.SetActive(false);
                GrappleUI.LevelSelectScreen.SetActive(true);
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
                GrappleUI.LevelSelectScreen.SetActive(true);
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
		GrappleUI.OnLevelSelectButtonClicked += GetReplays;
		GrappleUI.OnResetButtonClicked += ResetGame;
        GrappleUI.OnResetDataButtonClicked += ResetData;
        GrappleUI.OnGhostsValueChanged += GhostsValueChanged;
        GrappleUI.OnGhostRecordsValueChanged += GhostRecordsValueChanged;
		GrappleUI.OnUserSaveButtonClicked += ProcessNewUsername;
    }

	void OnDisable()
	{
        GrappleUI.OnStartButtonClicked -= StartGame;
		GrappleUI.OnLevelSelectButtonClicked -= GetReplays;
        GrappleUI.OnResetButtonClicked -= ResetGame;
        GrappleUI.OnResetDataButtonClicked -= ResetData;
        GrappleUI.OnGhostsValueChanged -= GhostsValueChanged;
        GrappleUI.OnGhostRecordsValueChanged -= GhostRecordsValueChanged;
		GrappleUI.OnUserSaveButtonClicked -= ProcessNewUsername;
    }
}
