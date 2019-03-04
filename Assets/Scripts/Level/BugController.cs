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
    private Vector3 _direction;


    void Start ()
    {

    }
	
	void Update ()
    {
        Debug.Log(Vector3.Distance(Bug.transform.position, BRTarget.transform.position));
        if (Vector3.Distance(Bug.transform.position, BRTarget.transform.position) > 44.0f)
        {

        }
	}

    void PlaceBRLeg()
    {
        _direction = Quaternion.AngleAxis(90, Vector3.back) * Vector3.right;
        Debug.DrawRay(Bug.transform.position, _direction * 1000.0f, Color.red, 60.0f);

        if (Physics.Raycast(Bug.transform.position, _direction, out _hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("Wall")))
        {
            BRTarget.transform.position = _hit.point;
            BRTarget.transform.rotation = Quaternion.LookRotation(_hit.normal);
        }
    }

    void PlaceBLLeg()
    {
        _direction = Quaternion.AngleAxis(-90, Vector3.back) * Vector3.right;
        Debug.DrawRay(Bug.transform.position, _direction * 1000.0f, Color.red, 60.0f);

        if (Physics.Raycast(Bug.transform.position, _direction, out _hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("Wall")))
        {
            BLTarget.transform.position = _hit.point;
            BLTarget.transform.rotation = Quaternion.LookRotation(_hit.normal);
        }
    }

    void PlaceFRLeg()
    {
        _direction = Quaternion.AngleAxis(45, Vector3.back) * Vector3.right;
        Debug.DrawRay(Bug.transform.position, _direction * 1000.0f, Color.red, 60.0f);

        if (Physics.Raycast(Bug.transform.position, _direction, out _hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("Wall")))
        {
            FRTarget.transform.position = _hit.point;
            FRTarget.transform.rotation = Quaternion.LookRotation(_hit.normal);
        }
    }

    void PlaceFLLeg()
    {
        _direction = Quaternion.AngleAxis(45, Vector3.back) * Vector3.right;
        Debug.DrawRay(Bug.transform.position, _direction * 1000.0f, Color.red, 60.0f);

        if (Physics.Raycast(Bug.transform.position, _direction, out _hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("Wall")))
        {
            FLTarget.transform.position = _hit.point;
            FLTarget.transform.rotation = Quaternion.LookRotation(_hit.normal);
        }
    }

    public void Init()
    {
        Vector3 direction;

        direction = Quaternion.AngleAxis(90, Vector3.back) * Vector3.right;
        Debug.DrawRay(Bug.transform.position, direction * 1000.0f, Color.red, 60.0f);

        if (Physics.Raycast(Bug.transform.position, direction, out _hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("Wall")))
        {
            BRTarget.transform.position = _hit.point;
            BRTarget.transform.rotation = Quaternion.LookRotation(_hit.normal);
        }

        direction = Quaternion.AngleAxis(-90, Vector3.back) * Vector3.right;
        Debug.DrawRay(Bug.transform.position, direction * 1000.0f, Color.red, 60.0f);

        if (Physics.Raycast(Bug.transform.position, direction, out _hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("Wall")))
        {
            BLTarget.transform.position = _hit.point;
            BLTarget.transform.rotation = Quaternion.LookRotation(_hit.normal);
        }

        direction = Quaternion.AngleAxis(45, Vector3.back) * Vector3.right;
        Debug.DrawRay(Bug.transform.position, direction * 1000.0f, Color.red, 60.0f);

        if (Physics.Raycast(Bug.transform.position, direction, out _hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("Wall")))
        {
            FRTarget.transform.position = _hit.point;
            FRTarget.transform.rotation = Quaternion.LookRotation(_hit.normal);
        }

        direction = Quaternion.AngleAxis(-45, Vector3.back) * Vector3.right;
        Debug.DrawRay(Bug.transform.position, direction * 1000.0f, Color.red, 60.0f);

        if (Physics.Raycast(Bug.transform.position, direction, out _hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("Wall")))
        {
            FLTarget.transform.position = _hit.point;
            FLTarget.transform.rotation = Quaternion.LookRotation(_hit.normal);
        }
    }
}
