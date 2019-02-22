using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enums;

public class LevelController : MonoBehaviour
{
    public string Seed;
    public bool UseRandomSeed;
    public List<LevelSectionController> LevelSections = new List<LevelSectionController>();

    private GameObject Player;
    private List<LevelSectionController> _oldLevelSections = new List<LevelSectionController>();
    private LevelSectionController _currentSection;
    private System.Random _pseudoRandom;
    
    void Start ()
    {
        if (UseRandomSeed)
        {
            Seed = Time.time.ToString();
        }

        _pseudoRandom = new System.Random(Seed.GetHashCode());
        _currentSection = LevelSections.Find(x => x.Section == SectionType.WE);
        Instantiate(_currentSection, transform);
    }
	
	void Update ()
    {
		while(Player != null && Vector3.Distance(_currentSection.transform.position, Player.transform.position) < 320.0f)
            LoadSection();
	}

    public void Init(GameObject player)
    {
        Player = player;
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
    }
}