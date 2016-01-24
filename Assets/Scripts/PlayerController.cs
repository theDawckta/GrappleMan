using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {
	
	public PlayerInput HookPlayerInput;
	public float MaxSpeed = 10.0f;
	public float JumpForce = 900.0f;
    public LayerMask Wall;
    public float hookDuration = 0.1f;
    public float climbDuration = 1.0f;

	private Animator anim;
	private GameObject playerBody;
	private bool grounded = false;
	private GameObject wallHook;
    private FixedJoint wallHookFixedJoint;
	private LineRenderer ropeLineRenderer;
	private bool hooked;
	private Vector3 hookPrepStartPosition;
	private Vector3 hookPrepEndPosition;
	void Start() {
        wallHook = transform.FindChild("WallHook").gameObject;
        ropeLineRenderer = wallHook.GetComponent<LineRenderer>();
        playerBody = transform.FindChild("PlayerBody").gameObject;
        wallHookFixedJoint = wallHook.GetComponent<FixedJoint>();
	}
	void Update()
	{
		if(HookPlayerInput.HookPressed())
		{
			if(hooked)
                UnHook();
            else
                PrepHook();
		}

        if (HookPlayerInput.JumpPressed())
        {
            if (hooked)
            {
                UnHook();
                GetComponent<Rigidbody>().AddForce(new Vector2(0, JumpForce));
                return;
            }

            if (grounded)
                GetComponent<Rigidbody>().AddForce(new Vector2(0, JumpForce));
        }

        if(HookPlayerInput.RopeClimbPressed())
        {
            if (hooked)
                PrepClimb();
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
    void PrepClimb()
    {
        ReleaseHook();
        StartCoroutine(ClimbRope(transform.position));
    }
    IEnumerator ClimbRope(Vector3 startPosition)
    {
        float elapsedTime = 0;
        float scale = 0.1f;
        Vector3 midBezierPoint = wallHook.transform.position - transform.position;
        midBezierPoint.Normalize();
        Debug.Log(transform.GetComponent<Rigidbody>().velocity);
        Vector3 midPoint = (transform.position + (scale * midBezierPoint)) + transform.GetComponent<Rigidbody>().velocity * 0.4f;
        transform.GetComponent<Rigidbody>().isKinematic = true;
        while (elapsedTime < climbDuration)
        {
            Debug.DrawRay(transform.position, midPoint);
            float percentageComplete = elapsedTime / climbDuration;
            Vector3 newPlayerPosition = Vector3.Lerp(Vector3.Lerp(startPosition, midPoint, percentageComplete), Vector3.Lerp(midPoint, wallHook.transform.localPosition, percentageComplete), percentageComplete);
            transform.position = newPlayerPosition;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        UnHook();
    }

	void PrepHook()
	{
        transform.GetComponent<Rigidbody>().isKinematic = false;
        // Wall hitting code, just keepin ya around for a bit :)
		//RaycastHit2D wallHit = new RaycastHit2D();
        //Vector3 hookDirection = Camera.main.ScreenToWorldPoint(new Vector3(HookPlayerInput.GetPlayerTouchPosition().x,
        //                                                                   HookPlayerInput.GetPlayerTouchPosition().y,
        //                                                                   -(Camera.main.transform.position.z + transform.position.z)));
		//wallHit = Physics2D.Raycast(transform.position, hookDirection, Mathf.Infinity, wall);
		//wallhook.transform.position = new Vector3(wallHit.point.x, wallHit.point.y, transform.position.z);
        wallHook.transform.position = Camera.main.ScreenToWorldPoint(new Vector3(HookPlayerInput.GetPlayerTouchPosition().x,
                                                                                    HookPlayerInput.GetPlayerTouchPosition().y,
                                                                                    -(Camera.main.transform.position.z + transform.position.z)));
        wallHook.transform.parent = null;
        StartCoroutine(ShootHook());
	}
	IEnumerator ShootHook()
	{
        ropeLineRenderer.enabled = true;
		float elapsedTime = 0;
		while (elapsedTime < hookDuration)
		{
			float percentageComplete = elapsedTime / hookDuration;
            Vector3 ropeEndPoint = Vector3.Lerp(transform.position, wallHook.transform.position, percentageComplete);
            ropeLineRenderer.SetPosition(0, transform.position);
            ropeLineRenderer.SetPosition(1, ropeEndPoint);
			elapsedTime += Time.deltaTime;
			yield return null;
		}
        SetHook();
	}
	void SetHook()
	{
		wallHook.GetComponent<MeshRenderer>().enabled = true;
        wallHookFixedJoint.connectedBody = transform.GetComponent<Rigidbody>();
        hooked = true;
	}
	void UnHook()
	{
		hooked = false;
        ropeLineRenderer.enabled = false;
        wallHook.GetComponent<MeshRenderer>().enabled = false;
        wallHookFixedJoint.connectedBody = null;
        wallHook.transform.parent = transform;
		transform.rotation = Quaternion.identity;
		playerBody.gameObject.transform.rotation = Quaternion.identity;
	}
    void CheckRopeSlack()
    {
        bool playerMovingTowardHook = PhysicsHelper.Instance.isMovingTowards(wallHook.transform.localPosition,
                                                                             transform.position,
                                                                             transform.GetComponent<Rigidbody>().velocity);
        if (playerMovingTowardHook)
            ReleaseHook();
        else
            LockHook();
    }
    void ReleaseHook()
    {
        wallHook.GetComponent<FixedJoint>().connectedBody = null;
    }
    void LockHook()
    {
        wallHook.GetComponent<FixedJoint>().connectedBody = transform.GetComponent<Rigidbody>();
    }
    void HandleMove()
    {
        if ((HookPlayerInput.Move() < 0 || HookPlayerInput.Move() > 0) && !hooked)
            GetComponent<Rigidbody>().velocity = new Vector2(HookPlayerInput.Move() * MaxSpeed, GetComponent<Rigidbody>().velocity.y);
    }
	void OnCollisionEnter(Collision collision) 
    {
        if (collision.gameObject.name == "MapGenerator")
        {
            grounded = true;
            if(hooked)
            { 
                UnHook();
            }
        }
	}
	void OnCollisionExit(Collision collision) 
    {
		if(collision.gameObject.name == "MapGenerator")
			grounded = false;
	}
    void DoDebugDrawing()
    {
        if(hooked)
        {

        }
    }

}
