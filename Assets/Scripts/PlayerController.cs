using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    public Text TestText;

    public GameObject RopeOrigin;
    public GameObject PlayerSprite;
    public GameObject GrappleArmEnd;
    public PlayerInput HookPlayerInput;
    public float Speed = 1.0f;
    public float BoostForce = 8.0f;
    public float SwingBoostForce = 5.0f;
    public float MaxVelocity = 18.0f;
    public float HookSpeed = 130.0f;
    public float ClimbSpeed = 1.0f;
    public float MaxClimbVelocity = 27.0f;
    public float AnimationSmoothTime = 0.3F;
    public delegate void OnPlayerDiedEvent();
    public event OnPlayerDiedEvent OnPlayerDied;
    public delegate void OnPlayerWonEvent();
    public event OnPlayerWonEvent OnPlayerWon;

    private float zVelocity = 0.0f;
    private RaycastHit _playerRaycastOut;
    private RaycastHit _nextPlayerRaycastOut;
    private GameObject _wallHookGraphic;
    private GameObject _wallHook;
    private FixedJoint _wallHookFixedJoint;
    private Vector3 _playerStartPosition;
    private Rigidbody _playerRigidbody;
    private AudioSource _playerAudio;
    private LineRenderer _ropeLineRenderer;
    private float _ropeMinLength;
    private float _currentRopeLength;
    private AudioClip _hookHitSoundEffect;
    private AudioClip _hookFireSoundEffect;
    private List<float> _ropeBendAngles = new List<float>();
    private Vector3 _wallHookHitPosition = new Vector3();
    private float _distToGround;
    private bool _grounded { get{return Physics.Raycast(PlayerSprite.transform.position, Vector3.down, _distToGround + 0.1f);}}           
    private bool _hookActive = false;
    private bool _hooked = false;
    private bool _hookShooting = false;
    private bool _floating = false;

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
		_playerRigidbody = GetComponent<Rigidbody>();
		_playerAudio = GetComponent<AudioSource>();
        _ropeLineRenderer = _wallHookGraphic.GetComponent<LineRenderer>();
		_ropeMinLength = (RopeOrigin.transform.position - _wallHookGraphic.transform.position).magnitude * 2;
        _hookFireSoundEffect = Resources.Load("SoundEffects/GunFire") as AudioClip;
        _hookHitSoundEffect = Resources.Load("SoundEffects/GunHit") as AudioClip;
        _distToGround = PlayerSprite.GetComponent<Collider>().bounds.extents.y;
        HookPlayerInput.InputActive = true;
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
        _floating = false;
        transform.position = _playerStartPosition;
    }

    void Update()
    {
    	// keep this around for debugging
		//Debug.Log("GROUNDED:" + _grounded + "   HOOKED:" + _hooked + "   HOOKACTIVE:" + _hookActive + "   FLOATING:" + _floating + "   VELOCITY:" + _playerRigidbody.velocity);
        if(_hooked)
            _currentRopeLength = (_ropeLineRenderer.GetPosition(_ropeLineRenderer.positionCount - 2) - _ropeLineRenderer.GetPosition(_ropeLineRenderer.positionCount - 1)).magnitude;
		
        HandleBodyRotation();

		if (HookPlayerInput.HookButtonDown() && !_hookActive)
        {
            if (!_hooked)
            {
                CheckHookHit(HookPlayerInput.GetDirection());
            }
            else if(HookPlayerInput.GetDirection() != Vector3.zero)
            {
            	if(!_grounded)
                	BoostPlayer();
				_hookShooting = !_hookShooting;
				StartCoroutine(MoveHook(_ropeLineRenderer.GetPosition(0), _ropeLineRenderer.GetPosition(1), _hookShooting));
            }
        }
        else if (HookPlayerInput.ClimbButtonPressed())
        {
            if (_hooked && _ropeLineRenderer.positionCount > 1 && _currentRopeLength > _ropeMinLength && _playerRigidbody.velocity.magnitude < MaxClimbVelocity)
                ClimbRope();
        }
//        else if (_hooked && !_floating && !_grounded && _playerRigidbody.velocity.y < -1.5f && _currentRopeLength > _ropeMinLength * 2.0f)
//        {
//            Vector3 force = _playerRigidbody.velocity.normalized * SwingBoostForce;
//            _playerRigidbody.AddForce(force, ForceMode.Acceleration);
//        }
    }

    void FixedUpdate()
    {
        // handle _grounded
        if (_grounded)
            transform.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionZ |
                                                              RigidbodyConstraints.FreezeRotationX |
                                                              RigidbodyConstraints.FreezeRotationY |
                                                              RigidbodyConstraints.FreezeRotationZ;
        else
            transform.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionZ |
                                                              RigidbodyConstraints.FreezeRotationX |
                                                              RigidbodyConstraints.FreezeRotationY;

        // check if rope hit an edge and handle if true
        if ((_hooked || _hookActive) && _ropeLineRenderer.positionCount > 1)
        {
			bool hit = false;
			Vector3 direction = _ropeLineRenderer.GetPosition(_ropeLineRenderer.positionCount - 2) - transform.position;

            if (_hooked)
            {
                CheckRopeSlack();
            }

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

			_ropeLineRenderer.SetPosition(_ropeLineRenderer.positionCount - 1, RopeOrigin.transform.position);
        }

        HandleMove();

        // show linerenderer if active
        if ((_hooked || _hookActive) && !_ropeLineRenderer.enabled)
            _ropeLineRenderer.enabled = true;
    }

    void LateUpdate()
    {
		Quaternion grappleShoulderRotation = RopeOrigin.transform.rotation;

        if (_hooked)
			grappleShoulderRotation = Quaternion.LookRotation(_ropeLineRenderer.GetPosition(_ropeLineRenderer.positionCount - 2) - _ropeLineRenderer.GetPosition(_ropeLineRenderer.positionCount - 1), Vector3.back);
		else if (_hookActive)
			grappleShoulderRotation = Quaternion.LookRotation(_wallHookGraphic.transform.position - RopeOrigin.transform.position, Vector3.back);
		else
        {
			// Handle grappleShoulderRotation for !_hooked !_hookActive 
        }
        grappleShoulderRotation.x = 0.0f;
        grappleShoulderRotation.y = 0.0f;
        RopeOrigin.transform.rotation = grappleShoulderRotation;
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
            _wallHookGraphic.transform.SetParent(GrappleArmEnd.transform, true);
            _wallHookGraphic.transform.rotation = Quaternion.identity;
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
        if (_grounded && _playerRigidbody.velocity.magnitude < MaxVelocity)
            _playerRigidbody.AddForce(new Vector3(HookPlayerInput.Move() * Speed, 0.0f,  0.0f), ForceMode.VelocityChange);
    }

    void ClimbRope()
    {
		Vector3 direction;

		if(_hooked)
		{
	        _wallHookFixedJoint.connectedBody = null;
			direction = (_ropeLineRenderer.GetPosition(_ropeLineRenderer.positionCount - 2) - _ropeLineRenderer.GetPosition(_ropeLineRenderer.positionCount - 1)).normalized;
			direction = direction * ClimbSpeed / Time.deltaTime;
			_playerRigidbody.AddForce(direction, ForceMode.Acceleration);
		}
    }

	void BoostPlayer()
    {
        _playerRigidbody.AddForce(HookPlayerInput.GetDirection().normalized * BoostForce, ForceMode.VelocityChange);
    }

    void CheckRopeSlack()
    {
        if (!_grounded && _hooked)
        {
            bool playerMovingTowardHook = Math3d.ObjectMovingTowards(_ropeLineRenderer.GetPosition(_ropeLineRenderer.positionCount - 2),
                                                                     transform.position,
                                                                     transform.GetComponent<Rigidbody>().velocity);
            if (playerMovingTowardHook || HookPlayerInput.RopeReleasePressed())
            {
                _floating = true;
               _wallHookFixedJoint.connectedBody = null;
            }
            else if(!playerMovingTowardHook || HookPlayerInput.RopeReleaseUp())
            {
                _floating = false;
                _wallHookFixedJoint.connectedBody = _playerRigidbody;
            }
        }
        else if(_grounded && _hooked)
            _wallHookFixedJoint.connectedBody = null;
    }

	void CheckHookHit(Vector2 shotDirection)
    {
        RaycastHit wallHit = new RaycastHit();
        Vector3 shotDirectionVector3 = new Vector3(shotDirection.x, shotDirection.y, 0.0f);
		if (Physics.Raycast(RopeOrigin.transform.position, shotDirectionVector3, out wallHit, Mathf.Infinity, 1 << LayerMask.NameToLayer("Wall")))
        {
            _wallHookHitPosition = wallHit.point + wallHit.normal.normalized * 0.1f;
			_hookShooting = !_hookShooting;
			StartCoroutine(MoveHook(_wallHookGraphic.transform.position, _wallHookHitPosition, _hookShooting));
        }
    }

    void HandleBodyRotation()
    {
		if(PlayerSprite != null)
		{
			Vector3 newRotation = PlayerSprite.transform.eulerAngles;
			if(_grounded)
			{
				TestText.text = "GROUNDED";
				newRotation = new Vector3(PlayerSprite.transform.eulerAngles.x,PlayerSprite.transform.eulerAngles.y, -_playerRigidbody.velocity.x * 2);
			}
			else if(_hooked && !_floating)	
			{	
				TestText.text = "HOOKED";
				newRotation = new Vector3(PlayerSprite.transform.eulerAngles.x,PlayerSprite.transform.eulerAngles.y, _playerRigidbody.velocity.x * 3);
			}
			else if(_hookActive)
			{
				TestText.text = "HOOK ACTIVE";
				newRotation = new Vector3(PlayerSprite.transform.eulerAngles.x,PlayerSprite.transform.eulerAngles.y, -_playerRigidbody.velocity.x * 3);
			}
			else if(_floating || !_grounded)
			{
                TestText.text = "FLOATING";
                if (_playerRigidbody.velocity.y > 0.0f)
                    newRotation = new Vector3(PlayerSprite.transform.eulerAngles.x, PlayerSprite.transform.eulerAngles.y, -_playerRigidbody.velocity.x * 2);
                else
                    newRotation = new Vector3(PlayerSprite.transform.eulerAngles.x, PlayerSprite.transform.eulerAngles.y, _playerRigidbody.velocity.x * 2);
			}

			float zAngle = Mathf.SmoothDampAngle(PlayerSprite.transform.eulerAngles.z, newRotation.z, ref zVelocity, AnimationSmoothTime);
			PlayerSprite.transform.eulerAngles = new Vector3(PlayerSprite.transform.eulerAngles.x,PlayerSprite.transform.eulerAngles.y, zAngle);
		}
    }

    IEnumerator PlayerDied()
    {
        if(OnPlayerDied != null)
            OnPlayerDied();
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
        if (collision.gameObject.layer == LayerMask.NameToLayer("Lava"))
            Init();
    }

    void OnCollisionExit(Collision collision)
    {    
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