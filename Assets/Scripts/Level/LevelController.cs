﻿using System;
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
    public BugController Bug;
    public List<LevelSectionController> LevelSections = new List<LevelSectionController>();
    public GameObject FrontBarrier;
    public GameObject Pressure;
    public float PressureMaxSpeed = 11.0f;

    private string _seed = "123456";
    private GameObject _player;
    private GameObject _startPlatform;
    private List<LevelSectionController> _levelSections = new List<LevelSectionController>();
    private SectionType _nextSection;
    private System.Random _pseudoRandom;
    private List<CinemachineSmoothPath.Waypoint> _cameraWaypoints = new List<CinemachineSmoothPath.Waypoint>();
    private Vector3 _pressureStartPosition;
    private Quaternion _pressureStartRotation;
    private List<Vector3> _pressureWayoints = new List<Vector3>();
    private Tween _pressureTween;

    void Start ()
    {
        _pressureStartPosition = Pressure.transform.position;
        _pressureStartRotation = Pressure.transform.rotation;
        Reset();
    }
	
	void Update ()
    {
        if(_player != null && _levelSections.Count > 0)
        {
            while (Vector3.Distance(_levelSections[_levelSections.Count - 1].transform.position, _player.transform.position) < 10280.0f)
                LoadSection();
            while (Vector3.Distance(_levelSections[0].transform.position, Pressure.transform.position) > 320.0f)
                DestroySection();
        }
    }

    public void Init(GameObject player)
    {
        _player = player;
        Bug.Init();
    }

    public void StartDeathMarch()
    {
        _pressureTween = Pressure.transform.DOPath(_pressureWayoints.ToArray(), 5.0f, PathType.CatmullRom, PathMode.Full3D, 10, Color.green).SetSpeedBased().OnWaypointChange(PressureWaypointChange).SetLookAt(0.001f, Vector3.left);
        _pressureTween.timeScale = 2.0f;
        _startPlatform.gameObject.SetActive(false);
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


        CinemachineSmoothPath.Waypoint newWaypoint = new CinemachineSmoothPath.Waypoint();
        newWaypoint.position = new Vector3(_pressureStartPosition.x, _pressureStartPosition.y, -80.0f);
        _cameraWaypoints.Add(newWaypoint);
        OnCameraWaypointsChanged(_cameraWaypoints);

        _nextSection = SectionType.START_SECTION;
        Pressure.transform.position = _pressureStartPosition;
        Pressure.transform.rotation = _pressureStartRotation;
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
        else
        {
            _startPlatform = GameObject.Find("StartPlatform");
        }
        
        CinemachineSmoothPath.Waypoint newWaypoint = new CinemachineSmoothPath.Waypoint();
        newWaypoint.position = newSection.CameraPathPoint.transform.position;
        _cameraWaypoints.Add(newWaypoint);
        OnCameraWaypointsChanged(_cameraWaypoints);

        _pressureWayoints.Add(new Vector3(newSection.CameraPathPoint.transform.position.x, newSection.CameraPathPoint.transform.position.y, 0.0f));
        _pressureWayoints.Add(newSection.PressurePathPoint.transform.position);

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
        if (_pressureTween.timeScale < PressureMaxSpeed)
            DOTween.To(() => _pressureTween.timeScale, x => _pressureTween.timeScale = x, _pressureTween.timeScale + 1.1f, 5.0f);

        if (waypointIndex > 0)
        {
            float currentTimeScale = _pressureTween.timeScale;
            _pressureWayoints.RemoveAt(0);
            _pressureTween.Kill();
            _pressureTween = Pressure.transform.DOPath(_pressureWayoints.ToArray(), 5.0f, PathType.CatmullRom, PathMode.Full3D, 10, Color.green).SetSpeedBased().OnWaypointChange(PressureWaypointChange).SetLookAt(0.001f, Vector3.left);
            _pressureTween.timeScale = currentTimeScale;
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