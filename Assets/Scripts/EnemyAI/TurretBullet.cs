using UnityEngine;
using System.Collections;

public class TurretBullet : MonoBehaviour {

    void OnCollisionEnter(Collision collision)
    {
        Destroy(gameObject);
    }
}
