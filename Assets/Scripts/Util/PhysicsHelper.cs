using UnityEngine;
using System.Collections;

public class PhysicsHelper : Singleton<PhysicsHelper>
{
    public bool isMovingTowards(Vector3 testPoint, Vector3 objectPosition, Vector3 objectVelocty)
    {
        Vector3 toPoint = testPoint - objectPosition; //a vector going from your obect to the point
        return Vector3.Dot(toPoint, objectVelocty) > 0;
    }
}