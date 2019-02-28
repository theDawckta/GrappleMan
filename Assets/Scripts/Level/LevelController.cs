using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Enums;
using Cinemachine;
using DG.Tweening;

public class LevelController : MonoBehaviour
{
    public delegate void LevelSetionAdded(List<CinemachineSmoothPath.Waypoint> waypoints);
    public event LevelSetionAdded OnCameraWaypointsChanged;
    public delegate void PlayerHit();
    public event PlayerHit OnPlayerHit;

    public bool UseRandomSeed;
    public List<LevelSectionController> LevelSections = new List<LevelSectionController>();
    public GameObject FrontBarrier;
    public GameObject Pressure;

    private string _seed = "123456";
    private GameObject _player;
    private List<LevelSectionController> _levelSections = new List<LevelSectionController>();
    private SectionType _nextSection;
    private System.Random _pseudoRandom;
    private List<CinemachineSmoothPath.Waypoint> _cameraWaypoints = new List<CinemachineSmoothPath.Waypoint>();
    private Vector3 _pressureStartPosition;
    private List<Vector3> _pressureWayoints = new List<Vector3>();
    private Tween _pressureTween;

    void Start ()
    {
        _pressureStartPosition = Pressure.transform.position;
        Reset();
    }
	
	void Update ()
    {
        if(_player != null && _levelSections.Count > 0)
        {
            while (Vector3.Distance(_levelSections[_levelSections.Count - 1].transform.position, _player.transform.position) < 320.0f)
                LoadSection();
            while (Vector3.Distance(_levelSections[0].transform.position, Pressure.transform.position) > 320.0f)
                DestroySection();
        }
    }

    public void Init(GameObject player)
    {
        _player = player;
        //_pseudoRandom = new System.Random(_seed.GetHashCode());
    }

    public void StartDeathMarch()
    {
        _pressureTween = Pressure.transform.DOPath(_pressureWayoints.ToArray(), 5.0f, PathType.CatmullRom, PathMode.Full3D, 10, Color.green).SetSpeedBased().OnWaypointChange(PressureWaypointChange);
        _pressureTween.timeScale = 2.0f;
    }

    public void UpdateDeathMarchWaypoints(List<Vector3> newWaypoints)
    {
        _pressureWayoints = newWaypoints;
    }

    public void Reset()
    {
        int seed = int.Parse(_seed);
        for (int i = 0; i < _levelSections.Count; i++)
            Destroy(_levelSections[i].gameObject);

        _levelSections.Clear();
        _cameraWaypoints.Clear();
        _pressureWayoints.Clear();

        OnCameraWaypointsChanged(_cameraWaypoints);

        _nextSection = SectionType.START_SECTION;
        Pressure.transform.position = _pressureStartPosition;
        Debug.Log(seed);
        _pseudoRandom = new System.Random(seed);

        LoadSection();
    }

    private void LoadSection()
    {
        Vector3 oldSectionPosition;

        LevelSectionController newSection = Instantiate(LevelSections.Find(x => x.Section == _nextSection), transform);
        _levelSections.Add(newSection);
        if(newSection.Section != SectionType.START_SECTION)
        {
            oldSectionPosition = _levelSections[_levelSections.Count - 2].transform.position;
            newSection.transform.position = new Vector3(oldSectionPosition.x + _levelSections[_levelSections.Count - 2].OffsetX,
                                                        oldSectionPosition.y + _levelSections[_levelSections.Count - 2].OffsetY,
                                                        0.0f);
        }
        
        CinemachineSmoothPath.Waypoint newWaypoint = new CinemachineSmoothPath.Waypoint();
        newWaypoint.position = newSection.CameraPathPoint.transform.position;
        _cameraWaypoints.Add(newWaypoint);
        OnCameraWaypointsChanged(_cameraWaypoints);

        _pressureWayoints.Add(new Vector3(newWaypoint.position.x, newWaypoint.position.y, 0.0f));

        List<SectionType> possibleNextSections = newSection.PossibleNextSections;
        _nextSection = possibleNextSections[_pseudoRandom.Next(0, possibleNextSections.Count - 1)];
    }

    private void DestroySection()
    {
        Destroy(_levelSections[0].gameObject);
        _levelSections.RemoveAt(0);
        _cameraWaypoints.RemoveAt(0);
        OnCameraWaypointsChanged(_cameraWaypoints);
    }

    public void PressureWaypointChange(int waypointIndex)
    {
        DOTween.To(() => _pressureTween.timeScale, x => _pressureTween.timeScale = x, _pressureTween.timeScale * 1.2f, 5.0f);

        if (waypointIndex > 2)
        {
            //float currentTimeScale = _pressureTween.timeScale;
            //Destroy(_levelSections[0].gameObject);
            //_levelSections.RemoveAt(0);
            ////_pressureWayoints.RemoveAt(0);
            //_cameraWaypoints.RemoveAt(0);
            //OnCameraWaypointsChanged(_cameraWaypoints);
            //_pressureTween.Kill();
            //Pressure.transform.position = _pressureWayoints[0];
            //_pressureTween = Pressure.transform.DOPath(_pressureWayoints.ToArray(), 5.0f, PathType.CatmullRom, PathMode.Full3D, 10, Color.green).SetSpeedBased().OnWaypointChange(PressureWaypointChange).Pause();
            ////_pressureTween.GotoWaypoint(1, true);
            //_pressureTween.timeScale = currentTimeScale;
            //_pressureTween.Play();
        }
        else if(waypointIndex > 0)
        {
            float currentTimeScale = _pressureTween.timeScale;
            _pressureWayoints.RemoveAt(0);
            _pressureTween.Kill();
            //Pressure.transform.position = _pressureWayoints[0];
            _pressureTween = Pressure.transform.DOPath(_pressureWayoints.ToArray(), 5.0f, PathType.CatmullRom, PathMode.Full3D, 10, Color.green).SetSpeedBased().OnWaypointChange(PressureWaypointChange);
            //_pressureTween.GotoWaypoint(1, true);
            _pressureTween.timeScale = currentTimeScale;
            //_pressureTween.Play();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        OnPlayerHit();
        _pressureTween.Kill();
    }

    public string GetSeed()
    {
        return _seed;
    }

    public void ShowBarrier()
    {
        FrontBarrier.gameObject.SetActive(true);
    }

    public void HideBarrier()
    {
        FrontBarrier.gameObject.SetActive(false);
    }
}