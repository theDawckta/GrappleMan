using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
	public GameObject TestObject;
	public Text TestText;

    public PlayerInput HookPlayerInput;
    public float Speed = 10.0f;
    public float BoostForce = 10.0f;
    public float MaxVelocity = 10.0f;
    public float HookSpeed = 60.0f;
    public float ClimbSpeed = 1.0f;
	public float ClimbingBrakeSpeedModifier = 0.5f;
	public float animationSmoothTime = 0.3F;
    public GameObject GrappleArmEnd;
    public delegate void OnPlayerDiedEvent();
    public event OnPlayerDiedEvent OnPlayerDied;
    public delegate void OnPlayerWonEvent();
    public event OnPlayerWonEvent OnPlayerWon;

	private Vector3 velocity = Vector3.zero;
	private RaycastHit _playerRaycastOut;
	private RaycastHit _nextPlayerRaycastOut;
	private GameObject _wallHookGraphic;
	private GameObject _wallHook;
	private FixedJoint _wallHookFixedJoint;
	private Vector3 _playerStartPosition;
	private GameObject _playerBody; 
	private Rigidbody _playerRigidbody;
	private AudioSource _playerAudio;
	private GameObject _grappleShoulder;	
	private LineRenderer _ropeLineRenderer;
	private float _ropeMinLength;
	private AudioClip _hookHitSoundEffect;
    private AudioClip _hookFireSoundEffect;
    private List<float> _ropeBendAngles = new List<float>();
    private Vector3 _wallHookHitPosition = new Vector3();
	private bool _grounded = false;
    private bool _hookActive = false;
    private bool _hooked = false;
    private bool _hookShooting = false;
    private Plane _gameSurfacePlane = new Plane(Vector3.back, Vector3.zero);

    void Awake()
    {
        _wallHookGraphic = GameObject.Find("WallHook").gameObject;
        _wallHook = new GameObject();
        _wallHook.name = "WallHookFixedJoint";
        _wallHookFixedJoint = _wallHook.AddComponent<FixedJoint>();
        _wallHook.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionX |
                                                          RigidbodyConstraints.FreezePositionY |
                                                          RigidbodyConstraints.FreezePositionZ |
                                                          RigidbodyConstraints.FreezeRotationX |
                                                          RigidbodyConstraints.FreezeRotationY;
		_playerStartPosition = transform.position;
		_playerBody = transform.Find("PlayerBody").gameObject;
		_playerRigidbody = GetComponent<Rigidbody>();
		_playerAudio = GetComponent<AudioSource>();
		_grappleShoulder = _playerBody.transform.Find("GrappleShoulder").gameObject;
        _ropeLineRenderer = _wallHookGraphic.GetComponent<LineRenderer>();
		_ropeMinLength = (_grappleShoulder.transform.position - _wallHookGraphic.transform.position).magnitude * 2;
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
    	//Debug.Log("GROUNDED: " + _grounded + "     HOOKED: " + _hooked + "     HOOKACTIVE: " + _hookActive);
		Debug.DrawRay(transform.position, _playerRigidbody.velocity * _playerRigidbody.velocity.magnitude * 0.05f, Color.yellow);

		HandleBodyRotation();

		// show linerenderer if active
        if((_hooked || _hookActive) && !_ropeLineRenderer.enabled)
        	_ropeLineRenderer.enabled = true;

		if (HookPlayerInput.HookButtonDown() && !_hookActive)
        {
            if (!_hooked)
            {
                if (CheckHookHit())
                {
					_hookShooting = !_hookShooting;
					StartCoroutine(MoveHook(_wallHookGraphic.transform.position, _wallHookHitPosition, _hookShooting));
                }
            }
            else
            {
            	if(!_grounded)
                	BoostPlayer();
				_hookShooting = !_hookShooting;
				StartCoroutine(MoveHook(_ropeLineRenderer.GetPosition(0), _ropeLineRenderer.GetPosition(1), _hookShooting));
            }
        }
		else if(HookPlayerInput.ClimbButtonPressed())
        {
			if (_hooked)
        		ClimbRope();
        }
		else if (HookPlayerInput.ClimbButtonUp() && _hooked)
		{
			// brake player a little whe done climbing
			float brakeSpeed = _playerRigidbody.velocity.magnitude * ClimbingBrakeSpeedModifier;
            Vector3 normalisedVelocity = _playerRigidbody.velocity.normalized;
            Vector3 brakeVelocity = normalisedVelocity * brakeSpeed;
            _playerRigidbody.AddForce(-brakeVelocity, ForceMode.Impulse);
		}
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

        // check if rope hit an edge and handle if true
        if ((_hooked || _hookActive) && _ropeLineRenderer.positionCount > 1)
        {
			bool hit = false;
			Vector3 direction = _ropeLineRenderer.GetPosition(_ropeLineRenderer.positionCount - 2) - transform.position;

			if(_hooked)
                CheckRopeSlack();

          	hit = Physics.Raycast(transform.position, direction, out _playerRaycastOut, direction.magnitude, 1 << LayerMask.NameToLayer("Wall"));
            if (hit)
            {
				hit = Physics.Raycast(_ropeLineRenderer.GetPosition(_ropeLineRenderer.positionCount - 2), -direction, out _nextPlayerRaycastOut, Mathf.Infinity, 1 << LayerMask.NameToLayer("Wall"));
                if (hit)
                {
                    if (_playerRaycastOut.transform.gameObject == _nextPlayerRaycastOut.transform.gameObject)
                    {
						if(_hookActive)
			    			StopAllCoroutines();

						HandleLineWinding(_playerRaycastOut, _nextPlayerRaycastOut);

						if(_hookActive)
			    			RestartMoveHook();
		    		}
	    		}
	    	}
			else if (_ropeLineRenderer.positionCount > 2)
			{
				Vector3 playersAngle = transform.position - _ropeLineRenderer.GetPosition(_ropeLineRenderer.positionCount - 2);
				Vector3 previousAngle = _ropeLineRenderer.GetPosition(_ropeLineRenderer.positionCount - 2) - _ropeLineRenderer.GetPosition(_ropeLineRenderer.positionCount - 3);
                float currentAngle = AngleFromAToB(playersAngle, previousAngle);

		        if (Mathf.Sign(currentAngle) != Mathf.Sign(_ropeBendAngles[_ropeBendAngles.Count - 1]))
		        {
					if(_hookActive)
		    			StopAllCoroutines();

					HandleLineUnwinding();

					if(_hookActive)
		    			RestartMoveHook();
		    	}
			}

			_ropeLineRenderer.SetPosition(_ropeLineRenderer.positionCount - 1, _grappleShoulder.transform.position);
        }

        HandleMove();
    }

    void LateUpdate()
    {
		Quaternion grappleShoulderRotation = new Quaternion();

        // adjust playerBody for parents rotation
        _playerBody.transform.rotation = Quaternion.Euler(transform.rotation.x, transform.rotation.y, -transform.rotation.z);

        // adjust arm rotation
        if (_hooked)
        {
			grappleShoulderRotation = Quaternion.LookRotation(_ropeLineRenderer.GetPosition(_ropeLineRenderer.positionCount - 2) - _ropeLineRenderer.GetPosition(_ropeLineRenderer.positionCount - 1), Vector3.back);
		}
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

    IEnumerator MoveHook(Vector3 startPosition, Vector3 destination, bool ropeShooting)
    {
		_hooked = false;
        _hookActive = true;
        _playerAudio.PlayOneShot(_hookFireSoundEffect);
		_wallHookFixedJoint.connectedBody = null;
        _wallHookGraphic.transform.parent = null;
		var dist = Vector3.Distance(startPosition, destination);
		float timePassed = 0;
        float timeTakenDuringLerp = dist / HookSpeed;

        if(_ropeLineRenderer.positionCount == 0)
        	_ropeLineRenderer.positionCount = 2;

        while (timePassed < timeTakenDuringLerp)
        {
			if (_ropeLineRenderer.positionCount == 2 && !ropeShooting)
				destination = GrappleArmEnd.transform.position;

            float percentageComplete = timePassed / timeTakenDuringLerp;
			_wallHookGraphic.transform.position = Vector3.Lerp(startPosition, destination, percentageComplete);
			_ropeLineRenderer.SetPosition(0, _wallHookGraphic.transform.position);

            timePassed += Time.deltaTime;
 
            yield return null;
        }

		if(ropeShooting)
        {
			_ropeLineRenderer.SetPosition(0, destination);
	        _wallHook.transform.position = destination;
			_wallHookGraphic.transform.position = destination;
			_wallHookFixedJoint.transform.position = _ropeLineRenderer.GetPosition(_ropeLineRenderer.positionCount - 2);
			if(!_grounded)
	        	_wallHookFixedJoint.connectedBody = _playerRigidbody;
			_hooked = true;
        }
        else
        {
			ShiftRopeArrayLeft();
			if (_ropeLineRenderer.positionCount > 1)
				yield return StartCoroutine(MoveHook(_ropeLineRenderer.GetPosition(0), _ropeLineRenderer.GetPosition(1), false));
			_ropeLineRenderer.enabled = false;
			_ropeBendAngles.Clear();
			_ropeLineRenderer.positionCount = 0;
        	_wallHookGraphic.transform.position = GrappleArmEnd.transform.position;
        	_wallHookGraphic.transform.parent = GrappleArmEnd.transform;
			_hooked = false;
        }

        _playerAudio.PlayOneShot(_hookHitSoundEffect);
        _hookActive = false;
    }

    void RestartMoveHook()
    {
		if(_hookShooting)
			StartCoroutine(MoveHook(_wallHookGraphic.transform.position, _wallHookHitPosition, _hookShooting));
		else
			StartCoroutine(MoveHook(_ropeLineRenderer.GetPosition(0), _ropeLineRenderer.GetPosition(1), _hookShooting));
    }

	void HandleLineWinding(RaycastHit playerRaycastOut, RaycastHit nextPlayerRaycastOut)
	{
        Vector3 cornerNormal = playerRaycastOut.normal + nextPlayerRaycastOut.normal;
        float modifier = Mathf.Sign(AngleFromAToB(playerRaycastOut.normal, cornerNormal));

        Vector3 pointDirection1 = (Quaternion.Euler(0, 0, modifier * -45) * cornerNormal) * 100.0f;
        Vector3 pointDirection2 = (Quaternion.Euler(0, 0, modifier * 45) * cornerNormal) * 100.0f;

        try
        {
            Vector2 intersection2D = Math3d.LineIntersectionPoint(nextPlayerRaycastOut.point, nextPlayerRaycastOut.point + pointDirection1 * 10.0f, playerRaycastOut.point, playerRaycastOut.point + pointDirection2 * 10.0f);
            Vector3 intersection = new Vector3(intersection2D.x, intersection2D.y, 0.0f);
            intersection = intersection + (cornerNormal.normalized * 0.1f);
            _ropeLineRenderer.positionCount = _ropeLineRenderer.positionCount + 1;
            _ropeLineRenderer.SetPosition(_ropeLineRenderer.positionCount - 2, intersection);

            if(_hooked)
            {
				_wallHook.GetComponent<FixedJoint>().connectedBody = null;
                _wallHook.transform.position = intersection;
                if(!_grounded)
                	_wallHookFixedJoint.connectedBody = _playerRigidbody;
            }

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

	void HandleLineUnwinding()
	{
		if(_hooked)
		{
	        _wallHook.GetComponent<FixedJoint>().connectedBody = null;
	        _wallHook.transform.position = _ropeLineRenderer.GetPosition(_ropeLineRenderer.positionCount - 3);
	        if(!_grounded)
	        	_wallHookFixedJoint.connectedBody = _playerRigidbody;
	    }
		_ropeLineRenderer.positionCount = _ropeLineRenderer.positionCount - 1;
	    _ropeBendAngles.RemoveAt(_ropeBendAngles.Count - 1);
	}

	void HandleMove()
    {
    	// moves player left and right
        if ((HookPlayerInput.Move() < 0 || HookPlayerInput.Move() > 0) && (_grounded))
            GetComponent<Rigidbody>().velocity = new Vector2(HookPlayerInput.Move() * Speed, GetComponent<Rigidbody>().velocity.y);
    }

    void ClimbRope()
    {
		Vector3 direction;
		float currentRopeLength = (_ropeLineRenderer.GetPosition(_ropeLineRenderer.positionCount - 2) - _ropeLineRenderer.GetPosition(_ropeLineRenderer.positionCount - 1)).magnitude;

		if(currentRopeLength > _ropeMinLength)
		{
	        _wallHookFixedJoint.connectedBody = null;
			direction = (_ropeLineRenderer.GetPosition(_ropeLineRenderer.positionCount - 2) - _ropeLineRenderer.GetPosition(_ropeLineRenderer.positionCount - 1)).normalized;
			direction = direction * ClimbSpeed / Time.deltaTime;
			_playerRigidbody.AddForce(direction, ForceMode.Acceleration);
        }
        else
			_wallHookFixedJoint.connectedBody = null;
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
        float enter;
        if (_gameSurfacePlane.Raycast(ray, out enter))
        {
            RaycastHit wallHit = new RaycastHit();
            Vector3 wallHookPosition = ray.GetPoint(enter);
            Vector3 origin = new Vector3(_grappleShoulder.transform.position.x, _grappleShoulder.transform.position.y, _grappleShoulder.transform.position.z + -_grappleShoulder.transform.position.z);
            Vector3 direction = wallHookPosition - origin;
            if (Physics.Raycast(origin, direction, out wallHit, Mathf.Infinity, 1 << LayerMask.NameToLayer("Wall")))
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
			float currentRopeLength = (_ropeLineRenderer.GetPosition(_ropeLineRenderer.positionCount - 2) - _ropeLineRenderer.GetPosition(_ropeLineRenderer.positionCount - 1)).magnitude;
            bool playerMovingTowardHook = Math3d.ObjectMovingTowards(_ropeLineRenderer.GetPosition(_ropeLineRenderer.positionCount - 2),
                                                                     transform.position,
                                                                     transform.GetComponent<Rigidbody>().velocity);

			if (playerMovingTowardHook || HookPlayerInput.RopeReleasePressed() || currentRopeLength < _ropeMinLength)
                _wallHookFixedJoint.connectedBody = null;
            else
                _wallHookFixedJoint.connectedBody = _playerRigidbody;
        }
    }

    void HandleBodyRotation()
    {
		if(TestObject != null)
		{
			if(_grounded)
			{
				TestText.text = "GROUNDED";
				TestObject.transform.eulerAngles = new Vector3(TestObject.transform.eulerAngles.x,TestObject.transform.eulerAngles.y, -_playerRigidbody.velocity.x * 2);
			}
			else if(_hooked && _wallHookFixedJoint.connectedBody != null)	
			{	
				TestText.text = "HOOKED";
				TestObject.transform.eulerAngles = new Vector3(TestObject.transform.eulerAngles.x,TestObject.transform.eulerAngles.y, _playerRigidbody.velocity.x * 4);
			}
			else if(_hookActive)
			{
				TestText.text = "HOOK ACTIVE";
				TestObject.transform.eulerAngles = new Vector3(TestObject.transform.eulerAngles.x,TestObject.transform.eulerAngles.y, -_playerRigidbody.velocity.x * 4);
			}
			else if(HookPlayerInput.RopeReleasePressed() || !_grounded)
			{
				TestText.text = "FLOATING";
			}
		}

//		Vector3 targetPosition = target.TransformPoint(new Vector3(0, 5, -10));
//        TestObject.transform.eulerAngles = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
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
//		if (collision.gameObject.layer == LayerMask.NameToLayer("Lava") || collision.gameObject.layer == LayerMask.NameToLayer("Bullet"))
//            StartCoroutine("PlayerDied");

		if(collision.contacts.Length > 0 && collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
		{
			ContactPoint contact = collision.contacts[0];
			if(Vector3.Dot(contact.normal, Vector3.up) > 0.5)
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
    }

    void OnCollisionExit(Collision collision)
    {
		if(collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
		{
			_grounded = false;
            transform.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionZ |
                                                              RigidbodyConstraints.FreezeRotationX |
                                                              RigidbodyConstraints.FreezeRotationY;
            if (_hooked)
                _wallHookFixedJoint.connectedBody = _playerRigidbody;
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

    void ShiftRopeArrayLeft()
    {
		Vector3[] tempLineRendererPositions = new Vector3[_ropeLineRenderer.positionCount];
		Vector3[] tempLineRendererPositionsNew = new Vector3[_ropeLineRenderer.positionCount];
		_ropeLineRenderer.GetPositions(tempLineRendererPositions);
		Array.Copy(tempLineRendererPositions, 1, tempLineRendererPositionsNew, 0, tempLineRendererPositions.Length - 1);
		_ropeLineRenderer.SetPositions(tempLineRendererPositionsNew);
		_ropeLineRenderer.positionCount = _ropeLineRenderer.positionCount - 1;
    }
}