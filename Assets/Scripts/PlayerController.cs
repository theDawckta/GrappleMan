using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour {
	
	public PlayerInput HookPlayerInput;
	public float MaxSpeed = 10.0f;
	public float JumpForce = 900.0f;
    public LayerMask Wall;
    public float hookSpeed = 80.0f;
    public float lineSpeed = 90.0f;
    public float climbSpeed = 30.0f;
    public LayerMask ropeLayerMask;
    public Text debugText;

	private Animator anim;
	private GameObject playerBody;
	private bool grounded = false;
	private GameObject wallHookGraphic;
	private LineRenderer ropeLineRenderer;
    private List<float> ropeBendAngles = new List<float>();
    private List<Vector3> lineRenderPositions =  new List<Vector3>();
    private Vector3 currentLineEndpoint;
    private GameObject wallHook;
    private FixedJoint wallHookFixedJoint;
    private bool hookActive = false;
    private bool wallHookOut = false;
	private bool hooked = false;
	private Vector3 hookPrepStartPosition;
	private Vector3 hookPrepEndPosition;
    private Vector3 playerPreviousPosition;
	void Awake() 
    {
        wallHookGraphic = transform.FindChild("WallHook").gameObject;
        wallHook = new GameObject();
        wallHook.name = "WallHookFixedJoint";
        wallHookFixedJoint = wallHook.AddComponent<FixedJoint>();
        wallHook.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionX |
                                                         RigidbodyConstraints.FreezePositionY |
                                                         RigidbodyConstraints.FreezePositionZ |
                                                         RigidbodyConstraints.FreezeRotationX |
                                                         RigidbodyConstraints.FreezeRotationY;
        ropeLineRenderer = wallHookGraphic.GetComponent<LineRenderer>();
        playerBody = transform.FindChild("PlayerBody").gameObject;
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
        if(hooked)
        {
            RaycastHit playerRaycastOut;
            Vector3 direction = wallHook.transform.position - transform.position;
            bool hit = Physics.Raycast(transform.position, direction, out playerRaycastOut, 1000.0f, 1 << LayerMask.NameToLayer("Ground"));
            Debug.DrawRay(transform.position, direction, Color.red);
            // This is hitting everytime need to fix it up
            if(hit)
            {
                Debug.Log("hit");
                Vector3 modifiedHitPoint = playerRaycastOut.point + (playerRaycastOut.normal.normalized * 0.3f);
                lineRenderPositions.Add(modifiedHitPoint);
                wallHook.GetComponent<FixedJoint>().connectedBody = null;
                currentLineEndpoint = modifiedHitPoint;
                wallHook.transform.position = modifiedHitPoint;
                wallHookFixedJoint.connectedBody = transform.GetComponent<Rigidbody>();

                // store rope bend polarity to check when we swing back
                Vector3 playersAngle = transform.position - lineRenderPositions[lineRenderPositions.Count - 1];
                Vector3 previousAngle = lineRenderPositions[lineRenderPositions.Count - 1] - lineRenderPositions[lineRenderPositions.Count - 2];
                ropeBendAngles.Add(AngleFromAToB(playersAngle, previousAngle));
            }

            if(lineRenderPositions.Count > 1)
            {
                Vector3 playersAngle = transform.position - lineRenderPositions[lineRenderPositions.Count - 1];
                Vector3 previousAngle = lineRenderPositions[lineRenderPositions.Count - 1] - lineRenderPositions[lineRenderPositions.Count - 2];
                float currentAngle = AngleFromAToB(playersAngle, previousAngle);

                if(Mathf.Sign(currentAngle) != Mathf.Sign(ropeBendAngles[ropeBendAngles.Count - 1]))
                {
                    wallHook.GetComponent<FixedJoint>().connectedBody = null;
                    wallHook.transform.position = lineRenderPositions[lineRenderPositions.Count - 2];
                    lineRenderPositions.RemoveAt(lineRenderPositions.Count - 1); 
                    wallHookFixedJoint.connectedBody = transform.GetComponent<Rigidbody>();
                    ropeBendAngles.RemoveAt(ropeBendAngles.Count - 1);
                }
               
                if(ropeBendAngles.Count > 0)
                {
                    debugText.text = "PreviousAngle: " + ropeBendAngles[ropeBendAngles.Count - 1] + "  |  CurrentAngle: " + currentAngle;
                    Debug.Log("PreviousAngle: " + ropeBendAngles[ropeBendAngles.Count - 1] + "  |  CurrentAngle: " + currentAngle);
                }
            }
        }
        HandleMove();
        DoDebugDrawing();
	}

    float AngleFromAToB(Vector3 angleA, Vector3 angleB)
    {
        Vector3 axis = new Vector3(0,0,1);
        float angle = Vector3.Angle(angleA, angleB);
        float sign = Mathf.Sign(Vector3.Dot(axis, Vector3.Cross(angleA, angleB)));

        // angle in [-179,180]
        float signed_angle = angle * sign;
        return signed_angle;
    }
    void LateUpdate()
    {
        // adjust playerBody for parents rotation
        playerBody.transform.rotation = Quaternion.Euler(transform.rotation.x, transform.rotation.y, -transform.rotation.z);

        if(hooked)
		{
			// draw rope if hooked
            ropeLineRenderer.SetVertexCount(lineRenderPositions.Count + 1);
            for (int i = 0; i < lineRenderPositions.Count; i++ )
            {
                ropeLineRenderer.SetPosition(i, lineRenderPositions[i]); 
            }
            ropeLineRenderer.SetPosition(lineRenderPositions.Count, transform.position);
		}
    }
    IEnumerator ShootHook()
    {
        hookActive = true;
        float elapsedTime = 0;
        Vector3 hookEndPoint = Camera.main.ScreenToWorldPoint(new Vector3(HookPlayerInput.GetPlayerTouchPosition().x,
                                                                          HookPlayerInput.GetPlayerTouchPosition().y,
                                                                        -(Camera.main.transform.position.z + transform.position.z)));
        wallHookGraphic.transform.parent = null;
        var dist = Vector3.Distance(wallHookGraphic.transform.position, hookEndPoint);
        float timeTakenDuringLerp = dist / hookSpeed;
        while (elapsedTime < timeTakenDuringLerp)
        {
            float percentageComplete = elapsedTime / timeTakenDuringLerp;
            wallHookGraphic.transform.position = Vector3.Lerp(wallHookGraphic.transform.position,
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
        Vector3 ropeEndPoint = new Vector3();
        var dist = Vector3.Distance(transform.position, wallHookGraphic.transform.position);
        float timeTakenDuringLerp = dist / hookSpeed;
        ropeLineRenderer.SetVertexCount(2);
        while (elapsedTime < timeTakenDuringLerp)
        {
            float percentageComplete = elapsedTime / timeTakenDuringLerp;
            ropeEndPoint = Vector3.Lerp(transform.position, wallHookGraphic.transform.position, percentageComplete);
            ropeLineRenderer.SetPosition(0, transform.position);
            ropeLineRenderer.SetPosition(1, ropeEndPoint);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        lineRenderPositions.Add(ropeEndPoint);
        wallHook.transform.position = ropeEndPoint;
        wallHookFixedJoint.connectedBody = transform.GetComponent<Rigidbody>();
        hooked = true;
        hookActive = false;
    }
    IEnumerator RetrieveHookRope()
    {
        // have to fix the line coming back
        lineRenderPositions.Clear();
        wallHookFixedJoint.connectedBody = null;
        hooked = false;
        wallHookOut = false;
        float elapsedTime = 0;
        var dist = Vector3.Distance(transform.position, wallHookGraphic.transform.position);
        float timeTakenDuringLerp = dist / hookSpeed;
        while (elapsedTime < timeTakenDuringLerp)
        {
            // retrieve rope
            float percentageComplete = elapsedTime / timeTakenDuringLerp;
            Vector3 ropeEndPoint = Vector3.Lerp(wallHookGraphic.transform.position , transform.position, percentageComplete);
            ropeLineRenderer.SetPosition(1, transform.position);
            ropeLineRenderer.SetPosition(0, ropeEndPoint);

            // retrieve hook
            wallHookGraphic.transform.position = Vector3.Lerp(wallHookGraphic.transform.position,
                                                       transform.position,
                                                       percentageComplete);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        ropeLineRenderer.enabled = false;
        wallHookGraphic.transform.parent = transform;
        hookActive = false;
    }
    IEnumerator ClimbRope()
    {
        grounded = false;
        hookActive = true;
        Vector3 startPosition = transform.position;
        float elapsedTime = 0;
        float scale = 0.1f;
        Vector3 midBezierPoint = wallHookGraphic.transform.position - transform.position;
        transform.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionZ |
                                                          RigidbodyConstraints.FreezeRotationX |
                                                          RigidbodyConstraints.FreezeRotationY;
        midBezierPoint.Normalize();
        Vector3 midPoint = (transform.position + (scale * midBezierPoint)) + transform.GetComponent<Rigidbody>().velocity * 0.2f;
        wallHookFixedJoint.connectedBody = null;
        transform.GetComponent<Rigidbody>().isKinematic = true;
        var dist = Vector3.Distance(transform.position, wallHookGraphic.transform.position);
        float timeTakenDuringLerp = dist / climbSpeed;

        while (elapsedTime < timeTakenDuringLerp)
        {
            playerPreviousPosition = transform.position;
            float percentageComplete = elapsedTime / timeTakenDuringLerp;
            transform.position = Vector3.Lerp(Vector3.Lerp(startPosition, midPoint, percentageComplete), Vector3.Lerp(midPoint, wallHookGraphic.transform.localPosition, percentageComplete), percentageComplete);
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
        wallHookGraphic.transform.parent = transform;
		transform.rotation = Quaternion.identity;
		playerBody.gameObject.transform.rotation = Quaternion.identity;
	}
    void CheckRopeSlack()
    {
        if(!grounded)
        {
            bool playerMovingTowardHook = PhysicsHelper.Instance.isMovingTowards(wallHookGraphic.transform.localPosition,
                                                                                 transform.position,
                                                                                 transform.GetComponent<Rigidbody>().velocity);
            if (playerMovingTowardHook)
                wallHookFixedJoint.connectedBody = null;
            else
                wallHookFixedJoint.connectedBody = transform.GetComponent<Rigidbody>();
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
