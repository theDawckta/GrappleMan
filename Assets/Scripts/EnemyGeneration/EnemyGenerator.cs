using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyGenerator : MonoBehaviour {

    public EnemyTurret _EnemyTurret;
	private List<EnemyTurret> EnemyTurrets = new List<EnemyTurret>();
    private List<Vector3> bottomCorners = new List<Vector3>();
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
            int xLocation = pseudoRandom.Next(20, (int)totalLevelLength);

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
		possibleXLocations.Sort((a, b) => a.CompareTo(b));

		// could be optimized
		for (int i = 0; i < possibleXLocations.Count; i++)
		{
			for (int j = 0; j < bottomCorners.Count; j++)
			{
				if(possibleXLocations[i] < bottomCorners[j].x)
				{
					Debug.Log("We got one");
					EnemyTurret tempEnemyTurret = (EnemyTurret)Instantiate(_EnemyTurret, new Vector3(possibleXLocations[i], bottomCorners[j].y, _EnemyTurret.transform.position.z), Quaternion.identity);
					tempEnemyTurret.transform.parent = transform;
					EnemyTurrets.Add(tempEnemyTurret);
					break;
				}
			}
		}
    }

    private void ProcessLevelVertices(List<Vector3> levelVertices)
    {
        for (int i = 10; i < levelVertices.Count; i = i + 24)
        {
            bottomCorners.Add(levelVertices[i + 1]);
            bottomCorners.Add(levelVertices[i]);
			Debug.DrawLine(levelVertices[i], levelVertices[i + 1], Color.yellow, 20.0f);
        }
    }
}
