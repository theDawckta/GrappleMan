using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BugController : MonoBehaviour
{
    public GameObject Bug;
    public GameObject BLTarget;
    public GameObject BRTarget;
    public GameObject FLTarget;
    public GameObject FRTarget;

    private RaycastHit _hit;

    void Start ()
    {
        Vector3 direction;
        Vector3 longestHit = transform.position;

        for (int i = 1; i < 360.0f; i = i + 30)
        {
            direction = Quaternion.AngleAxis(i, Vector3.back) * Vector3.right;
            Debug.DrawRay(Bug.transform.position, direction * 1000.0f, Color.red, 60.0f);
            if (Physics.Raycast(Bug.transform.position, direction, out _hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("Wall")))
            {
                longestHit = ((_hit.point - Bug.transform.position).magnitude > (longestHit - Bug.transform.position).magnitude) ? _hit.point : longestHit;
            }
        }
    }
	
	void Update ()
    {
		
	}
}
