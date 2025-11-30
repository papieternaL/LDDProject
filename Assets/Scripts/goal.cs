using UnityEngine;
using UnityEngine.SceneManagement;

public class Goal : MonoBehaviour
{
    public int scoreValue = 1;
    public bool resetBallOnScore = true;

    private void OnTriggerEnter(Collider other)
    {
        // accept either the collider or its parent having Ball
        Ball ball = other.GetComponentInParent<Ball>() ?? other.GetComponent<Ball>();
        if (ball == null) return;

        ScoreManager.Instance?.AddScore(scoreValue);

        if (resetBallOnScore)
            ball.ResetBall();
    }
}
