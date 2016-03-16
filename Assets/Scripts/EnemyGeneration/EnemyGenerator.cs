using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyGenerator : MonoBehaviour {

    public GameObject Turret;
    private List<Vector3> squareCorners = new List<Vector3>();
    private GameObject enemySection;

	// Use this for initialization
	void Start () {
	    
	}

    public GameObject MakeEnemies(List<Vector3> levelVertices, System.Random pseudoRandom, int minEnemyCount, int maxEnemyCount, float totalLevelLength) 
    {
        List<int> possibleXLocations = new List<int>();
        ProcessLevelVertices(levelVertices);
        int numOfEnemies = pseudoRandom.Next(minEnemyCount, maxEnemyCount);
        for (int i = 0; i < numOfEnemies; i++)
        {
            int xLocation = pseudoRandom.Next(0, (int)totalLevelLength);

            if (!possibleXLocations.Contains(xLocation))
                possibleXLocations.Add(xLocation);
            else
                i--;
        }
        GenerateEnemies(possibleXLocations);
            enemySection = new GameObject();
        return enemySection;
    }

    private void GenerateEnemies(List<int> possibleXLocations)
    {
        Debug.Log("Made it to GenerateEnemies");
    }

    private void ProcessLevelVertices(List<Vector3> levelVertices)
    {
        for (int i = 8; i < levelVertices.Count; i = i + 24)
        {
            squareCorners.Add(levelVertices[i]);
            squareCorners.Add(levelVertices[i + 1]);
            Debug.DrawLine(levelVertices[i], levelVertices[i + 1], Color.yellow, 20.0f);
            squareCorners.Add(levelVertices[i + 2]);
            Debug.DrawLine(levelVertices[i + 1], levelVertices[i + 2], Color.red, 20.0f);
            squareCorners.Add(levelVertices[i + 3]);
            Debug.DrawLine(levelVertices[i + 2], levelVertices[i + 3], Color.blue, 20.0f);
        }
    }
}
