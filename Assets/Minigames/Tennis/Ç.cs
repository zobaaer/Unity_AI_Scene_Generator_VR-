using UnityEngine;

public class RacketCollision : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("🟢 Collided with: " + collision.gameObject.name);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("🔵 Triggered with: " + other.gameObject.name);
    }
}