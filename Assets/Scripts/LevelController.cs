using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelController : MonoBehaviour {
    
    public PressureController _PressureController;
    public PlayerController _PlayerController;
    public GameObject StartPlatform;

	// Use this for initialization
	void Start () {
	    _PlayerController.OnPlayerDied += _PlayerController_OnPlayerDied;
        _PlayerController.OnPlayerStarted += _PlayerController_OnPlayerStarted;
	}
	
	// Update is called once per frame
	void Update () {
	}

    void _PlayerController_OnPlayerDied()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void _PlayerController_OnPlayerStarted()
    {
        StartPlatform.SetActive(false);
        _PressureController.LavaFlow();
    }

    void OnDestroy()
    {
        _PlayerController.OnPlayerDied -= _PlayerController_OnPlayerDied;
        _PlayerController.OnPlayerStarted -= _PlayerController_OnPlayerStarted;
    }
}
