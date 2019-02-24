using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Enums;
using Cinemachine;

public class LevelController : MonoBehaviour
{
    public delegate void LevelSetionAdded(List<CinemachineSmoothPath.Waypoint> waypoints);
    public event LevelSetionAdded OnLevelSectionAdded;

    public bool UseRandomSeed;
    public List<LevelSectionController> LevelSections = new List<LevelSectionController>();
    public GameObject FrontBarrier;

    private string _seed = "123456";
    private GameObject _player;
    private List<LevelSectionController> _oldLevelSections = new List<LevelSectionController>();
    private LevelSectionController _currentSection;
    private System.Random _pseudoRandom;
    private List<CinemachineSmoothPath.Waypoint> _cameraWayoints = new List<CinemachineSmoothPath.Waypoint>();
    
    void Start ()
    {
        _currentSection = Instantiate(LevelSections.Find(x => x.Section.Equals(SectionType.START_SECTION)), transform);
        CinemachineSmoothPath.Waypoint newWaypoint = new CinemachineSmoothPath.Waypoint();
        newWaypoint.position = _currentSection.CameraPathPoint.transform.position;
        _cameraWayoints.Add(newWaypoint);
        OnLevelSectionAdded(_cameraWayoints);
    }
	
	void Update ()
    {
		while(_player != null && Vector3.Distance(_currentSection.transform.position, _player.transform.position) < 320.0f)
            LoadSection();
	}

    public void Init(GameObject player)
    {
        _player = player;
        _pseudoRandom = new System.Random(_seed.GetHashCode());
    }

    public void Reset()
    {
        for (int i = 0; i < _oldLevelSections.Count; i++)
            Destroy(_oldLevelSections[i].gameObject);

        Destroy(_currentSection.gameObject);
        _cameraWayoints.Clear();
        OnLevelSectionAdded(_cameraWayoints);

        _pseudoRandom = new System.Random(_seed.GetHashCode());
        _currentSection = Instantiate(LevelSections.Find(x => x.Section.Equals(SectionType.START_SECTION)), transform);

        CinemachineSmoothPath.Waypoint newWaypoint = new CinemachineSmoothPath.Waypoint();
        newWaypoint.position = _currentSection.CameraPathPoint.transform.position;
        _cameraWayoints.Add(newWaypoint);
        OnLevelSectionAdded(_cameraWayoints);

        _pseudoRandom = new System.Random(_seed.GetHashCode());
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
    }
}