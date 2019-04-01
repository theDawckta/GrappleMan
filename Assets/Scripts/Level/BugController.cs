using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BugController : MonoBehaviour
{
    public delegate void PlayerHit();
    public event PlayerHit OnPlayerCaught;

    public GameObject BugSprite;
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
    private float _frPreviousDistance;
    private float _flPreviousDistance;

    void Awake ()
    {
       _maxDistance = Vector3.Distance(FRRaycastOrigin.position, Hand.position);
        _flPreviousDistance = _maxDistance;
        _frPreviousDistance = _maxDistance;
    }
	
	void Update ()
    {
        if (Vector3.Distance(BLRaycastOrigin.position, BLTarget.position) >= _maxDistance && !DOTween.IsTweening(BLTarget, true))
            PlaceBackLeg(-60, BLTarget, BLRaycastOrigin);

        if (Vector3.Distance(BRRaycastOrigin.position, BRTarget.position) >= _maxDistance && !DOTween.IsTweening(BRTarget, true))
            PlaceBackLeg(60, BRTarget, BRRaycastOrigin);

        if (Vector3.Distance(FLRaycastOrigin.position, FLTarget.position) <= _flPreviousDistance && !DOTween.IsTweening(FLTarget, true))
            _flPreviousDistance = Vector3.Distance(FLRaycastOrigin.position, FLTarget.position);
        else if (!DOTween.IsTweening(FLTarget, true))
            PlaceLeg(1, FLTarget, FLRaycastOrigin);
        else
            _flPreviousDistance = _maxDistance;

        if (Vector3.Distance(FRRaycastOrigin.position, FRTarget.position) <= _frPreviousDistance && !DOTween.IsTweening(FRTarget, true))
            _frPreviousDistance = Vector3.Distance(FRRaycastOrigin.position, FRTarget.position);
        else if (!DOTween.IsTweening(FRTarget, true))
            PlaceLeg(-1, FRTarget, FRRaycastOrigin);
        else
            _frPreviousDistance = _maxDistance;
    }
    
    public void Init()
    {
        PlaceBackLeg(-60, BLTarget, BLRaycastOrigin);
        PlaceBackLeg(60, BRTarget, BRRaycastOrigin);
        PlaceLeg(1, FLTarget, FLRaycastOrigin);
        PlaceLeg(-1, FRTarget, FRRaycastOrigin);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == LayerMask.NameToLayer("Player"))
            OnPlayerCaught();
    }

    public void UpdateBugSprite(Vector3 newPosition, Quaternion newRotation)
    {
        BugSprite.transform.position = newPosition;
        BugSprite.transform.rotation = newRotation;
    }

    public void Grab(Vector3 position)
    {

    }

    void PlaceLeg(int rotationDirection, Transform legTarget, Transform RaycastOrigin)
    {
        for (int i = 0; i != 90 * rotationDirection; i = i - 5 * rotationDirection)
        {
            _direction = Quaternion.AngleAxis(i, Vector3.back) * BugSprite.transform.right;
            //Debug.DrawRay(RaycastOrigin.position, _direction * _maxDistance, Color.blue, 10.0f);

            if (Physics.Raycast(RaycastOrigin.position, _direction, out _hit, _maxDistance, 1 << LayerMask.NameToLayer("Wall")))
            {
                break;
            }
        }

        Vector3 halfwayPoint = ((_hit.point - legTarget.position) * 0.5f) + legTarget.position;
        Vector3 halfwayPointDirection = _hit.point - legTarget.position;

        Vector3 direction = Quaternion.AngleAxis(90 * rotationDirection, Vector3.back) * halfwayPointDirection;
        halfwayPoint = halfwayPoint + (direction * 0.1f);
        Vector3[] path = new Vector3[] { legTarget.position, halfwayPoint, _hit.point };

        legTarget.DOPath(path, 0.15f, PathType.CatmullRom, PathMode.Full3D, 5, Color.green).SetEase(Ease.Linear);
        legTarget.DORotateQuaternion(Quaternion.LookRotation(_hit.normal), 0.15f);
    }

    void PlaceBackLeg(int rotationDirection, Transform legTarget, Transform RaycastOrigin)
    {
        int rotationSign = (rotationDirection < 0) ? -1 : 1;
        _direction = Quaternion.AngleAxis(rotationDirection, Vector3.back) * BugSprite.transform.right;
        Debug.DrawRay(RaycastOrigin.position, _direction * _maxDistance, Color.blue, 10.0f);

        if (Physics.Raycast(RaycastOrigin.position, _direction, out _hit, _maxDistance, 1 << LayerMask.NameToLayer("Wall")))
        {
            Vector3 halfwayPoint = ((_hit.point - legTarget.position) * 0.5f) + legTarget.position;
            Vector3 halfwayPointDirection = _hit.point - legTarget.position;

            Vector3 direction = Quaternion.AngleAxis(90 * -rotationSign, Vector3.back) * halfwayPointDirection;
            halfwayPoint = halfwayPoint + (direction * 0.1f);
            Vector3[] path = new Vector3[] { legTarget.position, halfwayPoint, _hit.point };

            legTarget.DOPath(path, 0.15f, PathType.CatmullRom, PathMode.Full3D, 5, Color.green).SetEase(Ease.Linear);
            legTarget.DORotateQuaternion(Quaternion.LookRotation(_hit.normal), 0.15f);
        }
    }
}
