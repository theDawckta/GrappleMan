using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    public PlayerInput HookPlayerInput;
    public float Speed = 10.0f;
    public float MaxVelocity = 10.0f;
    public float HookSpeed = 80.0f;
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
        _ropeLineRenderer.positionCount = 0;
        _hookActive = false;
        _hooked = false;
        transform.position = _playerStartPosition;
    }

    void Update()
    {
        Quaternion grappleShoulderRotation = new Quaternion();

		if (_hooked || _hookActive)
        {
			_ropeLineRenderer.SetPosition(_ropeLineRenderer.positionCount - 1, _grappleShoulder.transform.position);
		}

        if (HookPlayerInput.HookPressed())
        {
            if (!_hookActive && !_hooked)
            {
                if (CheckHookHit())
                {
                    StartCoroutine(ShootHook(_wallHookHitPosition));
                }
            }
            else if (!_hookActive && _hooked)
            {
                ClimbRope();
            }
        }
        else if (HookPlayerInput.HookReleased())
        {
            if (!_hookActive && _hooked && _ropeLineRenderer.positionCount > 1)
            {
				StartCoroutine(RetrieveHookSegment());
            }
        }

        if (_hooked)
			grappleShoulderRotation = Quaternion.LookRotation(_ropeLineRenderer.GetPosition(_ropeLineRenderer.positionCount - 2) - _ropeLineRenderer.GetPosition(_ropeLineRenderer.positionCount - 1), Vector3.back);
		else if (_hookActive)
			grappleShoulderRotation = Quaternion.LookRotation(_wallHookGraphic.transform.position - _grappleShoulder.transform.position, Vector3.back);
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
        // limit velocity
        if (_playerRigidbody.velocity.magnitude > MaxVelocity)
        {
            float brakeSpeed = _playerRigidbody.velocity.magnitude - MaxVelocity;
            Vector3 normalisedVelocity = _playerRigidbody.velocity.normalized;
            Vector3 brakeVelocity = normalisedVelocity * brakeSpeed;
            _playerRigidbody.AddForce(-brakeVelocity);
        }

        if ((_hooked) && _ropeLineRenderer.positionCount > 0)
        {
            if(_hooked)
                CheckRopeSlack();

            RaycastHit playerRaycastOut;
			Vector3 direction = _ropeLineRenderer.GetPosition(_ropeLineRenderer.positionCount - 2) - transform.position;
            bool hit = Physics.Raycast(transform.position, direction, out playerRaycastOut, direction.magnitude, 1 << LayerMask.NameToLayer("Ground"));

            if (hit)
            {
                RaycastHit nextPlayerRaycastOut;
                if (Physics.Raycast(_ropeLineRenderer.GetPosition(_ropeLineRenderer.positionCount - 2), -direction, out nextPlayerRaycastOut, Mathf.Infinity, 1 << LayerMask.NameToLayer("Ground")))
                {
                    if (playerRaycastOut.transform.gameObject == nextPlayerRaycastOut.transform.gameObject)
                    {
                        Vector3 cornerNormal = playerRaycastOut.normal + nextPlayerRaycastOut.normal;

//                        Debug.DrawRay(playerRaycastOut.point, playerRaycastOut.normal, Color.blue, 10.0f);
//                        Debug.DrawRay(nextPlayerRaycastOut.point, nextPlayerRaycastOut.normal, Color.blue, 10.0f);
//                        Debug.DrawRay(nextPlayerRaycastOut.point, cornerNormal, Color.red, 10.0f);
//                        Debug.DrawRay(nextPlayerRaycastOut.point, cornerNormal, Color.red, 10.0f);

                        float modifier = Mathf.Sign(AngleFromAToB(playerRaycastOut.normal, cornerNormal));

                        // Wish I knew a way to make these infinite
                        Vector3 pointDirection1 = (Quaternion.Euler(0, 0, modifier * -45) * cornerNormal) * 100.0f;
//                        Debug.DrawRay(nextPlayerRaycastOut.point, pointDirection1, Color.green, 10.0f);

                        Vector3 pointDirection2 = (Quaternion.Euler(0, 0, modifier * 45) * cornerNormal) * 100.0f;
//                        Debug.DrawRay(playerRaycastOut.point, pointDirection2, Color.green, 10.0f);

                        try
                        {
                            Vector2 intersection2D = Math3d.LineIntersectionPoint(nextPlayerRaycastOut.point, nextPlayerRaycastOut.point + pointDirection1 * 10.0f, playerRaycastOut.point, playerRaycastOut.point + pointDirection2 * 10.0f);
                            Vector3 intersection = new Vector3(intersection2D.x, intersection2D.y, 0.0f);
                            intersection = intersection + (cornerNormal.normalized * 0.1f);
                            _ropeLineRenderer.positionCount = _ropeLineRenderer.positionCount + 1;
                            _ropeLineRenderer.SetPosition(_ropeLineRenderer.positionCount - 2, intersection);
                            _wallHook.GetComponent<FixedJoint>().connectedBody = null;
                            _wallHook.transform.position = intersection;
                            _wallHookFixedJoint.connectedBody = _playerRigidbody;

							Vector3 playersAngle = transform.position - _ropeLineRenderer.GetPosition(_ropeLineRenderer.positionCount - 2);
							Vector3 previousAngle = _ropeLineRenderer.GetPosition(_ropeLineRenderer.positionCount - 2) - _ropeLineRenderer.GetPosition(_ropeLineRenderer.positionCount - 3);
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

            // check for line unwinding
			if (_ropeLineRenderer.positionCount > 2)
            {
				Vector3 playersAngle = transform.position - _ropeLineRenderer.GetPosition(_ropeLineRenderer.positionCount - 2);
				Vector3 previousAngle = _ropeLineRenderer.GetPosition(_ropeLineRenderer.positionCount - 2) - _ropeLineRenderer.GetPosition(_ropeLineRenderer.positionCount - 3);
                float currentAngle = AngleFromAToB(playersAngle, previousAngle);

                if (Mathf.Sign(currentAngle) != Mathf.Sign(_ropeBendAngles[_ropeBendAngles.Count - 1]))
                {
                    _wallHook.GetComponent<FixedJoint>().connectedBody = null;
                    _wallHook.transform.position = _ropeLineRenderer.GetPosition(_ropeLineRenderer.positionCount - 2);
                    _ropeLineRenderer.positionCount = _ropeLineRenderer.positionCount - 1;
                    _wallHookFixedJoint.connectedBody = _playerRigidbody;
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
    }

    void HandleMove()
    {
    	// moves player left and right
        if ((HookPlayerInput.Move() < 0 || HookPlayerInput.Move() > 0) && (_grounded))
            GetComponent<Rigidbody>().velocity = new Vector2(HookPlayerInput.Move() * Speed, GetComponent<Rigidbody>().velocity.y);
    }

    IEnumerator ShootHook(Vector3 location)
    {
        float timePassed = 0;
        _hookActive = true;
        _playerAudio.PlayOneShot(_hookFireSoundEffect);

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
            _wallHookGraphic.transform.position = Vector3.Lerp(origin, location, percentageComplete);
            _ropeLineRenderer.SetPosition(1, _grappleShoulder.transform.position);
            _ropeLineRenderer.SetPosition(0, _wallHookGraphic.transform.position);

            timePassed += Time.deltaTime;
            yield return null;
        }

		_ropeLineRenderer.SetPosition(1, _grappleShoulder.transform.position);
        _ropeLineRenderer.SetPosition(0, _wallHookGraphic.transform.position);
        _wallHook.transform.position = ropeEndPoint;
        _wallHookFixedJoint.transform.position = location;
        _wallHookFixedJoint.connectedBody = _playerRigidbody;
        _playerAudio.PlayOneShot(_hookHitSoundEffect);

        _hooked = true;
        _hookActive = false;
    }

    IEnumerator RetrieveHookSegment()
    {
        _hooked = false;
        _hookActive = true;
        _playerAudio.PlayOneShot(_hookHitSoundEffect);
        _wallHookFixedJoint.connectedBody = null;
        float elapsedTime = 0;
        float timeTakenDuringLerp = 0.0f;
		Vector3 startPosition;
        Vector3 endPosition;

        startPosition = _ropeLineRenderer.GetPosition(0);
		endPosition = _ropeLineRenderer.GetPosition(1);

        while (elapsedTime < timeTakenDuringLerp || elapsedTime == 0.0f)
        {
            if (_ropeLineRenderer.positionCount == 2)
                endPosition = GrappleArmEnd.transform.position;

            timeTakenDuringLerp =  Vector3.Distance(startPosition, endPosition) / HookSpeed;
            float percentageComplete = elapsedTime / timeTakenDuringLerp;
            _ropeLineRenderer.SetPosition(0, Vector3.Lerp(startPosition, endPosition, percentageComplete));
			_wallHookGraphic.transform.position = _ropeLineRenderer.GetPosition(0);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

		// Shift _ropeLineRenderer Positions left
		Vector3[] tempLineRendererPositions = new Vector3[_ropeLineRenderer.positionCount];
		Vector3[] tempLineRendererPositionsNew = new Vector3[_ropeLineRenderer.positionCount];
		_ropeLineRenderer.GetPositions(tempLineRendererPositions);
		Array.Copy(tempLineRendererPositions, 1, tempLineRendererPositionsNew, 0, tempLineRendererPositions.Length - 1);
		_ropeLineRenderer.SetPositions(tempLineRendererPositionsNew);
		_ropeLineRenderer.positionCount = _ropeLineRenderer.positionCount - 1;

        if (_ropeLineRenderer.positionCount > 1)
        {
            yield return StartCoroutine(RetrieveHookSegment());
        }

        _ropeLineRenderer.enabled = false;
        _wallHookGraphic.transform.position = GrappleArmEnd.transform.position;
        _wallHookGraphic.transform.parent = GrappleArmEnd.transform;
        _hookActive = false;
    }

    void ClimbRope()
    {
        _wallHookFixedJoint.connectedBody = null;
		Vector3 climbForce = (_ropeLineRenderer.GetPosition(_ropeLineRenderer.positionCount - 2) - _ropeLineRenderer.GetPosition(_ropeLineRenderer.positionCount - 1)).normalized;
		climbForce = climbForce * ClimbSpeed / Time.deltaTime;
        _playerRigidbody.AddForce(climbForce, ForceMode.Acceleration);
    }

    bool CheckHookHit()
    {
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(HookPlayerInput.GetPlayerTouchPosition().x,
                                                           HookPlayerInput.GetPlayerTouchPosition().y,
                                                           0.0f));
        float enter;
        if (_gameSurfacePlane.Raycast(ray, out enter))
        {
            RaycastHit wallHit = new RaycastHit();
            Vector3 wallHookPosition = ray.GetPoint(enter);
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
            bool playerMovingTowardHook = Math3d.ObjectMovingTowards(_ropeLineRenderer.GetPosition(_ropeLineRenderer.positionCount - 2),
                                                                     transform.position,
                                                                     transform.GetComponent<Rigidbody>().velocity);
            if (playerMovingTowardHook || HookPlayerInput.RopeReleasePressed())
            {
                _wallHookFixedJoint.connectedBody = null;
            }
            else
                _wallHookFixedJoint.connectedBody = _playerRigidbody;
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
                _wallHookFixedJoint.connectedBody = _playerRigidbody;
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