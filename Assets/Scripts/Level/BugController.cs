using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BugController : MonoBehaviour
{
    public GameObject Bug;
    public Transform BLTarget;
    public Transform BRTarget;
    public Transform FLTarget;
    public Transform FRTarget;
    public Transform BLRaycastOrigin;
    public Transform BRRaycastOrigin;
    public Transform FLRaycastOrigin;
    public Transform FRRaycastOrigin;
    public Transform Hand;

    private RaycastHit _hit;
    private Vector3 _direction;
    private float _maxDistance;

    void Start ()
    {
       _maxDistance = Vector3.Distance(FRRaycastOrigin.position, Hand.position);
    }
	
	void Update ()
    {
        Debug.Log(Vector3.Distance(BRRaycastOrigin.position, BRTarget.position));
        if (Vector3.Distance(BRRaycastOrigin.position, BRTarget.position) > 35.0f && !DOTween.IsTweening(BRTarget, true))
            PlaceLeg(-1, BRTarget, BRRaycastOrigin);
        if (Vector3.Distance(BLRaycastOrigin.position, BLTarget.position) > 35.0f && !DOTween.IsTweening(BLTarget, true))
            PlaceLeg(1, BLTarget, BLRaycastOrigin);
        if (Vector3.Distance(FRRaycastOrigin.position, FRTarget.position) > 35.0f && !DOTween.IsTweening(FRTarget, true))
            PlaceLeg(-1, FRTarget, FRRaycastOrigin);
        if (Vector3.Distance(FLRaycastOrigin.position, FLTarget.position) > 35.0f && !DOTween.IsTweening(FLTarget, true))
            PlaceLeg(1, FLTarget, FLRaycastOrigin);
    }
    
    public void Init()
    {
        PlaceLeg(-1, BRTarget, BRRaycastOrigin);
        PlaceLeg(1, BLTarget, BLRaycastOrigin);
        PlaceLeg(-1, FRTarget, FRRaycastOrigin);
        PlaceLeg(1, FLTarget, FLRaycastOrigin);
    }

    void PlaceLeg(int rotationDirection, Transform legTarget, Transform RaycastOrigin)
    {
        for (int i = 0; i != 90 * rotationDirection; i = i - 5 * rotationDirection)
        {
            _direction = Quaternion.AngleAxis(i, Vector3.back) * Bug.transform.right;
            Debug.DrawRay(RaycastOrigin.position, _direction * _maxDistance, Color.blue, 10.0f);

            if (Physics.Raycast(RaycastOrigin.position, _direction, out _hit, _maxDistance, 1 << LayerMask.NameToLayer("Wall")))
            {
                break;
            }
        }

        Vector3 halfwayPoint = ((_hit.point - legTarget.position) * 0.5f) + legTarget.position;
        Vector3 halfwayPointDirection = _hit.point - legTarget.position;

        Vector3 direction = Quaternion.AngleAxis(90 * rotationDirection, Vector3.back) * halfwayPointDirection;
        halfwayPoint = halfwayPoint + (direction * 0.25f);
        Vector3[] path = new Vector3[] { legTarget.position, halfwayPoint, _hit.point };

        legTarget.DOPath(path, 0.25f, PathType.CatmullRom, PathMode.Full3D, 5, Color.green).SetEase(Ease.Linear);
        legTarget.DORotateQuaternion(Quaternion.LookRotation(_hit.normal), 0.5f);


        //    _direction = Quaternion.AngleAxis(angle, Vector3.back) * Bug.transform.right;
        //Debug.DrawRay(RaycastOrigin.position, _direction * 1000.0f, Color.blue, 10.0f);


        //if (Physics.Raycast(RaycastOrigin.position, _direction, out _hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("Wall")))
        //{
        //    Vector3 halfwayPoint = ((_hit.point - legTarget.position) * 0.5f) + legTarget.position;
        //    Vector3 halfwayPointDirection = _hit.point - legTarget.position;

        //    Vector3 direction = Quaternion.AngleAxis(90 * angleSign, Vector3.back) * halfwayPointDirection;
        //    halfwayPoint = halfwayPoint + (direction * 0.25f);
        //    Vector3[] path = new Vector3[] { legTarget.position, halfwayPoint, _hit.point };

        //    legTarget.DOPath(path, 0.25f, PathType.CatmullRom, PathMode.Full3D, 5, Color.green).SetEase(Ease.Linear);
        //    legTarget.DORotateQuaternion(Quaternion.LookRotation(_hit.normal), 0.5f);
        //}
    }
}
