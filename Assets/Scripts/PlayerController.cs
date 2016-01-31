using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour {
	
	public PlayerInput HookPlayerInput;
	public float MaxSpeed = 10.0f;
	public float JumpForce = 900.0f;
    public LayerMask Wall;
    public float HookSpeed = 80.0f;
    public float LineSpeed = 90.0f;
    public float ClimbSpeed = 30.0f;
    public float ClimbSlowDownForce = 20.0f;
    public LayerMask RopeLayerMask;
    public Text DebugText;
    public SmoothFollow CameraSmoothFollow;

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
        if(Input.GetKeyDown("-"))
        {
            if (CameraSmoothFollow.distance > 10)
                CameraSmoothFollow.distance = CameraSmoothFollow.distance - 10;
        }

        if (Input.GetKeyDown("="))
        {
            CameraSmoothFollow.distance = CameraSmoothFollow.distance + 10;
        }

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

        if (hooked)
        {
            if (HookPlayerInput.RopeReleasePressed())
            {
                wallHookFixedJoint.connectedBody = null;
            Debug.Log("released");
        }
            if (HookPlayerInput.RopeReleaseReleased())
            {
                wallHookFixedJoint.connectedBody = transform.GetComponent<Rigidbody>();
                Debug.Log("hooked");
            }
        }

        //if (HookPlayerInput.RopeClimbPressed())
        //{
        //    if (hooked)
        //        StartCoroutine("ClimbRope");
        //}

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
            Vector3 direction = lineRenderPositions[lineRenderPositions.Count - 1] - transform.position;
            bool hit = Physics.Raycast(transform.position, direction, out playerRaycastOut, 1 << LayerMask.NameToLayer("Ground"));
            Debug.DrawRay(transform.position, direction, Color.black);
            // This is hitting everytime need to fix it up
            if(hit)
            {
                // figure where to add the wallHook
                Debug.DrawRay(playerRaycastOut.point, playerRaycastOut.normal, Color.blue, 10.0f);
                RaycastHit nextPlayerRaycastOut;
                if (Physics.Raycast(lineRenderPositions[lineRenderPositions.Count - 1], -direction, out nextPlayerRaycastOut, 1 << LayerMask.NameToLayer("Ground")))
                {
                    if(playerRaycastOut.transform.gameObject == nextPlayerRaycastOut.transform.gameObject)
                    {
                        Vector3 cornerNormal = playerRaycastOut.normal + nextPlayerRaycastOut.normal;
                        Debug.DrawRay(playerRaycastOut.point, playerRaycastOut.normal, Color.blue, 10.0f);
                        Debug.DrawRay(nextPlayerRaycastOut.point, nextPlayerRaycastOut.normal, Color.blue, 10.0f);
                        Debug.DrawRay(nextPlayerRaycastOut.point, cornerNormal, Color.red, 10.0f);

                        Debug.DrawRay(nextPlayerRaycastOut.point, cornerNormal, Color.red, 10.0f);
                        //DebugText.text = AngleFromAToB(playerRaycastOut.normal, cornerNormal).ToString();
                        //DebugText.text = AngleFromAToB(cornerNormal, new Vector3(0, 1, 0)).ToString();
                        float modifier = Mathf.Sign(AngleFromAToB(playerRaycastOut.normal, cornerNormal));

                        // Wish I knew a way to make these infinite
                        Vector3 pointDirection1 = (Quaternion.Euler(0, 0, modifier * -45) * cornerNormal) * 1000.0f;
                        Debug.DrawRay(nextPlayerRaycastOut.point, pointDirection1, Color.green);

                        Vector3 pointDirection2 = (Quaternion.Euler(0, 0, modifier * 45) * cornerNormal) * 1000.0f;
                        Debug.DrawRay(playerRaycastOut.point, pointDirection2, Color.green);

                        Vector3 intersection;
                        bool intersecting = Math3d.LineLineIntersection(out intersection, nextPlayerRaycastOut.point, pointDirection1, playerRaycastOut.point, pointDirection2);
                        if(intersecting)
                        {
                            Debug.Log("hit");
                            intersection = intersection + (cornerNormal.normalized * 0.1f);
                            Debug.DrawRay(intersection, cornerNormal, Color.green, 10.0f);
                            lineRenderPositions.Add(intersection);
                            wallHook.GetComponent<FixedJoint>().connectedBody = null;
                            currentLineEndpoint = intersection;
                            wallHook.transform.position = intersection;
                            wallHookFixedJoint.connectedBody = transform.GetComponent<Rigidbody>();

                            // store rope bend polarity to check when we swing back
                            Vector3 playersAngle = transform.position - lineRenderPositions[lineRenderPositions.Count - 1];
                            Vector3 previousAngle = lineRenderPositions[lineRenderPositions.Count - 1] - lineRenderPositions[lineRenderPositions.Count - 2];
                            ropeBendAngles.Add(AngleFromAToB(playersAngle, previousAngle));
                        }
                    }
                }
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
            }
        }
        HandleMove();
        if (HookPlayerInput.RopeClimbPressed())
        {
            wallHookFixedJoint.connectedBody = null;
            Vector3 climbForce = (lineRenderPositions[lineRenderPositions.Count - 1] - transform.position).normalized;
            climbForce = climbForce * ClimbSpeed / Time.deltaTime;
            Debug.Log(climbForce);
            transform.GetComponent<Rigidbody>().AddForce(climbForce, ForceMode.Acceleration);
        }
        else if(HookPlayerInput.RopeClimbReleased())
        {
            Vector3 climbForce = (lineRenderPositions[lineRenderPositions.Count - 1] - transform.position).normalized;
            climbForce = climbForce * ClimbSpeed * ClimbSlowDownForce / Time.deltaTime;
            transform.GetComponent<Rigidbody>().AddForce(-climbForce, ForceMode.Acceleration);
            Debug.Log("released");
        }
        DoDebugDrawing();
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
        float timeTakenDuringLerp = dist / HookSpeed;
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
        float timeTakenDuringLerp = dist / HookSpeed;
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
        //lineRenderPositions.Clear();
        wallHookFixedJoint.connectedBody = null;
        hooked = false;
        wallHookOut = false;
        float elapsedTime = 0;
        var dist = Vector3.Distance(transform.position, wallHookGraphic.transform.position);
        float timeTakenDuringLerp = dist / HookSpeed;
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
        lineRenderPositions.Clear();
        ropeLineRenderer.SetVertexCount(0);
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
        float timeTakenDuringLerp = dist / ClimbSpeed;

        while (elapsedTime < timeTakenDuringLerp)
        {
            playerPreviousPosition = transform.position;
            float percentageComplete = elapsedTime / timeTakenDuringLerp;
            transform.position = Vector3.Lerp(Vector3.Lerp(startPosition, midPoint, percentageComplete), Vector3.Lerp(midPoint, wallHookGraphic.transform.localPosition, percentageComplete), percentageComplete);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        LockPlayerPosition();
        lineRenderPositions.Clear();
        ropeLineRenderer.SetVertexCount(0);
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
            bool playerMovingTowardHook = Math3d.ObjectMovingTowards(lineRenderPositions[lineRenderPositions.Count - 1],
                                                                                 transform.position,
                                                                                 transform.GetComponent<Rigidbody>().velocity);
            if (playerMovingTowardHook)
                wallHookFixedJoint.connectedBody = null;
            else if(!HookPlayerInput.RopeReleasePressed())
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

    float AngleFromAToB(Vector3 angleA, Vector3 angleB)
    {
        Vector3 axis = new Vector3(0, 0, 1);
        float angle = Vector3.Angle(angleA, angleB);
        float sign = Mathf.Sign(Vector3.Dot(axis, Vector3.Cross(angleA, angleB)));

        // angle in [-179,180]
        float signed_angle = angle * sign;
        return signed_angle;
    }

    void DoDebugDrawing()
    {
        if(hooked)
        {

        }
    }

}
