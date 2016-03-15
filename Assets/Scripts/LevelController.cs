using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;

public class LevelController : MonoBehaviour {
    
    public PressureController _PressureController;
    public PlayerController _PlayerController;
    public UIController _UIController;
    public SideScrollerGenerator _SideScrollerGenerator;
    public SmoothFollow _SmoothFollow;
    public GameObject StartPlatform;
    public GameObject LevelBounds;
    [HideInInspector]
    public int DistanceTraveled = 0;
    [HideInInspector]
    public bool GameOn = false;

    private Vector3 playerStartPosition;
    private int levelSections = 0;

	void Start () 
    {
	    _PlayerController.OnPlayerDied += _PlayerController_OnPlayerDied;
        _PlayerController.OnPlayerStarted += _PlayerController_OnPlayerStarted;
        _SideScrollerGenerator.Init(_UIController.SeedInputField.text);
		AddLevelSection();
       
        playerStartPosition = _PlayerController.transform.position;
	}
	
	void Update () 
    {
        if (_PlayerController.transform.position.x - playerStartPosition.x > DistanceTraveled)
            DistanceTraveled = (int)(_PlayerController.transform.position.x - playerStartPosition.x);
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
        StartPlatform.SetActive(true);
        _PlayerController.transform.position = playerStartPosition;
        _PlayerController.Init();
        _PressureController.Init();
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
                                  new Vector3((levelSections * _SideScrollerGenerator.TotalLength) + (_SideScrollerGenerator.TotalLength / 2), 0.0f, 0.0f));

		LevelBounds.transform.position = new Vector3(((levelSections + 1)  * _SideScrollerGenerator.TotalLength) / 2, 0.0f, 0.0f);
		LevelBounds.transform.localScale = new Vector3(levelSections + 1, 1.0f, 1.0f);

        levelSections = levelSections + 1;
    }

    public void PlayerReady()
    {
        _PlayerController.HookPlayerInput.InputActive = true;
        _UIController.ToggleUIController();
    }

    void _PlayerController_OnPlayerDied()
    {
        _UIController.EndGame();
        _PressureController.LavaFlowStop();
        Time.timeScale = 0.0f;
        _PlayerController.HookPlayerInput.InputActive = false;
        GameOn = false;
    }

    void _PlayerController_OnPlayerStarted()
    {
        GameOn = true;
        StartPlatform.SetActive(false);
//        _PressureController.LavaFlow();
    }

    void _PlayerController_OnPlayerWon()
    {
        GameOn = false;
        Time.timeScale = 0.0f;
        _PlayerController.HookPlayerInput.InputActive = false;
        _PressureController.LavaFlowStop();
    }

    void OnDestroy()
    {
        _PlayerController.OnPlayerDied -= _PlayerController_OnPlayerDied;
        _PlayerController.OnPlayerStarted -= _PlayerController_OnPlayerStarted;
    }
}
