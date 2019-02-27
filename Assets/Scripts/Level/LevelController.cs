using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Enums;
using Cinemachine;
using DG.Tweening;

public class LevelController : MonoBehaviour
{
    public delegate void LevelSetionAdded(List<CinemachineSmoothPath.Waypoint> waypoints);
    public event LevelSetionAdded OnLevelSectionAdded;

    public bool UseRandomSeed;
    public List<LevelSectionController> LevelSections = new List<LevelSectionController>();
    public GameObject FrontBarrier;
    public GameObject Death;

    private string _seed = "123456";
    private GameObject _player;
    private List<LevelSectionController> _oldLevelSections = new List<LevelSectionController>();
    private LevelSectionController _currentSection;
    private System.Random _pseudoRandom;
    private List<CinemachineSmoothPath.Waypoint> _cameraWayoints = new List<CinemachineSmoothPath.Waypoint>();
    private List<Vector3> _pressureWayoints = new List<Vector3>();

    void Start ()
    {
        Reset();
    }
	
	void Update ()
    {
        if(_player != null && _currentSection != null)
        {
            while (Vector3.Distance(_currentSection.transform.position, _player.transform.position) < 320.0f)
                LoadSection();
        }
	}

    public void Init(GameObject player)
    {
        _player = player;
        _pseudoRandom = new System.Random(_seed.GetHashCode());
    }

    public void StartDeathMarch()
    {
        _pressureWayoints.Insert(0, Death.transform.position);
        Death.transform.DOPath(_pressureWayoints.ToArray(), 5f, PathType.Linear, PathMode.Full3D, 10, Color.green).SetSpeedBased().OnWaypointChange(PressureStepComplete);
    }

    public void UpdateDeathMarchWaypoints(List<Vector3> newWaypoints)
    {
        _pressureWayoints = newWaypoints;
    }

    public void Reset()
    {
        for (int i = 0; i < _oldLevelSections.Count; i++)
            Destroy(_oldLevelSections[i].gameObject);

        if(_currentSection != null)
            Destroy(_currentSection.gameObject);

        _cameraWayoints.Clear();
        OnLevelSectionAdded(_cameraWayoints);

        _pseudoRandom = new System.Random(_seed.GetHashCode());
        _currentSection = Instantiate(LevelSections.Find(x => x.Section.Equals(SectionType.START_SECTION)), transform);

        CinemachineSmoothPath.Waypoint newWaypoint = new CinemachineSmoothPath.Waypoint();
        newWaypoint.position = _currentSection.CameraPathPoint.transform.position;
        _cameraWayoints.Add(newWaypoint);
        OnLevelSectionAdded(_cameraWayoints);
        _pressureWayoints.Add(new Vector3(newWaypoint.position.x, newWaypoint.position.y, 0.0f));
        _pseudoRandom = new System.Random(_seed.GetHashCode());
    }

    private void LoadSection()
    {
        List<SectionType> possibleNextSections = _currentSection.PossibleNextSections;
        SectionType nextSection = possibleNextSections[_pseudoRandom.Next(0, possibleNextSections.Count - 1)];

        _oldLevelSections.Add(_currentSection);

        Vector3 newSectionPosition = new Vector3(_currentSection.transform.position.x + _currentSection.OffsetX,
                                                 _currentSection.transform.position.y + _currentSection.OffsetY,
                                                 0.0f);

        _currentSection = Instantiate(LevelSections.Find(x => x.Section == nextSection), transform);
        _currentSection.transform.position = newSectionPosition;
        CinemachineSmoothPath.Waypoint newWaypoint = new CinemachineSmoothPath.Waypoint();
        newWaypoint.position = _currentSection.CameraPathPoint.transform.position;
        _cameraWayoints.Add(newWaypoint);

        OnLevelSectionAdded(_cameraWayoints);
        _pressureWayoints.Add(new Vector3 (newWaypoint.position.x, newWaypoint.position.y, 0.0f));
    }

    public void PressureStepComplete(int waypointIndex)
    {
        if(waypointIndex > 2)
            Destroy(_oldLevelSections[waypointIndex - 3].gameObject);
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