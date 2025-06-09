using UnityEngine;

public class BallCollisionHandler : MonoBehaviour
{
    public TennisBounceGame gameManager;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Editable"))
        {
            gameManager.AddScore(1);
        }
        else if (collision.collider.CompareTag("Floor"))
        {
            gameManager.ResetScore();
        }
    }
}
