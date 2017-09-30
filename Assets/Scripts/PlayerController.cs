using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    public PlayerInput HookPlayerInput;
    public float MaxSpeed = 10.0f;
    public float JumpForce = 900.0f;
    public float BoostForce = 5.0f;
    public float HookSpeed = 80.0f;
    public float LineSpeed = 90.0f;
    public float ClimbSpeed = 30.0f;
    public float ClimbSlowDownForce = 20.0f;
    public GameObject GrappleArmEnd;
    public LayerMask RopeLayerMask;
    public Text DebugText;
    [HideInInspector]
    public bool playerStarted;
    public delegate void OnPlayerStartedEvent();
    public event OnPlayerStartedEvent OnPlayerStarted;
    public delegate void OnPlayerDiedEvent();
    public event OnPlayerDiedEvent OnPlayerDied;
    public delegate void OnPlayerWonEvent();
    public event OnPlayerWonEvent OnPlayerWon;

    private Vector3 playerStartPosition;
    private Animator anim;
    private GameObject playerBody;
    private GameObject grappleShoulder;
    private Rigidbody playerRigidbody;
    private bool grounded = false;
    private GameObject wallHookGraphic;
    private LineRenderer ropeLineRenderer;
    private List<float> ropeBendAngles = new List<float>();
    private List<Vector3> lineRenderPositions = new List<Vector3>();
    private GameObject wallHook;
    private FixedJoint wallHookFixedJoint;
    private Vector3 wallHookHitPosition = new Vector3();
    private bool hookActive = false;
    private bool hooked = false;
    private Vector3 hookPrepStartPosition;
    private Vector3 hookPrepEndPosition;
    private Vector3 playerPreviousPosition;
    private AudioSource playerAudio;
    private AudioClip HookHitSoundEffect;
    private AudioClip HookFireSoundEffect;
    private float MuzzleFlashRange;

    void Awake()
    {
        playerStartPosition = transform.position;
        wallHookGraphic = GameObject.Find("WallHook").gameObject;
        wallHook = new GameObject();
        wallHook.name = "WallHookFixedJoint";
        wallHookFixedJoint = wallHook.AddComponent<FixedJoint>();
        wallHook.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionX |
                                                         RigidbodyConstraints.FreezePositionY |
                                                         RigidbodyConstraints.FreezePositionZ |
                                                         RigidbodyConstraints.FreezeRotationX |
                                                         RigidbodyConstraints.FreezeRotationY;
        ropeLineRenderer = wallHookGraphic.GetComponent<LineRenderer>();
        playerBody = transform.Find("PlayerBody").gameObject;
        playerRigidbody = GetComponent<Rigidbody>();
        grappleShoulder = playerBody.transform.Find("GrappleShoulder").gameObject;
        playerAudio = GetComponent<AudioSource>();
        HookFireSoundEffect = Resources.Load("SoundEffects/GunFire") as AudioClip;
        HookHitSoundEffect = Resources.Load("SoundEffects/GunHit") as AudioClip;
    }

    public void Init()
    {
        StopAllCoroutines();
        ropeLineRenderer.enabled = false;
        wallHookGraphic.transform.position = GrappleArmEnd.transform.position;
        wallHookGraphic.transform.parent = GrappleArmEnd.transform;
        wallHookFixedJoint.connectedBody = null;
        playerRigidbody.isKinematic = true;
        transform.GetComponent<Rigidbody>().velocity = new Vector3(0.0f, 0.0f, 0.0f);
        ropeBendAngles.Clear();
        lineRenderPositions.Clear();
        hookActive = false;
        hooked = false;
        transform.position = playerStartPosition;
    }

    void Update()
    {
        if (hooked || hookActive)
        {
            if (HookPlayerInput.RopeReleasePressed())
            {
                wallHookFixedJoint.connectedBody = null;
            }
            if (HookPlayerInput.RopeReleaseReleased())
            {
                wallHookFixedJoint.connectedBody = transform.GetComponent<Rigidbody>();
            }
            if (lineRenderPositions.Count > 0)
            {
                Quaternion newRotation = Quaternion.LookRotation(lineRenderPositions[lineRenderPositions.Count - 1] - grappleShoulder.transform.position, Vector3.back);
                newRotation.x = 0.0f;
                newRotation.y = 0.0f;
                grappleShoulder.transform.rotation = newRotation;
            }
        }
        else
        {
            //Vector3 origin = new Vector3(grappleShoulder.transform.position.x, grappleShoulder.transform.position.y, grappleShoulder.transform.position.z + -grappleShoulder.transform.position.z);
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(new Vector3(HookPlayerInput.GetPlayerTouchPosition().x,
                                                                           HookPlayerInput.GetPlayerTouchPosition().y,
                                                                           -(Camera.main.transform.position.z + transform.position.z) + 1));
            Quaternion newRotation = Quaternion.LookRotation(mousePosition - grappleShoulder.transform.position, Vector3.back);
            newRotation.x = 0.0f;
            newRotation.y = 0.0f;
            grappleShoulder.transform.rotation = newRotation;
        }

        if (HookPlayerInput.HookPressed())
        {
            transform.GetComponent<Rigidbody>().isKinematic = false;
            if (!hookActive && !hooked)
            {
                if (playerStarted == false)
                {
                    playerStarted = true;
                    grounded = false;
                    transform.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionZ |
                                                                        RigidbodyConstraints.FreezeRotationX |
                                                                        RigidbodyConstraints.FreezeRotationY;
                    playerRigidbody.isKinematic = false;
                    if(OnPlayerStarted != null)
                        OnPlayerStarted();
                }

                if (CheckHookHit())
                {
                    StartCoroutine(ShootHook(wallHookHitPosition));
                    StartCoroutine(ShootRope(wallHookHitPosition));
                }
            }
            else if (!hookActive && hooked)
            {
                BoostPlayer();
                StartCoroutine(RetrieveHookRope());
            }
        }
    }

    void FixedUpdate()
    {
        if (hooked && lineRenderPositions.Count > 0)
        {
            CheckRopeSlack();
            RaycastHit playerRaycastOut;
            Vector3 direction = lineRenderPositions[lineRenderPositions.Count - 1] - transform.position;
            bool hit = Physics.Raycast(transform.position, direction, out playerRaycastOut, direction.magnitude, 1 << LayerMask.NameToLayer("Ground"));

            if (hit)
            {
                RaycastHit nextPlayerRaycastOut;
                if (Physics.Raycast(lineRenderPositions[lineRenderPositions.Count - 1], -direction, out nextPlayerRaycastOut, Mathf.Infinity, 1 << LayerMask.NameToLayer("Ground")))
                {
                    Debug.DrawRay(lineRenderPositions[lineRenderPositions.Count - 1], -direction, Color.yellow);
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
                            lineRenderPositions.Add(intersection);
                            wallHook.GetComponent<FixedJoint>().connectedBody = null;
                            wallHook.transform.position = intersection;
                            wallHookFixedJoint.connectedBody = transform.GetComponent<Rigidbody>();

                            // store rope bend polarity to check when we swing back
                            Vector3 playersAngle = transform.position - lineRenderPositions[lineRenderPositions.Count - 1];
                            Vector3 previousAngle = lineRenderPositions[lineRenderPositions.Count - 1] - lineRenderPositions[lineRenderPositions.Count - 2];
                            ropeBendAngles.Add(AngleFromAToB(playersAngle, previousAngle));
                        }
                        catch
                        {
                            // Lines were parallel need to implement logic
                            Debug.Log("Lines were parallel doing nothing");
                        }
                    }
                }
            }

            if (lineRenderPositions.Count > 1)
            {
                Vector3 playersAngle = transform.position - lineRenderPositions[lineRenderPositions.Count - 1];
                Vector3 previousAngle = lineRenderPositions[lineRenderPositions.Count - 1] - lineRenderPositions[lineRenderPositions.Count - 2];
                float currentAngle = AngleFromAToB(playersAngle, previousAngle);

                if (Mathf.Sign(currentAngle) != Mathf.Sign(ropeBendAngles[ropeBendAngles.Count - 1]))
                {
                    wallHook.GetComponent<FixedJoint>().connectedBody = null;
                    wallHook.transform.position = lineRenderPositions[lineRenderPositions.Count - 2];
                    lineRenderPositions.RemoveAt(lineRenderPositions.Count - 1);
                    wallHookFixedJoint.connectedBody = transform.GetComponent<Rigidbody>();
                    ropeBendAngles.RemoveAt(ropeBendAngles.Count - 1);
                }
            }
        }
        //HandleMove();

        DoDebugDrawing();
    }

    void LateUpdate()
    {
        // adjust playerBody for parents rotation
        playerBody.transform.rotation = Quaternion.Euler(transform.rotation.x, transform.rotation.y, -transform.rotation.z);

        if (lineRenderPositions.Count > 0)
        {
            // draw rope if hooked
            ropeLineRenderer.positionCount = lineRenderPositions.Count + 1;
            for (int i = 0; i < lineRenderPositions.Count; i++)
            {
                ropeLineRenderer.SetPosition(i, lineRenderPositions[i]);
            }
            ropeLineRenderer.SetPosition(lineRenderPositions.Count, grappleShoulder.transform.position);
        }
    }

    IEnumerator ShootHook(Vector3 location)
    {
        hookActive = true;
        playerAudio.PlayOneShot(HookFireSoundEffect);
        float timePassed = 0;
        // This is code for sending hook out in mid air, just keeping it around
        //Vector3 hookEndPoint = Camera.main.ScreenToWorldPoint(new Vector3(HookPlayerInput.GetPlayerTouchPosition().x,
        //                                                                  HookPlayerInput.GetPlayerTouchPosition().y,
        //                                                                -(Camera.main.transform.position.z + transform.position.z)));

        wallHookGraphic.transform.parent = null;
        wallHookGraphic.transform.position = new Vector3(wallHookGraphic.transform.position.x, wallHookGraphic.transform.position.y, wallHookGraphic.transform.position.z + -wallHookGraphic.transform.position.z);
        var dist = Vector3.Distance(wallHookGraphic.transform.position, location);
        float timeTakenDuringLerp = dist / HookSpeed;
        while (timePassed < timeTakenDuringLerp)
        {
            float percentageComplete = timePassed / timeTakenDuringLerp;
            wallHookGraphic.transform.position = Vector3.Lerp(wallHookGraphic.transform.position,
                                                        location,
                                                        percentageComplete);
            timePassed += Time.deltaTime;
            yield return null;
        }
        hooked = true;
        hookActive = false;
    }

    IEnumerator ShootRope(Vector3 location)
    {
        hookActive = true;
        ropeLineRenderer.enabled = true;
        float elapsedTime = 0;
        Vector3 ropeEndPoint = new Vector3();
        Vector3 origin = new Vector3(grappleShoulder.transform.position.x, grappleShoulder.transform.position.y, grappleShoulder.transform.position.z);
        var dist = Vector3.Distance(origin, wallHookHitPosition);
        float timeTakenDuringLerp = dist / HookSpeed;
        ropeLineRenderer.positionCount = 2;
        while (elapsedTime < timeTakenDuringLerp)
        {
            float percentageComplete = elapsedTime / timeTakenDuringLerp;
            ropeEndPoint = Vector3.Lerp(origin, location, percentageComplete);
            ropeLineRenderer.SetPosition(0, grappleShoulder.transform.position);
            ropeLineRenderer.SetPosition(1, ropeEndPoint);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        lineRenderPositions.Add(wallHookGraphic.transform.position);
        wallHook.transform.position = ropeEndPoint;
        wallHookFixedJoint.connectedBody = transform.GetComponent<Rigidbody>();
        playerAudio.PlayOneShot(HookHitSoundEffect);
        hooked = true;
        hookActive = false;
    }

    IEnumerator RetrieveHookRope()
    {
        // have to fix the line coming back
        hooked = false;
        hookActive = true;
        playerAudio.PlayOneShot(HookHitSoundEffect);
        wallHookFixedJoint.connectedBody = null;
        grounded = true;
        float elapsedTime = 0;
        float dist;
        Vector3 startPosition = new Vector3();
        Vector3 endPosition = new Vector3();
        if (lineRenderPositions.Count > 1)
        {
            dist = Vector3.Distance(lineRenderPositions[0], lineRenderPositions[1]);
            startPosition = lineRenderPositions[0];
            endPosition = lineRenderPositions[1];
        }
        else
        {
            dist = Vector3.Distance(lineRenderPositions[0], GrappleArmEnd.transform.position);
            startPosition = lineRenderPositions[0];
            endPosition = GrappleArmEnd.transform.position;
        }
        float timeTakenDuringLerp = dist / HookSpeed;
        while (elapsedTime < timeTakenDuringLerp)
        {
            // retrieve rope
            float percentageComplete = elapsedTime / timeTakenDuringLerp;
            //Debug.Log("percentage complete: " + percentageComplete + "   elapsed time: " + elapsedTime + "   line render position: " + lineRenderPositions[0] + "time taken: " + timeTakenDuringLerp);
            lineRenderPositions[0] = Vector3.Lerp(startPosition, endPosition, percentageComplete);

            // retrieve hook
            wallHookGraphic.transform.position = lineRenderPositions[0];

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        lineRenderPositions.RemoveAt(0);
        if (lineRenderPositions.Count > 0)
            StartCoroutine(RetrieveHookRope());
        else
        {
            ropeLineRenderer.enabled = false;
            wallHookGraphic.transform.position = GrappleArmEnd.transform.position;
            wallHookGraphic.transform.parent = GrappleArmEnd.transform;
            
        }
        hookActive = false;
    }

    void BoostPlayer()
    {
        Vector3 direction = Camera.main.ScreenToWorldPoint(new Vector3(HookPlayerInput.GetPlayerTouchPosition().x,
                                                                          HookPlayerInput.GetPlayerTouchPosition().y,
                                                                        -(Camera.main.transform.position.z + transform.position.z)));
        direction = direction - transform.position;

        Debug.DrawRay(transform.position, direction.normalized * BoostForce, Color.red, 10.0f);

        playerRigidbody.AddForce(direction.normalized * BoostForce, ForceMode.VelocityChange);
    }

    void LockPlayerPosition()
    {
        hooked = false;
        transform.GetComponent<Rigidbody>().isKinematic = true;
        ropeLineRenderer.enabled = false;
        wallHookFixedJoint.connectedBody = null;
        wallHookGraphic.transform.position = GrappleArmEnd.transform.position;
        wallHookGraphic.transform.parent = GrappleArmEnd.transform;
        transform.rotation = Quaternion.identity;
        playerBody.gameObject.transform.rotation = Quaternion.identity;
    }

    bool CheckHookHit()
    {
        RaycastHit wallHit = new RaycastHit();
        Vector3 wallHookPosition = Camera.main.ScreenToWorldPoint(new Vector3(HookPlayerInput.GetPlayerTouchPosition().x,
                                                                           HookPlayerInput.GetPlayerTouchPosition().y,
                                                                           -(Camera.main.transform.position.z + transform.position.z)));
        Vector3 origin = new Vector3(grappleShoulder.transform.position.x, grappleShoulder.transform.position.y, grappleShoulder.transform.position.z + -grappleShoulder.transform.position.z);
        Vector3 direction = wallHookPosition - origin;
        if (Physics.Raycast(origin, direction, out wallHit, Mathf.Infinity, 1 << LayerMask.NameToLayer("Ground")))
        {
            Debug.DrawLine(wallHit.point, wallHit.point + wallHit.normal.normalized * 0.1f, Color.yellow, 10.0f);
            wallHookHitPosition = wallHit.point + wallHit.normal.normalized * 0.1f;
            return true;
        }
        else
            return false;
    }

    void CheckRopeSlack()
    {
        if (!grounded)
        {
            bool playerMovingTowardHook = Math3d.ObjectMovingTowards(lineRenderPositions[lineRenderPositions.Count - 1],
                                                                                 transform.position,
                                                                                 transform.GetComponent<Rigidbody>().velocity);
            if (playerMovingTowardHook)
                wallHookFixedJoint.connectedBody = null;
            else if (!HookPlayerInput.RopeReleasePressed())
                wallHookFixedJoint.connectedBody = transform.GetComponent<Rigidbody>();
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
        if (collision.gameObject.layer == LayerMask.NameToLayer("Lava") || collision.gameObject.layer == LayerMask.NameToLayer("Ground") || collision.gameObject.layer == LayerMask.NameToLayer("Bullet"))
        {
            playerStarted = false;
            StartCoroutine("PlayerDied");
        }
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
            if (hooked)
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
        if (hooked)
        {

        }
    }

}
