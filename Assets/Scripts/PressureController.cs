using UnityEngine;
using System.Collections;

public class PressureController : MonoBehaviour {

	public float PressureSpeed = 15.0f;
    private bool active = false;
    private Vector3 startingPosition;
	
    void Start()
    {
        startingPosition = transform.position;
    }

    public void Init()
    {
        transform.position = startingPosition;
    }

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
