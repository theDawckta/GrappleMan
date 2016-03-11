﻿using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;

public class LevelController : MonoBehaviour {
    
    public PressureController _PressureController;
    public PlayerController _PlayerController;
    public UIController _UIController;
    public LevelGenerator _LevelGenerator;
    public string GameStartString;
    public GameObject StartPlatform;
    public GameObject LevelBounds;
    [HideInInspector]
    public int DistanceTraveled = 0;
    [HideInInspector]
    public bool GameOn = false;

    private Vector3 playerStartPosition;
    private int levelSections;

	void Start () 
    {
	    _PlayerController.OnPlayerDied += _PlayerController_OnPlayerDied;
        _PlayerController.OnPlayerStarted += _PlayerController_OnPlayerStarted;
		AddLevelSection(GameStartString);
		levelSections = 1;
        _UIController.SeedInputField.text = GameStartString;
        playerStartPosition = _PlayerController.transform.position;
	}
	
	void Update () 
    {
        if (_PlayerController.transform.position.x - playerStartPosition.x > DistanceTraveled)
            DistanceTraveled = (int)(_PlayerController.transform.position.x - playerStartPosition.x);
		if(_PlayerController.transform.position.x > (levelSections * _LevelGenerator.TotalLength) + _LevelGenerator.TotalLength / 2)
		{
			Debug.Log("halfway done");
			levelSections = levelSections + 1;
 			AddLevelSection(GameStartString);
		}
	}

    public void Init()
    {
        StartPlatform.SetActive(true);
        _PlayerController.transform.position = playerStartPosition;
        _PlayerController.Init();
        _PressureController.Init();
        _LevelGenerator.Init();
        if(_UIController.ParentCanvas.gameObject.activeSelf == false)
            _PlayerController.HookPlayerInput.InputActive = true;
		Time.timeScale = 1.0f;
		AddLevelSection(GameStartString);
		levelSections = 1;
    }

    public void AddLevelSection(string seed)
    {
		_LevelGenerator.MakeLevel(seed, Int32.Parse(_UIController.WidthMin.text), 
                                                   Int32.Parse(_UIController.WidthMax.text), 
                                                   Int32.Parse(_UIController.HeightMin.text), 
                                                   Int32.Parse(_UIController.HeightMax.text), 
                                                   Int32.Parse(_UIController.DepthMin.text), 
                                                   Int32.Parse(_UIController.DepthMax.text),
                                                   new Vector3((levelSections * _LevelGenerator.TotalLength) + (_LevelGenerator.TotalLength / 2), 0.0f, 0.0f));

		LevelBounds.transform.position = new Vector3((levelSections + 1  * _LevelGenerator.TotalLength) / 2, 0.0f, 0.0f);
		LevelBounds.transform.localScale = new Vector3(levelSections + 1, 1.0f, 1.0f);
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
