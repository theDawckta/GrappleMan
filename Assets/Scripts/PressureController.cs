using UnityEngine;
using System.Collections;

public class PressureController : MonoBehaviour {

	public float PressureSpeed = 1.0f;
	
	// Update is called once per frame
	void Update () {
		
		transform.Translate(new Vector3(transform.position.x + (PressureSpeed / Time.deltaTime), transform.position.y, transform.position.z), Space.World);
	}
}
