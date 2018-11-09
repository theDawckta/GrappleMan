using System.Collections.Generic;
using Grappler;
using Grappler.Data;
using Grappler.Util;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlaygroundSceneController : MonoBehaviour
{
    public PlayerController Player;
    public WaypointController Waypoint;

    private AudioSource _playerAudio;
    private AudioClip _song;
    private Camera _mainCamera;
    private Vector3 _mainCameraStartPosition;

    void Awake()
    {
        Application.targetFrameRate = 60;
        _playerAudio = GetComponent<AudioSource>();
        _song = Resources.Load("Songs/BeatOfTheTerror") as AudioClip;
        _playerAudio.clip = _song;
        _playerAudio.loop = true;
        _mainCamera = Camera.main;
        _mainCameraStartPosition = _mainCamera.transform.position;
    }

    void Start()
    {
        Player.Init();
        //Waypoint.Init(Player.transform.position, 5);
        Player.SetArrowDestination(Waypoint.GateCollider.transform.position);
    }

    void Waypoint_OnGatesPassed()
    {
        Player.SetArrowDestination(Waypoint.GateCollider.transform.position);
    }

    void Waypoint_OnWaypointHidden()
    {
        Player.PlayerArrow.SetActive(true);
    }

    void Waypoint_OnWaypointVisible()
    {
        Player.PlayerArrow.SetActive(false);
    }

    void Waypoint_OnGatesFinished()
    {
        Player.Init();
        //Waypoint.Init(Player.transform.position, 5);
    }

    void OnEnable()
	{
        Waypoint.OnGatesPassed += Waypoint_OnGatesPassed;
        Waypoint.OnWaypointVisible += Waypoint_OnWaypointVisible;
        Waypoint.OnWaypointHidden += Waypoint_OnWaypointHidden;
        Waypoint.OnGatesFinished += Waypoint_OnGatesFinished;
    }

	void OnDisable()
	{
        Waypoint.OnGatesPassed -= Waypoint_OnGatesPassed;
        Waypoint.OnGatesFinished -= Waypoint_OnGatesFinished;
        Waypoint.OnWaypointHidden -= Waypoint_OnWaypointHidden;
        Waypoint.OnGatesFinished -= Waypoint_OnGatesFinished;
    }
}
