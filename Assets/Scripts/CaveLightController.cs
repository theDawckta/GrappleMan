using UnityEngine;
using System.Collections;

public class CaveLightController : MonoBehaviour
{

    public float PressureSpeed = 5.0f;
    private bool active = false;
    private Vector3 startingPosition;

    void Awake()
    {
        startingPosition = transform.position;
    }

    public void Init()
    {
        transform.position = startingPosition;
    }

    void Update()
    {
        if (active)
        {
            transform.Translate(Vector3.right * PressureSpeed * Time.deltaTime);
        }
    }

    public void LightGo()
    {
        active = true;
    }
    public void LightStop()
    {
        active = false;
    }
}