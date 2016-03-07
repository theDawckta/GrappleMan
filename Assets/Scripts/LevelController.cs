using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelController : MonoBehaviour {
    
    public PressureController _PressureController;
    public PlayerController _PlayerController;
    public UIController _UIController;
    public LevelGenerator _LevelGenerator;
    public string GameStartString;
    public GameObject StartPlatform;
    [HideInInspector]
    public int DistanceTraveled = 0;
    [HideInInspector]
    public bool GameOn = false;

    private Vector3 playerStartPosition;

	void Start () 
    {
	    _PlayerController.OnPlayerDied += _PlayerController_OnPlayerDied;
        _PlayerController.OnPlayerStarted += _PlayerController_OnPlayerStarted;
        _LevelGenerator.MakeLevel(GameStartString);
        _UIController.SeedInputField.text = GameStartString;
        playerStartPosition = _PlayerController.transform.position;
	}
	
	void Update () 
    {
        if (_PlayerController.transform.position.x - playerStartPosition.x > DistanceTraveled)
            DistanceTraveled = (int)(_PlayerController.transform.position.x - playerStartPosition.x);
	}

    public void Init()
    {
        StartPlatform.SetActive(true);
        _PlayerController.transform.position = playerStartPosition;
        _PlayerController.Init();
        if(_UIController.ParentCanvas.gameObject.activeSelf == false)
            _PlayerController.HookPlayerInput.InputActive = true;
        Time.timeScale = 1.0f;
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
        //_PressureController.LavaFlow();
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
