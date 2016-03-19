using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    public ENEMY_STATE states;
    public GameObject TurretBarrelBody;
    public GameObject TurretBarrel;
    public GameObject player;

    private RaycastHit TurretHitInfo;

    void Awake()
    {
        states = ENEMY_STATE.IDLE;
    }

    void Start()
    {
        StartCoroutine(EnemyFSM());
    }

    void Update()
    {
        // Handle logic for state changes
        Vector3 direction = TurretBarrel.transform.position - TurretBarrelBody.transform.position;
        Debug.DrawRay(TurretBarrelBody.transform.position, direction * 10.0f, Color.yellow);
        if (states == ENEMY_STATE.IDLE)
        {
            if (Physics.Raycast(TurretBarrelBody.transform.position, direction, out TurretHitInfo, Mathf.Infinity, 1 << LayerMask.NameToLayer("Player")))
            {
                states = ENEMY_STATE.ATTACK;
            }
        }
    }

    IEnumerator EnemyFSM()
    {
        while (true)
        {
            yield return StartCoroutine(states.ToString());
        }
    }

    IEnumerator IDLE()
    {
        // Enter state code
        
        while (states == ENEMY_STATE.IDLE)
        { 
            // During state code
            int randomRotation = Random.Range(1, 360);
            float movementTime = Random.Range(1.0f, 2.0f);
            float nextMovementTime = Random.Range(1.0f, 5.0f);
            float rate = Random.Range(1.0f, 2.0f);
            float timePassed = 0.0f;
            Vector3 direction = TurretBarrel.transform.position - TurretBarrelBody.transform.position;
           
            while (timePassed < nextMovementTime && states == ENEMY_STATE.IDLE)
            {
                timePassed = timePassed + Time.deltaTime;
                yield return null;
            }
            timePassed = 0.0f;
            while (timePassed < movementTime && states == ENEMY_STATE.IDLE)
            {
                TurretBarrelBody.transform.rotation = Quaternion.Lerp(TurretBarrelBody.transform.rotation, Quaternion.Euler(0.0f, 0.0f, randomRotation), Time.deltaTime * rate);
                timePassed = timePassed + Time.deltaTime;
                yield return null;
            }
        }
        // Exit state code
        Debug.Log("State Changed");
    }

    IEnumerator ATTACK()
    {
        while (states == ENEMY_STATE.ATTACK)
        {
            //Debug.Log("SHOOT HIM");
            yield return null;
        }
    }
}

public enum ENEMY_STATE
{
    IDLE = 0,
    ATTACK = 1
}


