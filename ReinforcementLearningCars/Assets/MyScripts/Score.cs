using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Score : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI score;
    [SerializeField] private float standInCorrectSpotReward = 5;

    private int currentAttempts = 0;
    private int currentWins = 0;

    public static Score instance;

    public float StandInCorrectSpotReward { get => standInCorrectSpotReward; set => standInCorrectSpotReward = value; }

    private void Awake()
    {
        if (instance != null) Destroy(gameObject);
        else instance = this;

        currentAttempts = 0;
        currentWins = 0;
    }

    public void ChangeScore(int attempt, int win)
    {
        currentAttempts += attempt;
        currentWins += win;
        if (win > 0)
            Debug.Log($"WIN! Attempts: {currentAttempts}, Wins: {currentWins}, WinRate: {(float)currentWins / currentAttempts * 100:F1}%");
        if (currentAttempts % 100 == 0)
            Debug.Log($"[Score] Attempts: {currentAttempts}, Wins: {currentWins}, WinRate: {(float)currentWins / currentAttempts * 100:F1}%");
    }

}
