using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }
    public TMP_Text scoreText;
    private int score;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void AddScore(int amount)
    {
        score += amount;
        if (scoreText) scoreText.text = $"Score: {score}";
        else Debug.Log($"Score: {score}");
    }
}
