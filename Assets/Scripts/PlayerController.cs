using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    public PlayerInput HookPlayerInput;
    public float Speed = 10.0f;
    public float BoostForce = 5.0f;
    public float HookSpeed = 80.0f;
    public float LineSpeed = 90.0f;
    public float ClimbSpeed = 30.0f;
    public float ClimbSlowDownForce = 20.0f;
    public GameObject GrappleArmEnd;
    public delegate void OnPlayerDiedEvent();
    public event OnPlayerDiedEvent OnPlayerDied;
    public delegate void OnPlayerWonEvent();
    public event OnPlayerWonEvent OnPlayerWon;

    private Vector3 _playerStartPosition;
    private GameObject _playerBody;
    private GameObject _grappleShoulder;
    private Rigidbody _playerRigidbody;
    private bool _grounded = false;
    private GameObject _wallHookGraphic;
    private LineRenderer _ropeLineRenderer;
    private List<float> _ropeBendAngles = new List<float>();
    private List<Vector3> _lineRenderPositions = new List<Vector3>();
    private GameObject _wallHook;
    private FixedJoint _wallHookFixedJoint;
    private Vector3 _wallHookHitPosition = new Vector3();
    private bool _hookActive = false;
    private bool _hooked = false;
    private AudioSource _playerAudio;
    private AudioClip _hookHitSoundEffect;
    private AudioClip _hookFireSoundEffect;
    private Plane _gameSurfacePlane = new Plane(Vector3.back, Vector3.zero);

    void Awake()
    {
        _playerStartPosition = transform.position;
        _wallHookGraphic = GameObject.Find("WallHook").gameObject;
        _wallHook = new GameObject();
        _wallHook.name = "WallHookFixedJoint";
        _wallHookFixedJoint = _wallHook.AddComponent<FixedJoint>();
        _wallHook.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionX |
                                                         RigidbodyConstraints.FreezePositionY |
                                                         RigidbodyConstraints.FreezePositionZ |
                                                         RigidbodyConstraints.FreezeRotationX |
                                                         RigidbodyConstraints.FreezeRotationY;
        _ropeLineRenderer = _wallHookGraphic.GetComponent<LineRenderer>();
        _playerBody = transform.Find("PlayerBody").gameObject;
        _playerRigidbody = GetComponent<Rigidbody>();
        _grappleShoulder = _playerBody.transform.Find("GrappleShoulder").gameObject;
        _playerAudio = GetComponent<AudioSource>();
        _hookFireSoundEffect = Resources.Load("SoundEffects/GunFire") as AudioClip;
        _hookHitSoundEffect = Resources.Load("SoundEffects/GunHit") as AudioClip;
    }

    public void Init()
    {
        StopAllCoroutines();
        _ropeLineRenderer.enabled = false;
        _wallHookGraphic.transform.position = GrappleArmEnd.transform.position;
        _wallHookGraphic.transform.parent = GrappleArmEnd.transform;
        _wallHookFixedJoint.connectedBody = null;
        transform.GetComponent<Rigidbody>().velocity = new Vector3(0.0f, 0.0f, 0.0f);
        _ropeBendAngles.Clear();
        _lineRenderPositions.Clear();
        _hookActive = false;
        _hooked = false;
        transform.position = _playerStartPosition;
    }

    void Update()
    {
        Debug.Log(_grounded);
        Quaternion grappleShoulderRotation = new Quaternion();

        if (HookPlayerInput.HookPressed())
        {
            if (!_hookActive && !_hooked && _grounded)
            {
                BoostPlayer();
            }
            else if (!_hookActive && !_hooked && !_grounded)
            {
                if (CheckHookHit())
                {
                    StartCoroutine(ShootHook(_wallHookHitPosition));
                }
            }
            else if (!_hookActive && _hooked && !_grounded)
            {
                BoostPlayer();
                StartCoroutine(RetrieveHookRope());
            }
            else if (!_hookActive && _hooked && _grounded)
            {
                StartCoroutine(RetrieveHookRope());
            }
        }

        if (_hooked || _hookActive)
        {
            if (_lineRenderPositions.Count > 0)
            {
                grappleShoulderRotation = Quaternion.LookRotation(_lineRenderPositions[_lineRenderPositions.Count - 1] - _grappleShoulder.transform.position, Vector3.back);
            }
        }
        else
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(new Vector3(HookPlayerInput.GetPlayerTouchPosition().x,
                                                                        HookPlayerInput.GetPlayerTouchPosition().y,
                                                                    -(Camera.main.transform.position.z + transform.position.z) + 1));
            grappleShoulderRotation = Quaternion.LookRotation(mousePosition - _grappleShoulder.transform.position, Vector3.back);
        }

        grappleShoulderRotation.x = 0.0f;
        grappleShoulderRotation.y = 0.0f;
        _grappleShoulder.transform.rotation = grappleShoulderRotation;
    }

    void FixedUpdate()
    {
        if (_hooked && _lineRenderPositions.Count > 0)
        {
            CheckRopeSlack();
            RaycastHit playerRaycastOut;
            Vector3 direction = _lineRenderPositions[_lineRenderPositions.Count - 1] - transform.position;
            bool hit = Physics.Raycast(transform.position, direction, out playerRaycastOut, direction.magnitude, 1 << LayerMask.NameToLayer("Ground"));

            if (hit)
            {
                RaycastHit nextPlayerRaycastOut;
                if (Physics.Raycast(_lineRenderPositions[_lineRenderPositions.Count - 1], -direction, out nextPlayerRaycastOut, Mathf.Infinity, 1 << LayerMask.NameToLayer("Ground")))
                {
                    Debug.DrawRay(_lineRenderPositions[_lineRenderPositions.Count - 1], -direction, Color.yellow);
                    if (playerRaycastOut.transform.gameObject == nextPlayerRaycastOut.transform.gameObject)
                    {
                        Vector3 cornerNormal = playerRaycastOut.normal + nextPlayerRaycastOut.normal;

                        //Debug.DrawRay(playerRaycastOut.point, playerRaycastOut.normal, Color.blue, 10.0f);
                        //Debug.DrawRay(nextPlayerRaycastOut.point, nextPlayerRaycastOut.normal, Color.blue, 10.0f);
                        //Debug.DrawRay(nextPlayerRaycastOut.point, cornerNormal, Color.red, 10.0f);
                        //Debug.DrawRay(nextPlayerRaycastOut.point, cornerNormal, Color.red, 10.0f);

                        float modifier = Mathf.Sign(AngleFromAToB(playerRaycastOut.normal, cornerNormal));

                        // Wish I knew a way to make these infinite
                        Vector3 pointDirection1 = (Quaternion.Euler(0, 0, modifier * -45) * cornerNormal) * 100.0f;
                        Debug.DrawRay(nextPlayerRaycastOut.point, pointDirection1, Color.green);

                        Vector3 pointDirection2 = (Quaternion.Euler(0, 0, modifier * 45) * cornerNormal) * 100.0f;
                        Debug.DrawRay(playerRaycastOut.point, pointDirection2, Color.green);

                        try
                        {
                            Vector2 intersection2D = Math3d.LineIntersectionPoint(nextPlayerRaycastOut.point, nextPlayerRaycastOut.point + pointDirection1 * 10.0f, playerRaycastOut.point, playerRaycastOut.point + pointDirection2 * 10.0f);
                            Vector3 intersection = new Vector3(intersection2D.x, intersection2D.y, 0.0f);
                            intersection = intersection + (cornerNormal.normalized * 0.1f);
                            _lineRenderPositions.Add(intersection);
                            _wallHook.GetComponent<FixedJoint>().connectedBody = null;
                            _wallHook.transform.position = intersection;
                            _wallHookFixedJoint.connectedBody = transform.GetComponent<Rigidbody>();

                            // store rope bend polarity to check when we swing back
                            Vector3 playersAngle = transform.position - _lineRenderPositions[_lineRenderPositions.Count - 1];
                            Vector3 previousAngle = _lineRenderPositions[_lineRenderPositions.Count - 1] - _lineRenderPositions[_lineRenderPositions.Count - 2];
                            _ropeBendAngles.Add(AngleFromAToB(playersAngle, previousAngle));
                        }
                        catch
                        {
                            // Lines were parallel need to implement logic
                            Debug.Log("Lines were parallel doing nothing");
                        }
                    }
                }
            }

            if (_lineRenderPositions.Count > 1)
            {
                Vector3 playersAngle = transform.position - _lineRenderPositions[_lineRenderPositions.Count - 1];
                Vector3 previousAngle = _lineRenderPositions[_lineRenderPositions.Count - 1] - _lineRenderPositions[_lineRenderPositions.Count - 2];
                float currentAngle = AngleFromAToB(playersAngle, previousAngle);

                if (Mathf.Sign(currentAngle) != Mathf.Sign(_ropeBendAngles[_ropeBendAngles.Count - 1]))
                {
                    _wallHook.GetComponent<FixedJoint>().connectedBody = null;
                    _wallHook.transform.position = _lineRenderPositions[_lineRenderPositions.Count - 2];
                    _lineRenderPositions.RemoveAt(_lineRenderPositions.Count - 1);
                    _wallHookFixedJoint.connectedBody = transform.GetComponent<Rigidbody>();
                    _ropeBendAngles.RemoveAt(_ropeBendAngles.Count - 1);
                }
            }
        }

        HandleMove();
    }

    void LateUpdate()
    {
        // adjust playerBody for parents rotation
        _playerBody.transform.rotation = Quaternion.Euler(transform.rotation.x, transform.rotation.y, -transform.rotation.z);

        if (_lineRenderPositions.Count > 0 && (_hookActive || _hooked))
        {
            // draw rope if hooked
            _ropeLineRenderer.positionCount = _lineRenderPositions.Count + 1;
            for (int i = 0; i < _lineRenderPositions.Count; i++)
            {
                _ropeLineRenderer.SetPosition(i, _lineRenderPositions[i]);
            }
            _ropeLineRenderer.SetPosition(_lineRenderPositions.Count, _grappleShoulder.transform.position);
        }
    }

    void HandleMove()
    {
        if ((HookPlayerInput.Move() < 0 || HookPlayerInput.Move() > 0) && (_grounded))
            GetComponent<Rigidbody>().velocity = new Vector2(HookPlayerInput.Move() * Speed, GetComponent<Rigidbody>().velocity.y);
    }

    IEnumerator ShootHook(Vector3 location)
    {
        _hookActive = true;
        _playerAudio.PlayOneShot(_hookFireSoundEffect);
        float timePassed = 0;

        _wallHookGraphic.transform.parent = null;
        _ropeLineRenderer.enabled = true;
        Vector3 origin = _wallHookGraphic.transform.position;
        Vector3 ropeEndPoint = new Vector3();
        var dist = Vector3.Distance(_wallHookGraphic.transform.position, location);
        _ropeLineRenderer.positionCount = 2;
        float timeTakenDuringLerp = dist / HookSpeed;

        while (timePassed < timeTakenDuringLerp)
        {
            float percentageComplete = timePassed / timeTakenDuringLerp;

            _wallHookGraphic.transform.position = Vector3.Lerp(_wallHookGraphic.transform.position,
                                                        location,
                                                        percentageComplete);

            ropeEndPoint = Vector3.Lerp(origin, location, percentageComplete);
            _ropeLineRenderer.SetPosition(0, _grappleShoulder.transform.position);
            _ropeLineRenderer.SetPosition(1, ropeEndPoint);

            timePassed += Time.deltaTime;
            yield return null;
        }

        _lineRenderPositions.Add(_wallHookGraphic.transform.position);
        _wallHook.transform.position = ropeEndPoint;
        _wallHookFixedJoint.connectedBody = transform.GetComponent<Rigidbody>();
        _playerAudio.PlayOneShot(_hookHitSoundEffect);

        _hooked = true;
        _hookActive = false;
    }

    IEnumerator RetrieveHookRope()
    {
        // have to fix the line coming back
        _hooked = false;
        _hookActive = true;
        _playerAudio.PlayOneShot(_hookHitSoundEffect);
        _wallHookFixedJoint.connectedBody = null;
        float elapsedTime = 0;
        float dist;
        Vector3 startPosition = new Vector3();
        Vector3 endPosition = new Vector3();
        if (_lineRenderPositions.Count > 1)
        {
            dist = Vector3.Distance(_lineRenderPositions[0], _lineRenderPositions[1]);
            startPosition = _lineRenderPositions[0];
            endPosition = _lineRenderPositions[1];
        }
        else
        {
            dist = Vector3.Distance(_lineRenderPositions[0], GrappleArmEnd.transform.position);
            startPosition = _lineRenderPositions[0];
            endPosition = GrappleArmEnd.transform.position;
        }
        float timeTakenDuringLerp = dist / HookSpeed;
        while (elapsedTime < timeTakenDuringLerp)
        {
            // retrieve rope
            float percentageComplete = elapsedTime / timeTakenDuringLerp;
            //Debug.Log("percentage complete: " + percentageComplete + "   elapsed time: " + elapsedTime + "   line render position: " + lineRenderPositions[0] + "time taken: " + timeTakenDuringLerp);
            _lineRenderPositions[0] = Vector3.Lerp(startPosition, endPosition, percentageComplete);

            // retrieve hook
            _wallHookGraphic.transform.position = _lineRenderPositions[0];

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        _lineRenderPositions.RemoveAt(0);
        if (_lineRenderPositions.Count > 0)
            StartCoroutine(RetrieveHookRope());
        else
        {
            _ropeLineRenderer.enabled = false;
            _wallHookGraphic.transform.position = GrappleArmEnd.transform.position;
            _wallHookGraphic.transform.parent = GrappleArmEnd.transform;
            
        }
        _hookActive = false;
    }

    void BoostPlayer()
    {
        Vector3 direction = Camera.main.ScreenToWorldPoint(new Vector3(HookPlayerInput.GetPlayerTouchPosition().x,
                                                                          HookPlayerInput.GetPlayerTouchPosition().y,
                                                                        -(Camera.main.transform.position.z + transform.position.z)));
        direction = direction - transform.position;
        _playerRigidbody.AddForce(direction.normalized * BoostForce, ForceMode.VelocityChange);
    }

    bool CheckHookHit()
    {
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(HookPlayerInput.GetPlayerTouchPosition().x,
                                                           HookPlayerInput.GetPlayerTouchPosition().y,
                                                           0.0f));
        float ent;
        if (_gameSurfacePlane.Raycast(ray, out ent))
        {
            RaycastHit wallHit = new RaycastHit();
            Vector3 wallHookPosition = ray.GetPoint(ent);
            Vector3 origin = new Vector3(_grappleShoulder.transform.position.x, _grappleShoulder.transform.position.y, _grappleShoulder.transform.position.z + -_grappleShoulder.transform.position.z);
            Vector3 direction = wallHookPosition - origin;
            if (Physics.Raycast(origin, direction, out wallHit, Mathf.Infinity, 1 << LayerMask.NameToLayer("Ground")))
            {
                _wallHookHitPosition = wallHit.point + wallHit.normal.normalized * 0.1f;
                return true;
            }
            else
                return false;
        }
        else
            return false;
    }

    void CheckRopeSlack()
    {
        if (!_grounded)
        {
            bool playerMovingTowardHook = Math3d.ObjectMovingTowards(_lineRenderPositions[_lineRenderPositions.Count - 1],
                                                                                 transform.position,
                                                                                 transform.GetComponent<Rigidbody>().velocity);
            if (playerMovingTowardHook || HookPlayerInput.RopeReleasePressed())
            {
                Debug.Log("RELEASED");
                _wallHookFixedJoint.connectedBody = null;
            }
                
            else
                _wallHookFixedJoint.connectedBody = transform.GetComponent<Rigidbody>();
        }
    }

    IEnumerator PlayerDied()
    {
        if(OnPlayerDied != null)
            //OnPlayerDied();
        yield return null;
    }

    IEnumerator PlayerWon()
    {
        if(OnPlayerWon != null)
            OnPlayerWon();
        yield return null;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Lava") || collision.gameObject.layer == LayerMask.NameToLayer("Ground") || collision.gameObject.layer == LayerMask.NameToLayer("Bullet"))
        {
            StartCoroutine("PlayerDied");
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            _grounded = true;
            transform.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionZ |
                                                              RigidbodyConstraints.FreezeRotationX |
                                                              RigidbodyConstraints.FreezeRotationY |
                                                              RigidbodyConstraints.FreezeRotationZ;
            if (_hooked)
            {
                _wallHookFixedJoint.connectedBody = null;
            }
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            _grounded = false;
            transform.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionZ |
                                                              RigidbodyConstraints.FreezeRotationX |
                                                              RigidbodyConstraints.FreezeRotationY;
            if (_hooked)
            {
                _wallHookFixedJoint.connectedBody = transform.GetComponent<Rigidbody>();
            }
        }
    }

    float AngleFromAToB(Vector3 angleA, Vector3 angleB)
    {
        Vector3 axis = new Vector3(0, 0, 1);
        float angle = Vector3.Angle(angleA, angleB);
        float sign = Mathf.Sign(Vector3.Dot(axis, Vector3.Cross(angleA, angleB)));

        // angle in [-179,180]
        float signed_angle = angle * sign;
        return signed_angle;
    }
}
