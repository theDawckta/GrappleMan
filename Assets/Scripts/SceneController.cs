using System.Collections;
using System.Collections.Generic;
using System.IO;
using Grappler.DataModel;
using UnityEngine;
using UnityEngine.UI;

public class SceneController : MonoBehaviour
{
    public PlayerController Player;
    public UIController GrappleUI;
    public PlayerRecorderController PlayerRecorder;
	public GhostPlaybackController GhostPlayback;
	public GameObject Ghosts;
	public int NumberOfGhosts = 6;

    private AudioSource _playerAudio;
    private AudioClip _song;
    private Camera _mainCamera;
    private Vector3 _mainCameraStartPosition;
    private List<PlayerPlaybackModel> _playerPlaybacks = new List<PlayerPlaybackModel>();
    private bool _gameOn = false;

    void Awake()
    {
        Application.targetFrameRate = 60;
        _playerAudio = GetComponent<AudioSource>();
        _song = Resources.Load("Songs/BeatOfTheTerror") as AudioClip;
        _playerAudio.clip = _song;
        _playerAudio.loop = true;
        _mainCamera = Camera.main;
        _mainCameraStartPosition = _mainCamera.transform.position;
		_playerPlaybacks = PlayerPlaybackController.GetPlayerPlaybackLocal(NumberOfGhosts);
        PlayerPlaybackController.Init();
    }

    void Start()
    {
        GrappleUI.GhostsInput.text = NumberOfGhosts.ToString();
        GrappleUI.GhostRecordsInput.text = PlayerPlaybackController.MaxNumOfRecords.ToString();
        //_playerAudio.Play();
    }

	private void StartGame()
    {
    	List<GameObject> ghosts = new List<GameObject>();
		_playerPlaybacks = PlayerPlaybackController.GetPlayerPlaybackLocal(NumberOfGhosts);
        int numOfGhosts = (_playerPlaybacks.Count < NumberOfGhosts) ? _playerPlaybacks.Count : NumberOfGhosts;
		_mainCamera.transform.position = _mainCameraStartPosition;

		for(int i = Ghosts.transform.childCount - 1; i >= 0 ; i--)
			Destroy(Ghosts.transform.GetChild(i).gameObject);

        for (int i = 0; i < numOfGhosts; i++)
		{
            if (_playerPlaybacks[i] != null)
            {
                if (_playerPlaybacks[i].Time > 0.0f)
                {
                    GhostPlaybackController ghostPlayback = (GhostPlaybackController)Instantiate(GhostPlayback);
                    ghostPlayback.transform.SetParent(Ghosts.transform);
                    ghostPlayback.StartPlayGhostPlayback(_playerPlaybacks[i]);
                }
            }
		}

        Player.Init();

        if(!_gameOn)
			_gameOn = true;
	}

    void ResetData()
    {
        PlayerPlaybackController.ClearData();
    }

    void GhostsValueChanged(int value)
    {
        NumberOfGhosts = value;
    }

    void GhostRecordsValueChanged(int value)
    {
        PlayerPlaybackController.SetNumOfRecords(value);
        PlayerPlaybackController.Init();
    }

	void PlayerFinished(PlayerPlaybackModel playerPlayback, bool playerCompleted)
    {
		if(_gameOn)
		{
			GrappleUI.EndGame();
			PlayerPlaybackController.SavePlayerPlaybackLocal(playerPlayback, playerCompleted);
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
        GrappleUI.OnResetButtonClicked += ResetData;
        GrappleUI.OnGhostsValueChanged += GhostsValueChanged;
        GrappleUI.OnGhostRecordsValueChanged += GhostRecordsValueChanged;
        Player.OnPlayerCompleted += PlayerFinished;
		Player.OnPlayerDied += PlayerFinished;
    }

	void OnDisable()
	{
        GrappleUI.OnStartButtonClicked -= StartGame;
        GrappleUI.OnResetButtonClicked -= ResetData;
        GrappleUI.OnGhostsValueChanged -= GhostsValueChanged;
        GrappleUI.OnStartButtonClicked -= StartGame;
		Player.OnPlayerCompleted -= PlayerFinished;
		Player.OnPlayerDied -= PlayerFinished;
    }
}
