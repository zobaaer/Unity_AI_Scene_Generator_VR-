using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TennisBounceGame : MonoBehaviour
{
    public Rigidbody ballRb;
    public Rigidbody batRb;
    public Transform bat;
    public Transform ball;
    public TMP_Text scoreText;
    public Button resetButton;

    private Vector3 initialBatPos;
    private Vector3 initialBallPos;
    private int score = 0;

    void Start()
    {
        initialBatPos = bat.position;
        initialBallPos = ball.position;

        resetButton.onClick.AddListener(ResetGame);
        UpdateScoreText();
    }

    public void AddScore(int amount)
    {
        score += amount;
        UpdateScoreText();
    }

    public void ResetScore()
    {
        score = 0;
        UpdateScoreText();
    }

    public void ResetGame()
    {
            // Reset positions
        bat.position = initialBatPos;
        ball.position = initialBallPos;

        // Reset physics for the ball
        ballRb.linearVelocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;
        ballRb.useGravity = false;

        // Reset physics for the ball
        batRb.linearVelocity = Vector3.zero;
        batRb.angularVelocity = Vector3.zero;
        batRb.useGravity = false;
        batRb.isKinematic = true;
        batRb.isKinematic = false;

        // Reset score
        ResetScore();
    }

    void UpdateScoreText()
    {
        scoreText.text = score.ToString();
    }
}
