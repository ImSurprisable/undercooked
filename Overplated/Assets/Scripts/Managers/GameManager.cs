using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    public static GameManager Instance { get; private set; }


    public event EventHandler OnStateChanged;
    public event EventHandler OnGamePaused;
    public event EventHandler OnGameUnpaused;
    
    private enum State { WaitingToStart, CountdownToStart, GamePlaying, GameOver }
    private State state;

    private float countdownToStartTimer;
    [SerializeField] float countdownToStartTimerMax;
    private float gamePlayingTimer;
    [SerializeField] float gamePlayingTimerMax;
    private bool isGamePaused = false;
    

    private void Awake()
    {
        Instance = this;
        state = State.WaitingToStart;
        InvokeStateChanged();
    }

    private void Start()
    {
        GameInput.Instance.OnPauseAction += GameInput_OnPauseAction;
        GameInput.Instance.OnInteractAction += GameInput_OnInteractAction;

        countdownToStartTimer = countdownToStartTimerMax;
        gamePlayingTimer = gamePlayingTimerMax;
    }

    private void GameInput_OnInteractAction(object sender, EventArgs e)
    {
        if (state == State.WaitingToStart) 
        {
            state = State.CountdownToStart;
            OnStateChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private void GameInput_OnPauseAction(object sender, EventArgs e)
    {
        TogglePauseGame();
    }

    private void Update()
    {
        switch (state) {
            case State.WaitingToStart:
                break;
            case State.CountdownToStart:
                countdownToStartTimer -= countdownToStartTimer > 0  ?  Time.deltaTime : 0f;
                if (countdownToStartTimer <= 0f) {
                    state = State.GamePlaying;
                    InvokeStateChanged();
                }
                break;
            case State.GamePlaying:
                gamePlayingTimer -= gamePlayingTimer > 0  ?  Time.deltaTime : 0f;
                if (gamePlayingTimer <= 0f) {
                    state = State.GameOver;
                    InvokeStateChanged();
                }
                break;
            case State.GameOver:
                break;
        }
    }

    private void InvokeStateChanged()
    {
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }


    public bool IsCountdownToStartActive()
    {
        return state == State.CountdownToStart;
    }
    public bool IsGamePlaying()
    {
        return state == State.GamePlaying;
    }
    public bool IsGameOver()
    {
        return state == State.GameOver;
    }

    public float GetCountdownToStartTimer()
    {
        return countdownToStartTimer;
    }

    public float GetGamePlayingTimerNormalized()
    {
        return gamePlayingTimer / gamePlayingTimerMax;
    }

    public void TogglePauseGame()
    {
        isGamePaused = !isGamePaused;
        if (isGamePaused)
        {
            Time.timeScale = 0f;
            OnGamePaused?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            Time.timeScale = 1f;
            OnGameUnpaused?.Invoke(this, EventArgs.Empty);
        }
    }
}
