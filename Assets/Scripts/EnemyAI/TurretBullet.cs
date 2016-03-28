using UnityEngine;
using System.Collections;

public class TurretBullet : MonoBehaviour {
    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.transform.root.tag != "Enemy" && collision.collider.transform.root.tag != "Bullet")
        {
            Destroy(gameObject);
        }
    }
}
