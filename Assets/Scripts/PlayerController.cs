using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {
	
	public PlayerInput HookPlayerInput;
	public float MaxSpeed = 10.0f;
	public float JumpForce = 900.0f;
    public LayerMask Wall;
    public float hookSpeed = 80.0f;
    public float lineSpeed = 90.0f;
    public float climbSpeed = 30.0f;

	private Animator anim;
	private GameObject playerBody;
	private bool grounded = false;
	private GameObject wallHook;
    private FixedJoint wallHookFixedJoint;
	private LineRenderer ropeLineRenderer;
    private bool hookActive = false;
    private bool wallHookOut = false;
	private bool hooked = false;
	private Vector3 hookPrepStartPosition;
	private Vector3 hookPrepEndPosition;
    private Vector3 playerPreviousPosition;
	void Start() {
        wallHook = transform.FindChild("WallHook").gameObject;
        ropeLineRenderer = wallHook.GetComponent<LineRenderer>();
        playerBody = transform.FindChild("PlayerBody").gameObject;
        wallHookFixedJoint = wallHook.GetComponent<FixedJoint>();
	}
	void Update()
	{
		if(HookPlayerInput.RopePressed())
		{
            //if(hooked)
            //    UnHook();
		}

        if (HookPlayerInput.JumpPressed())
        {
            if (hooked)
            {
                if(!grounded)
                {   
                    StartCoroutine(RetrieveHookRope());
                }
                GetComponent<Rigidbody>().AddForce(new Vector2(0, JumpForce));
                return;
            }

            if (grounded || transform.GetComponent<Rigidbody>().isKinematic)
            {
                transform.GetComponent<Rigidbody>().isKinematic = false;
                GetComponent<Rigidbody>().AddForce(new Vector2(0, JumpForce));
            }
        }

        if(HookPlayerInput.RopeReleasePressed())
        {
            wallHookFixedJoint.connectedAnchor -= wallHookFixedJoint.connectedAnchor.normalized;
        }

        if(HookPlayerInput.RopeClimbPressed())
        {
            if (hooked)
                StartCoroutine("ClimbRope");
        }

        if (HookPlayerInput.HookPressed())
        {
            transform.GetComponent<Rigidbody>().isKinematic = false;
            if (!hookActive)
            {
                if (!wallHookOut && !hooked)
                {
                    StartCoroutine(ShootHook());
                }
                else if (wallHookOut && !hooked)
                {
                    StartCoroutine(ShootRope());
                }
                else if(wallHookOut && hooked)
                {
                    StartCoroutine(RetrieveHookRope());
                }
            } 
            else if(wallHookOut && hooked && hookActive)
            {
                StopCoroutine("ClimbRope");
                Debug.Log("stopping corroutine");
               
                StartCoroutine(RetrieveHookRope()); 
                Vector3 playerVelocity = (transform.position - playerPreviousPosition) / Time.deltaTime;
                transform.GetComponent<Rigidbody>().velocity = playerVelocity;
            }
        }
	}
	void FixedUpdate() 
	{
        if(hooked)
            CheckRopeSlack();

        HandleMove();
        DoDebugDrawing();
	}
    void LateUpdate()
    {
        // adjust playerBody for parents rotation
        playerBody.transform.rotation = Quaternion.Euler(transform.rotation.x, transform.rotation.y, -transform.rotation.z);

        if(hooked)
		{
			// draw rope if hooked
            ropeLineRenderer.SetPosition(0, wallHook.transform.position);
            ropeLineRenderer.SetPosition(1, transform.position);
		}
    }
    IEnumerator ShootHook()
    {
        hookActive = true;
        float elapsedTime = 0;
        Vector3 hookEndPoint = Camera.main.ScreenToWorldPoint(new Vector3(HookPlayerInput.GetPlayerTouchPosition().x,
                                                                          HookPlayerInput.GetPlayerTouchPosition().y,
                                                                        -(Camera.main.transform.position.z + transform.position.z)));
        wallHook.transform.parent = null;
        var dist = Vector3.Distance(wallHook.transform.position, hookEndPoint);
        float timeTakenDuringLerp = dist / hookSpeed;
        while (elapsedTime < timeTakenDuringLerp)
        {
            float percentageComplete = elapsedTime / timeTakenDuringLerp;
            wallHook.transform.position = Vector3.Lerp(wallHook.transform.position,
                                                       hookEndPoint, 
                                                       percentageComplete);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        wallHookOut = true;
        hookActive = false;
    }
    IEnumerator ShootRope()
    {
        hookActive = true;
        // Wall hitting code, just keepin ya around for a bit :)
        //RaycastHit2D wallHit = new RaycastHit2D();
        //Vector3 hookDirection = Camera.main.ScreenToWorldPoint(new Vector3(HookPlayerInput.GetPlayerTouchPosition().x,
        //                                                                   HookPlayerInput.GetPlayerTouchPosition().y,
        //                                                                   -(Camera.main.transform.position.z + transform.position.z)));
        //wallHit = Physics2D.Raycast(transform.position, hookDirection, Mathf.Infinity, wall);
        //wallhook.transform.position = new Vector3(wallHit.point.x, wallHit.point.y, transform.position.z);


        //wallHook.transform.position = Camera.main.ScreenToWorldPoint(new Vector3(HookPlayerInput.GetPlayerTouchPosition().x,
        //                                                                            HookPlayerInput.GetPlayerTouchPosition().y,
        //   
        ropeLineRenderer.enabled = true;
        float elapsedTime = 0;
        var dist = Vector3.Distance(transform.position, wallHook.transform.position);
        float timeTakenDuringLerp = dist / hookSpeed;
        while (elapsedTime < timeTakenDuringLerp)
        {
            float percentageComplete = elapsedTime / timeTakenDuringLerp;
            Vector3 ropeEndPoint = Vector3.Lerp(transform.position, wallHook.transform.position, percentageComplete);
            ropeLineRenderer.SetPosition(0, transform.position);
            ropeLineRenderer.SetPosition(1, ropeEndPoint);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        hooked = true;
        wallHook.transform.parent = null;
        wallHookFixedJoint.connectedBody = transform.GetComponent<Rigidbody>();
        hookActive = false;
    }
    IEnumerator RetrieveHookRope()
    {
        wallHookFixedJoint.connectedBody = null;
        hooked = false;
        wallHookOut = false;
        float elapsedTime = 0;
        var dist = Vector3.Distance(transform.position, wallHook.transform.position);
        float timeTakenDuringLerp = dist / hookSpeed;
        while (elapsedTime < timeTakenDuringLerp)
        {
            // retrieve rope
            float percentageComplete = elapsedTime / timeTakenDuringLerp;
            Vector3 ropeEndPoint = Vector3.Lerp(wallHook.transform.position , transform.position, percentageComplete);
            ropeLineRenderer.SetPosition(1, transform.position);
            ropeLineRenderer.SetPosition(0, ropeEndPoint);

            // retrieve hook
            wallHook.transform.position = Vector3.Lerp(wallHook.transform.position,
                                                       transform.position,
                                                       percentageComplete);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        ropeLineRenderer.enabled = false;
        wallHook.transform.parent = transform;
        hookActive = false;
    }
    IEnumerator ClimbRope()
    {
        grounded = false;
        hookActive = true;
        Vector3 startPosition = transform.position;
        float elapsedTime = 0;
        float scale = 0.1f;
        Vector3 midBezierPoint = wallHook.transform.position - transform.position;
        transform.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionZ |
                                                          RigidbodyConstraints.FreezeRotationX |
                                                          RigidbodyConstraints.FreezeRotationY;
        midBezierPoint.Normalize();
        Vector3 midPoint = (transform.position + (scale * midBezierPoint)) + transform.GetComponent<Rigidbody>().velocity * 0.2f;
        wallHook.GetComponent<FixedJoint>().connectedBody = null;
        transform.GetComponent<Rigidbody>().isKinematic = true;
        var dist = Vector3.Distance(transform.position, wallHook.transform.position);
        float timeTakenDuringLerp = dist / climbSpeed;

        while (elapsedTime < timeTakenDuringLerp)
        {
            playerPreviousPosition = transform.position;
            float percentageComplete = elapsedTime / timeTakenDuringLerp;
            transform.position = Vector3.Lerp(Vector3.Lerp(startPosition, midPoint, percentageComplete), Vector3.Lerp(midPoint, wallHook.transform.localPosition, percentageComplete), percentageComplete);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        LockPlayerPosition();
        hookActive = false;
    }
	void LockPlayerPosition()
	{
        hooked = false;
        wallHookOut = false;
        transform.GetComponent<Rigidbody>().isKinematic = true;
        ropeLineRenderer.enabled = false;
        wallHookFixedJoint.connectedBody = null;
        wallHook.transform.parent = transform;
		transform.rotation = Quaternion.identity;
		playerBody.gameObject.transform.rotation = Quaternion.identity;
	}
    void CheckRopeSlack()
    {
        if(!grounded)
        {
            bool playerMovingTowardHook = PhysicsHelper.Instance.isMovingTowards(wallHook.transform.localPosition,
                                                                                 transform.position,
                                                                                 transform.GetComponent<Rigidbody>().velocity);
            if (playerMovingTowardHook)
                wallHook.GetComponent<FixedJoint>().connectedBody = null;
            else
                wallHook.GetComponent<FixedJoint>().connectedBody = transform.GetComponent<Rigidbody>();
        }
    }
    void HandleMove()
    {
        if ((HookPlayerInput.Move() < 0 || HookPlayerInput.Move() > 0) && grounded)
            GetComponent<Rigidbody>().velocity = new Vector2(HookPlayerInput.Move() * MaxSpeed, GetComponent<Rigidbody>().velocity.y);
    }
	void OnCollisionStay(Collision collision) 
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            grounded = true;
            transform.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionZ |
                                                              RigidbodyConstraints.FreezeRotationX |
                                                              RigidbodyConstraints.FreezeRotationY |
                                                              RigidbodyConstraints.FreezeRotationZ;
            if(hooked)
            {
                wallHookFixedJoint.connectedBody = null;
            }
        }
	}
	void OnCollisionExit(Collision collision) 
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        { 
			grounded = false;
            transform.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionZ |
                                                              RigidbodyConstraints.FreezeRotationX |
                                                              RigidbodyConstraints.FreezeRotationY;
            if (hooked)
            {
                wallHookFixedJoint.connectedBody = transform.GetComponent<Rigidbody>();
            }
        }
	}
    void DoDebugDrawing()
    {
        if(hooked)
        {

        }
    }

}
