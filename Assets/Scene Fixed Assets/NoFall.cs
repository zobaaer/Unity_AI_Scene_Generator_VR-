using UnityEngine;

public class ObjectFallReset : MonoBehaviour
{
    public float resetHeight = 2f;
    public float minY = -5f;

    private Vector3 startPos;

    private void Start()
    {
        startPos = transform.position;
    }

    private void Update()
    {
        if (transform.position.y < minY)
        {
            Debug.Log($"{gameObject.name} fell below {minY}, resetting...");
            transform.position = new Vector3(startPos.x, resetHeight, startPos.z);

            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
    }
}
