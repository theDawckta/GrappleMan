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
    public Transform FFLTarget;
    public Transform FFRTarget;
    public Transform BLRaycastOrigin;
    public Transform BRRaycastOrigin;
    public Transform FLRaycastOrigin;
    public Transform FRRaycastOrigin;
    public Transform Hand;
    public GameObject Torso;

    private RaycastHit _hit;
    private Vector3 _direction;
    private float _maxDistance;
    private float _frPreviousDistance;
    private float _flPreviousDistance;
    private Vector3 _ffrTargetOGPosition;
    private Vector3 _fflTargetOGPosition;
    private Tweener _ffrTweener;
    private Tweener _fflTweener;
    private bool _ffrAvailable = true;
    private bool _fflAvailable = true;
    private List<GhostController> _punchQueue = new List<GhostController>();

    void Awake ()
    {
       _maxDistance = Vector3.Distance(FRRaycastOrigin.position, Hand.position);
        _flPreviousDistance = _maxDistance;
        _frPreviousDistance = _maxDistance;
        _ffrTargetOGPosition = FFRTarget.transform.localPosition;
        _fflTargetOGPosition = FFLTarget.transform.localPosition;
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

        if(_punchQueue.Count > 0 && (_ffrAvailable || _fflAvailable))
        {
            Punch(_punchQueue[0].transform.position, _punchQueue[0]);
            _punchQueue.RemoveAt(0);
        }
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
        {
            Grab(other.gameObject);
            OnPlayerCaught();
        }
            
        if (other.gameObject.layer == LayerMask.NameToLayer("Ghost"))
        {
            GhostController caughtGhost = other.GetComponentInParent<GhostController>();
            _punchQueue.Add(caughtGhost);
        }
    }

    public void UpdateBugSprite(Vector3 newPosition, Quaternion newRotation)
    {
        BugSprite.transform.position = newPosition;
        BugSprite.transform.rotation = newRotation;
    }

    private void Grab(GameObject player)
    {
        if(Vector3.Distance(FFLTarget.transform.position, player.transform.position) < Vector3.Distance(FFRTarget.transform.position, player.transform.position))
            FFLTarget.DOMove(player.transform.position, 0.2f);
        else
            FFRTarget.DOMove(player.transform.position, 0.2f);
    }

    private void Punch(Vector3 position, GhostController caughtGhost)
    {
        if (_fflAvailable && _ffrAvailable)
        {
            if (Vector3.Distance(FFLTarget.transform.position, position) < Vector3.Distance(FFRTarget.transform.position, position))
            {
                StartFLPunch(position, caughtGhost);
            }
            else
            {
                StartFRPunch(position, caughtGhost);
            }
        }
        else if(_fflAvailable)
        {
            StartFLPunch(position, caughtGhost);
        }
        else if(_ffrAvailable)
        {
            StartFRPunch(position, caughtGhost);
        }
    }

    private void StartFRPunch(Vector3 position, GhostController caughtGhost)
    {
        _ffrAvailable = false;
        FFRTarget.parent = null;
        position = new Vector3(position.x, position.y, -3.0f);

        if (_ffrTweener != null && _ffrTweener.IsPlaying())
            _ffrTweener.Kill();

        _ffrTweener = FFRTarget.DOMove(position, 0.2f).OnComplete(() => {
            StartCoroutine(BringFFRBack(caughtGhost));
        });
    }

    private void StartFLPunch(Vector3 position, GhostController caughtGhost)
    {
        _fflAvailable = false;
        FFLTarget.parent = null;
        position = new Vector3(position.x, position.y, -3.0f);

        if (_fflTweener != null && _fflTweener.IsPlaying())
            _fflTweener.Kill();

        _fflTweener = FFLTarget.DOMove(position, 0.2f).OnComplete(() => {
            StartCoroutine(BringFFLBack(caughtGhost));
        });
    }

    IEnumerator BringFFRBack(GhostController caughtGhost)
    {
        yield return new WaitForSeconds(0.2f);

        FFRTarget.SetParent(Torso.transform, true);
        caughtGhost.Caught();
        _ffrAvailable = true;
        if(_punchQueue.Count == 0)
            FFRTarget.DOLocalMove(_ffrTargetOGPosition, 0.2f);

        yield return null;
    }

    IEnumerator BringFFLBack(GhostController caughtGhost)
    {
        yield return new WaitForSeconds(0.2f);

        FFLTarget.SetParent(Torso.transform, true);
        caughtGhost.Caught();
        _fflAvailable = true;
        if (_punchQueue.Count == 0)
            FFLTarget.DOLocalMove(_fflTargetOGPosition, 0.2f);

        yield return null;
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
