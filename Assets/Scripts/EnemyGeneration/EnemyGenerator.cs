using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyGenerator : MonoBehaviour {

    public GameObject _EnemyAI;
    private List<Vector3> bottomCorners = new List<Vector3>();
    private GameObject enemySection;

	// Use this for initialization
	void Start () {
	    
	}

    public GameObject MakeEnemySection(List<Vector3> levelVertices, System.Random pseudoRandom, int minEnemyCount, int maxEnemyCount, float totalLevelLength) 
    {
        List<int> possibleXLocations = new List<int>();

        bottomCorners = ProcessLevelVertices(levelVertices);
        int numOfEnemies = pseudoRandom.Next(minEnemyCount, maxEnemyCount);
        for (int i = 0; i < numOfEnemies; i++)
        {
            int xLocation = pseudoRandom.Next(20, (int)totalLevelLength);

            if (!possibleXLocations.Contains(xLocation))
                possibleXLocations.Add(xLocation);
            else
                i--;
        }
        return GenerateEnemies(possibleXLocations);
    }

    private GameObject GenerateEnemies(List<int> possibleXLocations)
    {
        GameObject turretSection = new GameObject();
		possibleXLocations.Sort((a, b) => a.CompareTo(b));

		// could be optimized
		for (int i = 0; i < possibleXLocations.Count; i++)
		{
			for (int j = 0; j < bottomCorners.Count; j++)
			{
				if(possibleXLocations[i] < bottomCorners[j].x)
				{
					GameObject tempEnemyAI = (GameObject)Instantiate(_EnemyAI, new Vector3(possibleXLocations[i], bottomCorners[j].y, _EnemyAI.transform.position.z), Quaternion.identity);
					tempEnemyAI.transform.parent = turretSection.transform;
					break;
				}
			}
		}
        turretSection.name = "TurretSection";
        return turretSection;
    }

    private List<Vector3> ProcessLevelVertices(List<Vector3> levelVertices)
    {
        List<Vector3> tempBottomCorners = new List<Vector3>();

        for (int i = 10; i < levelVertices.Count; i = i + 24)
        {
            tempBottomCorners.Add(levelVertices[i + 1]);
            tempBottomCorners.Add(levelVertices[i]);
			Debug.DrawLine(levelVertices[i], levelVertices[i + 1], Color.yellow, 20.0f);
        }

        return tempBottomCorners;
    }
}
