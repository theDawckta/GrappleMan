using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BugController : MonoBehaviour
{

    public GameObject Bug;
    public GameObject MouthLocation;
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
    public Transform FFLRaycastOrigin;
    public Transform FFRRaycastOrigin;
    public Transform FRFoot;
    
    private RaycastHit _hit;
    private Vector3 _direction;
    private float _maxDistance;
    private float _frPreviousDistance;
    private float _flPreviousDistance;
    private Vector3 _ffrTargetOGPosition;
    private Vector3 _fflTargetOGPosition;
    private Vector3 _ffrTargetOGRotation;
    private Vector3 _fflTargetOGRotation;
    private Tweener _ffrTweener;
    private Tweener _fflTweener;
    private bool _ffrAvailable = true;
    private bool _fflAvailable = true;
    private Animator _bugAnimator;
    private List<GhostController> _punchQueue = new List<GhostController>();
    private List<PlayerController> _eatQueue = new List<PlayerController>();

    void Awake ()
    {
        _maxDistance = Vector3.Distance(FRRaycastOrigin.position, FRFoot.position);
        _flPreviousDistance = _maxDistance;
        _frPreviousDistance = _maxDistance;
        _ffrTargetOGPosition = FFRTarget.transform.localPosition;
        _fflTargetOGPosition = FFLTarget.transform.localPosition;
        _ffrTargetOGRotation = FFRTarget.transform.localEulerAngles;
        _fflTargetOGRotation = FFLTarget.transform.localEulerAngles;
        _bugAnimator = gameObject.GetComponent<Animator>();
    }
	
	void Update ()
    {
        SetLegs();

        if (_eatQueue.Count > 0 && (_ffrAvailable && _fflAvailable))
        {
            Grab(_eatQueue[0]);
            _eatQueue.RemoveAt(0);
        }
        else if (_eatQueue.Count == 0 && _punchQueue.Count > 0 && (_ffrAvailable || _fflAvailable))
        {
            Punch(_punchQueue[0]);
            _punchQueue.RemoveAt(0);
        }
    }

    public void StartChomping()
    {
        _bugAnimator.SetTrigger("Chomping");
    }

    public void StopChomping()
    {
        _bugAnimator.SetTrigger("NotChomping");
    }

    public void Init()
    {
        PlaceBackLeg(-60, BLTarget, BLRaycastOrigin);
        PlaceBackLeg(50, BRTarget, BRRaycastOrigin);
        PlaceLeg(-1, FLTarget, FLRaycastOrigin);
        PlaceLeg(1, FRTarget, FRRaycastOrigin);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            PlayerController caughtPlayer = other.GetComponentInParent<PlayerController>();
            caughtPlayer.Caught();
            _eatQueue.Add(caughtPlayer);
        }

        if (other.gameObject.layer == LayerMask.NameToLayer("Ghost"))
        {
            GhostController caughtGhost = other.GetComponentInParent<GhostController>();
            _punchQueue.Add(caughtGhost);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Egg"))
        {
            collision.gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
        }
    }

        public void UpdateBugSprite(Vector3 newPosition, Quaternion newRotation)
    {
        Bug.transform.position = newPosition;
        Bug.transform.rotation = newRotation;
    }

    private void Grab(PlayerController player)
    {
        _fflAvailable = false;
        _ffrAvailable = false;

        if (player.GrabSpot1.transform.position.y > player.GrabSpot2.transform.position.y)
        {
            FFLTarget.DOMove(player.GrabSpot1.transform.position, 0.2f).OnStart(() =>
            {
                if(player.GrabSpot1.transform.position.y < FFLTarget.position.y)
                {
                    Vector3 newAngle = new Vector3(player.GrabSpot1.transform.eulerAngles.x, player.GrabSpot1.transform.eulerAngles.y, player.GrabSpot1.transform.eulerAngles.z);
                    FFLTarget.DORotate(newAngle, 0.2f);
                }
                else
                {
                    Vector3 newAngle = new Vector3(player.GrabSpot1.transform.eulerAngles.x, player.GrabSpot1.transform.eulerAngles.y, player.GrabSpot1.transform.eulerAngles.z);
                    FFLTarget.DORotate(newAngle, 0.2f);
                }
            }).OnComplete(() => {
                FFLTarget.SetParent(player.GrabSpot1.transform, true);
                Retrieve(player);
            });
            FFRTarget.DOMove(player.GrabSpot2.transform.position, 0.2f).OnStart(() =>
            {
                if (player.GrabSpot2.transform.position.y < FFRTarget.position.y)
                {
                    Vector3 newAngle = new Vector3(player.GrabSpot2.transform.eulerAngles.x, player.GrabSpot2.transform.eulerAngles.y, player.GrabSpot2.transform.eulerAngles.z);
                    FFRTarget.DORotate(newAngle, 0.2f);
                }
                else
                {
                    Vector3 newAngle = new Vector3(player.GrabSpot2.transform.eulerAngles.x, player.GrabSpot2.transform.eulerAngles.y, player.GrabSpot2.transform.eulerAngles.z);
                    FFRTarget.DORotate(newAngle, 0.2f);
                }
            }).OnComplete(() => {
                FFRTarget.SetParent(player.GrabSpot2.transform, true);
                Retrieve(player);
            });
        }
        else
        {
            FFLTarget.DOMove(player.GrabSpot2.transform.position, 0.2f).OnStart(() =>
            {
                if (player.GrabSpot2.transform.position.y < FFLTarget.position.y)
                {
                    Vector3 newAngle = new Vector3(player.GrabSpot2.transform.eulerAngles.x, player.GrabSpot2.transform.eulerAngles.y, player.GrabSpot2.transform.eulerAngles.z - 180);
                    FFLTarget.DORotate(newAngle, 0.2f);
                }
                else
                {
                    Vector3 newAngle = new Vector3(player.GrabSpot2.transform.eulerAngles.x, player.GrabSpot2.transform.eulerAngles.y, player.GrabSpot2.transform.eulerAngles.z - 180);
                    FFLTarget.DORotate(newAngle, 0.2f);
                }
            }).OnComplete(() => {
                FFLTarget.SetParent(player.GrabSpot2.transform, true);
            });
            FFRTarget.DOMove(player.GrabSpot1.transform.position, 0.2f).OnStart(() =>
            {
                if (player.GrabSpot1.transform.position.y < FFRTarget.position.y)
                {
                    Vector3 newAngle = new Vector3(player.GrabSpot1.transform.eulerAngles.x, player.GrabSpot1.transform.eulerAngles.y, player.GrabSpot1.transform.eulerAngles.z - 180);
                    FFRTarget.DORotate(newAngle, 0.2f);
                }
                else
                {
                    Vector3 newAngle = new Vector3(player.GrabSpot1.transform.eulerAngles.x, player.GrabSpot1.transform.eulerAngles.y, player.GrabSpot1.transform.eulerAngles.z - 180);
                    FFRTarget.DORotate(newAngle, 0.2f);
                }
            }).OnComplete(() => {
                Retrieve(player);
                FFRTarget.SetParent(player.GrabSpot1.transform, true);
            });
        }
    }

    private void Retrieve(PlayerController player)
    {
        player.transform.SetParent(Bug.transform);
        player.transform.DOMove(MouthLocation.transform.position, 0.1f).OnStart(() => {
            if (player.GrabSpot1.transform.position.y > player.GrabSpot2.transform.position.y)
            {
                player.transform.DORotate(new Vector3(0.0f, 0.0f, -120.0f), 0.1f);
            }
            else
            {
                player.transform.DORotate(new Vector3(0.0f, 0.0f, 60.0f), 0.1f);
            }
        }).OnComplete(() => {
            _bugAnimator.SetTrigger("Chomp");
            StartCoroutine(Open(player));
        });
    }

    IEnumerator Open(PlayerController player)
    {
        Transform thrownPiece;
        ParticleSystem thrownPiecePS;
        Transform eatPiece;

        yield return new WaitForSeconds(0.3f);

        player.PlayerSprite.SetActive(false);
        player.PlayerPiece1.gameObject.SetActive(true);
        player.PlayerPiece2.gameObject.SetActive(true);

        if (player.GrabSpot1.transform.position.y > player.GrabSpot2.transform.position.y)
        {
            player.GrabSpot1.GetComponentInParent<Rigidbody>().isKinematic = false;
            player.GrabSpot2.GetComponentInParent<Rigidbody>().isKinematic = true;

            player.GrabSpot1.GetComponentInParent<Rigidbody>().AddExplosionForce(8.0f, player.PlayerSprite.transform.position, 10.0f, 0.0f, ForceMode.Impulse);
            
            thrownPiece = player.PlayerPiece1.transform;
            thrownPiecePS = player.GrabSpot1FlamePS;
            eatPiece = player.PlayerPiece2.transform;
        }
        else
        {
            player.GrabSpot1.GetComponentInParent<Rigidbody>().isKinematic = true;
            player.GrabSpot2.GetComponentInParent<Rigidbody>().isKinematic = false;

            player.GrabSpot2.GetComponentInParent<Rigidbody>().AddExplosionForce(8.0f, player.PlayerSprite.transform.position, 10.0f, 0.0f, ForceMode.Impulse);

            thrownPiece = player.PlayerPiece2.transform;
            thrownPiecePS = player.GrabSpot2FlamePS;
            eatPiece = player.PlayerPiece1.transform;
        }

        yield return new WaitForSeconds(0.2f);

        _fflAvailable = true;
        FFLTarget.SetParent(Bug.transform, true);
        thrownPiece.parent = null;
        thrownPiecePS.Play();
        thrownPiece.DOLocalMoveZ(0.0f, 0.3f);

        if (_punchQueue.Count == 0 && _eatQueue.Count == 0)
        {
            FFLTarget.DOLocalMove(_fflTargetOGPosition, 0.2f);
            FFLTarget.DOLocalRotate(_fflTargetOGRotation, 0.2f);
        }

        Eat(eatPiece);
    }

    private void Eat(Transform eatPiece)
    {
        Vector3 ogPosition = eatPiece.localPosition;
        Vector3 pullAwayPosition = new Vector3(eatPiece.position.x + 3, eatPiece.position.y - 2, eatPiece.position.z);
        Vector3 pullAwayRotation = new Vector3(eatPiece.eulerAngles.x, eatPiece.eulerAngles.y, eatPiece.eulerAngles.z + 80.0f);

        eatPiece.DOMove(pullAwayPosition, 0.2f).OnStart(() => {
            eatPiece.DORotate(pullAwayRotation, 0.2f);
        }).OnComplete(() => {
            eatPiece.DOLocalMove(ogPosition, 0.1f).OnComplete(() => {
                _bugAnimator.SetTrigger("Chomping");
            });
        });

        StartCoroutine(FinishEating(eatPiece));
    }

    IEnumerator FinishEating(Transform eatPiece)
    {
        yield return new WaitForSeconds(0.6f);

        eatPiece.GetComponent<Rigidbody>().isKinematic = false;
        eatPiece.GetComponent<Rigidbody>().AddExplosionForce(10.0f, eatPiece.parent.position, 10.0f, 0.0f, ForceMode.Impulse);
        yield return new WaitForSeconds(0.2f);

        eatPiece.parent = null;
        eatPiece.DOMoveZ(0.0f, 0.3f);
        _ffrAvailable = true;
        FFRTarget.SetParent(Bug.transform, true);
        _bugAnimator.SetTrigger("NotChomping");

        if (_punchQueue.Count == 0 && _eatQueue .Count == 0)
        {
            FFRTarget.DOLocalMove(_ffrTargetOGPosition, 0.2f);
            FFRTarget.DOLocalRotate(_ffrTargetOGRotation, 0.2f);
        }
    }

    private void Punch(GhostController caughtGhost)
    {
        if (_fflAvailable && _ffrAvailable)
        {
            if (Vector3.Distance(FFLTarget.transform.position, caughtGhost.transform.position) < Vector3.Distance(FFRTarget.transform.position, caughtGhost.transform.position))
            {
                StartFLPunch(caughtGhost);
            }
            else
            {
                StartFRPunch(caughtGhost);
            }
        }
        else if(_fflAvailable)
        {
            StartFLPunch(caughtGhost);
        }
        else if(_ffrAvailable)
        {
            StartFRPunch(caughtGhost);
        }
    }

    private void StartFRPunch(GhostController caughtGhost)
    {
        _ffrAvailable = false;
        FFRTarget.parent = null;
        float distToGrabBottom = Vector3.Distance(FFRRaycastOrigin.position, caughtGhost.GrabPointBottom.position);
        float distToGrabTop = Vector3.Distance(FFRRaycastOrigin.position, caughtGhost.GrabPointTop.position);
        Vector3 position = (distToGrabBottom < distToGrabTop) ? caughtGhost.GrabPointBottom.position : caughtGhost.GrabPointTop.position;
        position = new Vector3(position.x, position.y, -3.0f);
        if (_ffrTweener != null && _ffrTweener.IsPlaying())
            _ffrTweener.Kill();

        _ffrTweener = FFRTarget.DOMove(position, 0.2f).OnStart(() =>
        {
            if (distToGrabBottom < distToGrabTop)
                FFRTarget.DORotate(caughtGhost.GrabPointBottom.eulerAngles, 0.2f);
            else
                FFRTarget.DORotate(caughtGhost.GrabPointTop.eulerAngles, 0.2f);
        }).OnComplete(() => {
            StartCoroutine(BringFFRBack(caughtGhost));
        });
    }

    private void StartFLPunch(GhostController caughtGhost)
    {
        _fflAvailable = false;
        FFLTarget.parent = null;
        float distToGrabBottom = Vector3.Distance(FFLRaycastOrigin.position, caughtGhost.GrabPointBottom.position);
        float distToGrabTop = Vector3.Distance(FFLRaycastOrigin.position, caughtGhost.GrabPointTop.position);
        Vector3 position = (distToGrabBottom < distToGrabTop) ? caughtGhost.GrabPointBottom.position : caughtGhost.GrabPointTop.position;
        position = new Vector3(position.x, position.y, -3.0f);
        if (_fflTweener != null && _fflTweener.IsPlaying())
            _fflTweener.Kill();

        _fflTweener = FFLTarget.DOMove(position, 0.2f).OnStart(() =>
        {
            if (distToGrabBottom < distToGrabTop)
                FFLTarget.DORotate(caughtGhost.GrabPointBottom.eulerAngles, 0.2f);
            else
                FFLTarget.DORotate(caughtGhost.GrabPointTop.eulerAngles, 0.2f);
        }).OnComplete(() => {
            StartCoroutine(BringFFLBack(caughtGhost));
        });
    }

    IEnumerator BringFFRBack(GhostController caughtGhost)
    {
        yield return new WaitForSeconds(0.1f);

        FFRTarget.SetParent(Bug.transform, true);
        caughtGhost.Caught();
        _ffrAvailable = true;
        if(_punchQueue.Count == 0 && _eatQueue.Count == 0)
        {
            FFRTarget.DOLocalMove(_ffrTargetOGPosition, 0.2f);
            FFRTarget.DOLocalRotate(_ffrTargetOGRotation, 0.2f);
        }

        yield return null;
    }

    IEnumerator BringFFLBack(GhostController caughtGhost)
    {
        yield return new WaitForSeconds(0.1f);

        FFLTarget.SetParent(Bug.transform, true);
        caughtGhost.Caught();
        _fflAvailable = true;
        if (_punchQueue.Count == 0 && _eatQueue.Count == 0)
        {
            FFLTarget.DOLocalMove(_fflTargetOGPosition, 0.2f);
            FFLTarget.DOLocalRotate(_fflTargetOGRotation, 0.2f);
        }

        yield return null;
    }

    void SetLegs()
    {
        if (Vector3.Distance(BLRaycastOrigin.position, BLTarget.position) >= _maxDistance && !DOTween.IsTweening(BLTarget, true))
            PlaceBackLeg(-60, BLTarget, BLRaycastOrigin);

        if (Vector3.Distance(BRRaycastOrigin.position, BRTarget.position) >= _maxDistance && !DOTween.IsTweening(BRTarget, true))
            PlaceBackLeg(60, BRTarget, BRRaycastOrigin);

        if (Vector3.Distance(FLRaycastOrigin.position, FLTarget.position) <= _flPreviousDistance && !DOTween.IsTweening(FLTarget, true))
            _flPreviousDistance = Vector3.Distance(FLRaycastOrigin.position, FLTarget.position);
        else if (!DOTween.IsTweening(FLTarget, true))
            PlaceLeg(-1, FLTarget, FLRaycastOrigin);
        else
            _flPreviousDistance = _maxDistance;

        if (Vector3.Distance(FRRaycastOrigin.position, FRTarget.position) <= _frPreviousDistance && !DOTween.IsTweening(FRTarget, true))
            _frPreviousDistance = Vector3.Distance(FRRaycastOrigin.position, FRTarget.position);
        else if (!DOTween.IsTweening(FRTarget, true))
            PlaceLeg(1, FRTarget, FRRaycastOrigin);
        else
            _frPreviousDistance = _maxDistance;
    }

    void PlaceLeg(int rotationDirection, Transform legTarget, Transform RaycastOrigin)
    {
        for (int i = 0; i != 90 * rotationDirection; i = i + 5 * rotationDirection)
        {
            _direction = Quaternion.AngleAxis(i, Vector3.back) * Bug.transform.right;
            Debug.DrawRay(RaycastOrigin.position, _direction * _maxDistance, Color.blue, 10.0f);

            if (Physics.Raycast(RaycastOrigin.position, _direction, out _hit, _maxDistance, 1 << LayerMask.NameToLayer("Wall") | 1 << LayerMask.NameToLayer("BugRail")))
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

        Vector3 BugMoveDirection = (_hit.point - Bug.transform.position) * 0.1f;

        Debug.DrawRay(Bug.transform.position, BugMoveDirection, Color.yellow, 3.0f);
        //Bug.transform.DOBlendableMoveBy(BugMoveDirection, 0.075f).SetDelay(0.15f).OnComplete(() => {
        //    Bug.transform.DOBlendableMoveBy(-BugMoveDirection, 0.075f);
        //});
    }

    void PlaceBackLeg(int rotationDirection, Transform legTarget, Transform RaycastOrigin)
    {
        int rotationSign = (rotationDirection < 0) ? -1 : 1;
        _direction = Quaternion.AngleAxis(rotationDirection, Vector3.back) * Bug.transform.right;
        Debug.DrawRay(RaycastOrigin.position, _direction * _maxDistance, Color.blue, 10.0f);

        if (Physics.Raycast(RaycastOrigin.position, _direction, out _hit, _maxDistance))
        {
            Vector3 halfwayPoint = ((_hit.point - legTarget.position) * 0.5f) + legTarget.position;
            Vector3 halfwayPointDirection = _hit.point - legTarget.position;

            Vector3 direction = Quaternion.AngleAxis(90 * -rotationSign, Vector3.back) * halfwayPointDirection;
            halfwayPoint = halfwayPoint + (direction * 0.1f);
            Vector3[] path = new Vector3[] { legTarget.position, halfwayPoint, _hit.point };

            legTarget.DOPath(path, 0.15f, PathType.CatmullRom, PathMode.Full3D, 5, Color.green).SetEase(Ease.Linear);
            legTarget.DORotateQuaternion(Quaternion.LookRotation(_hit.normal), 0.15f);

            Vector3 BugMoveDirection = (_hit.point - Bug.transform.position) * 0.2f;

            Debug.DrawRay(Bug.transform.position, BugMoveDirection, Color.yellow, 3.0f);
            Bug.transform.DOBlendableMoveBy(BugMoveDirection, 0.15f).OnComplete(() =>{
                Bug.transform.DOBlendableMoveBy(-BugMoveDirection, 0.15f);
            });
        }
    }
}
