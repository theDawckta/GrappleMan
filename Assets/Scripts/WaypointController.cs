using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointController : MonoBehaviour 
{
    public delegate void OnGatesPassedEvent();
    public event OnGatesPassedEvent OnGatesPassed;

    public delegate void OnWaypointVisibleEvent();
    public event OnWaypointVisibleEvent OnWaypointVisible;

    public delegate void OnWaypointHiddenEvent();
    public event OnWaypointHiddenEvent OnWaypointHidden;

    public delegate void OnGatesFinishedEvent();
    public event OnGatesFinishedEvent OnGatesFinished;

    public GameObject GateCollider;
    public int NumberOfGates = 5;
    public ParticleSystem GateParticles;
    [HideInInspector]
    public Vector3 CurrentWaypointPosition;

    private Collider _gateCollider;
    private LineRenderer _gateLineRenderer;
    private RaycastHit _hit;
    private Vector3 _gateNormal;
    private int _remainingGates;
    private Renderer _gateRenderer;
    private bool _gateVisible = false;

    void Awake()
    {
        _gateCollider = gameObject.GetComponentInChildren<Collider>();
        _gateCollider.enabled = false;
        _gateLineRenderer = gameObject.GetComponent<LineRenderer>();
        _gateLineRenderer.enabled = false;
        _remainingGates = NumberOfGates;
        _gateRenderer = GateCollider.GetComponent<Renderer>();
        _gateRenderer.enabled = false;
    }

    void Update()
    {
        if(_gateRenderer.enabled)
        {
            if (_gateVisible != _gateRenderer.isVisible)
            {
                _gateVisible = _gateRenderer.isVisible;
                if (_gateVisible)
                {
                    Debug.Log("WAYPOINT VISIBLE");
                    if (OnWaypointVisible != null)
                        OnWaypointVisible();
                }
                else
                {
                    Debug.Log("WAYPOINT HIDDEN");
                    if (OnWaypointHidden != null)
                        OnWaypointHidden();
                }
            }
        }
    }

    public void Init(Vector3 startPosition)
    {
        transform.position = startPosition;
        _gateLineRenderer.SetPositions(new Vector3[0]);
        MakeWaypoint();
        _gateLineRenderer.enabled = true;
        _gateRenderer.enabled = true;
        _gateCollider.enabled = true;
        GateParticles.Play();
    }

    void MakeWaypoint()
    {
        Vector3 direction;
        Vector3 longestHit = transform.position;
        Vector3 halfwayPoint;
        Vector3 halfwayPointDirection;
        Vector3 waypointAttachPoint1 = new Vector3();
        Vector3 waypointAttachPoint2 = new Vector3();
        float gateLength;

        //CreatePrimitive(transform.position, Color.red);
        if(_remainingGates == NumberOfGates || NumberOfGates == 0)
        {
            for (int i = 1; i < 360.0f; i = i + 30)
            {
                direction = Quaternion.AngleAxis(i, Vector3.back) * Vector3.right;
                Debug.DrawRay(transform.position, direction * 1000.0f, Color.red, 60.0f);
                if (Physics.Raycast(transform.position, direction, out _hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("Wall")))
                {
                    longestHit = ((_hit.point - transform.position).magnitude > (longestHit - transform.position).magnitude) ? _hit.point : longestHit;
                }
            }
        }
        else
        {
            for (int i = 1; i < 180.0f; i = i + 30)
            {
                direction = Quaternion.AngleAxis(i, Vector3.back) * _gateNormal;
                direction = Quaternion.AngleAxis(105, Vector3.back) * direction;
                Debug.DrawRay(transform.position, direction * 1000.0f, Color.red, 10.0f);
                if (Physics.Raycast(transform.position, direction, out _hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("Wall")))
                {
                    longestHit = ((_hit.point - transform.position).magnitude > (longestHit - transform.position).magnitude) ? _hit.point : longestHit;
                }
            }
        }
        //CreatePrimitive(longestHit, Color.yellow);
        //Debug.DrawRay(transform.position, longestHit - transform.position, Color.yellow, 60.0f);
        halfwayPoint = Vector3.Lerp(longestHit, transform.position, 0.5f);
        halfwayPointDirection = longestHit - transform.position;
        _gateNormal = -halfwayPointDirection;

        //CreatePrimitive(halfwayPoint, Color.green);

        halfwayPointDirection = Quaternion.AngleAxis(90, Vector3.back) * halfwayPointDirection;
        //Debug.DrawRay(halfwayPoint, halfwayPointDirection * 1000.0f, Color.blue, 60.0f);

        if (Physics.Raycast(halfwayPoint, halfwayPointDirection, out _hit, 1 << LayerMask.NameToLayer("Wall")))
        {
            waypointAttachPoint1 = _hit.point;

            //CreatePrimitive(waypointAttachPoint1, Color.blue);
        }


        halfwayPointDirection = Quaternion.AngleAxis(180, Vector3.back) * halfwayPointDirection;
        //Debug.DrawRay(halfwayPoint, halfwayPointDirection * 1000.0f, Color.black, 60.0f);

        if (Physics.Raycast(halfwayPoint, halfwayPointDirection, out _hit, 1 << LayerMask.NameToLayer("Wall")))
        {
            waypointAttachPoint2 = _hit.point;

            //CreatePrimitive(waypointAttachPoint2, Color.black);
        }
        Vector3[] waypoints = new Vector3[] { waypointAttachPoint1, waypointAttachPoint2 };
        _gateLineRenderer.enabled = true;
        _gateLineRenderer.SetPositions(waypoints);

        gateLength = (waypointAttachPoint1 - waypointAttachPoint2).magnitude;
        GateCollider.transform.position = Vector3.Lerp(waypointAttachPoint1, waypointAttachPoint2, 0.5f);
        GateCollider.transform.LookAt(GateCollider.transform.position - _gateNormal);
        GateCollider.transform.localScale = new Vector3(GateCollider.transform.localScale.x, gateLength,  GateCollider.transform.localScale.z);

        CurrentWaypointPosition = GateCollider.transform.position;
        _gateVisible = _gateRenderer.isVisible;
    }

    void OnTriggerEnter(Collider other)
    {
        float angle = Vector3.Dot(other.attachedRigidbody.velocity, GateCollider.transform.forward);
        _remainingGates = _remainingGates - 1;

        if (angle > 0)
        {
            if (_remainingGates > 0 || _remainingGates < 0)
            {
                Debug.DrawRay(_gateLineRenderer.GetPosition(0), (_gateLineRenderer.GetPosition(1) - _gateLineRenderer.GetPosition(0)) * 1000.0f, Color.magenta, 10.0f);

                transform.position = GateCollider.transform.position;
                MakeWaypoint();
                OnGatesPassed();
            }
            else
            {
                if(OnGatesFinished != null)
                {
                    GateParticles.Stop();
                    gameObject.SetActive(false);
                    OnGatesFinished();
                }
                _remainingGates = NumberOfGates;
            }     
        }
    }

    void CreatePrimitive(Vector3 location, Color color)
    {
        GameObject testObject2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        testObject2.transform.position = location;
        testObject2.transform.localScale = Vector3.one * 5;
        testObject2.GetComponent<Renderer>().material.color = color;
    }
}
