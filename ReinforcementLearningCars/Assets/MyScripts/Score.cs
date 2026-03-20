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

        currentAttempts = PlayerPrefs.GetInt("currentAttempts");
        currentWins = PlayerPrefs.GetInt("currentWins");

    }

    public void ChangeScore(int attempt, int win)
    {
        currentAttempts += attempt;
        currentWins += win;
        PlayerPrefs.SetInt("currentAttempts", currentAttempts);
        PlayerPrefs.SetInt("currentWins", currentWins);
        score.text = "Attempts: " + currentAttempts + " \nWins: " + currentWins;
        if (currentAttempts == 1000) Debug.LogError("1000 attempts, " + currentWins + " wins.");
    }

}
