using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;

public class LevelController : MonoBehaviour
{
    public PlayerController _PlayerController;
    public UIController _UIController;
    public SideScrollerGenerator _SideScrollerGenerator;
    public SmoothFollow _SmoothFollow;
    public GameObject StartPlatform;
    public GameObject LevelBounds;
    [HideInInspector]
    public bool GameOn = false;

    private AudioSource playerAudio;
    private AudioClip song;
    private int levelSections = 0;

    void Awake()
    {
        playerAudio = GetComponent<AudioSource>();
        song = Resources.Load("Songs/BeatOfTheTerror") as AudioClip;

        playerAudio.clip = song;
        playerAudio.loop = true;
        SetVolume(_UIController.Volume.value);
    }
	void Start () 
    {
	    _PlayerController.OnPlayerDied += _PlayerController_OnPlayerDied;
        _PlayerController.OnPlayerStarted += _PlayerController_OnPlayerStarted;
        _SideScrollerGenerator.Init(_UIController.SeedInputField.text);
        Init();
	}
	
	void Update () 
    {
		if(_PlayerController.transform.position.x > (levelSections * _SideScrollerGenerator.TotalLength) - _SideScrollerGenerator.TotalLength / 2)
		{
 			AddLevelSection();
		}

        if (Input.GetKeyDown("-"))
        {
            if (_SmoothFollow.distance > 10)
                _SmoothFollow.distance = _SmoothFollow.distance - 10;
        }

        if (Input.GetKeyDown("="))
        {
            _SmoothFollow.distance = _SmoothFollow.distance + 10;
        }
	}

    public void Init()
    {
        playerAudio.Play();
        StartPlatform.SetActive(true);
        _PlayerController.Init();
        _SideScrollerGenerator.Init(_SideScrollerGenerator.seed);
        if(_UIController.ParentCanvas.gameObject.activeSelf == false)
            _PlayerController.HookPlayerInput.InputActive = true;
		Time.timeScale = 1.0f;
        levelSections = 0;
		AddLevelSection();
    }

    public void AddLevelSection()
    {
		_SideScrollerGenerator.MakeLevel(Int32.Parse(_UIController.WidthMin.text), 
                                  Int32.Parse(_UIController.WidthMax.text), 
                                  Int32.Parse(_UIController.HeightMin.text), 
                                  Int32.Parse(_UIController.HeightMax.text), 
                                  Int32.Parse(_UIController.DepthMin.text), 
                                  Int32.Parse(_UIController.DepthMax.text),
                                  Int32.Parse(_UIController.EnemyMin.text),
                                  Int32.Parse(_UIController.EnemyMax.text),
                                  new Vector3(levelSections * _SideScrollerGenerator.TotalLength, 0.0f, 0.0f));

		LevelBounds.transform.position = new Vector3(((levelSections + 1)  * _SideScrollerGenerator.TotalLength) / 2, 0.0f, 0.0f);
		LevelBounds.transform.localScale = new Vector3(levelSections + 1, 1.0f, 1.0f);

        levelSections = levelSections + 1;
    }

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
    }

    public void PlayerReady()
    {
        _PlayerController.HookPlayerInput.InputActive = true;
        _UIController.ToggleUIController();
    }

    void _PlayerController_OnPlayerDied()
    {
        _UIController.EndGame();
        Time.timeScale = 0.0f;
        _PlayerController.HookPlayerInput.InputActive = false;
        GameOn = false;
    }

    void _PlayerController_OnPlayerStarted()
    {
        GameOn = true;
        StartPlatform.SetActive(false);
    }

    void _PlayerController_OnPlayerWon()
    {
        // just leaving this here, not sure if the player will be able to actually win
    }

    void OnDestroy()
    {
        _PlayerController.OnPlayerDied -= _PlayerController_OnPlayerDied;
        _PlayerController.OnPlayerStarted -= _PlayerController_OnPlayerStarted;
    }
}
