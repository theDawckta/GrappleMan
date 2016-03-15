using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyGenerator : MonoBehaviour {

    private List<Vector3> squareCorners;
    private GameObject enemySection = new GameObject();

	// Use this for initialization
	void Start () {
	
	}

    public GameObject MakeEnemies(List<Vector3> levelVertices, System.Random pseudoRandom) 
    {
        ProcessLevelVertices(levelVertices);
        //GenerateTurrets();
        return enemySection;
    }

    private void ProcessLevelVertices(List<Vector3> levelVertices)
    {

    }
}
