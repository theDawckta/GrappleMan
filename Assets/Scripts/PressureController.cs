using UnityEngine;
using System.Collections;

public class PressureController : MonoBehaviour {

	public float PressureSpeed = 10.0f;
    private bool active = false;
	
	// Update is called once per frame
	void Update () {
        if (active)
        {
            transform.Translate(Vector3.right * PressureSpeed * Time.deltaTime);
        }
	}

    public void LavaFlow()
    {
        active = true;
    }
    public void LavaFlowStop()
    {
        active = false;
    }
}
