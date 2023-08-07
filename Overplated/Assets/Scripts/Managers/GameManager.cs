using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEditor.Rendering;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    private const string PLAYER_PREFS_BEST_SURVIVAL_SCORE = "BestSurvivalScore";
    private const string PLAYER_PREFS_BEST_TIMED_SCORE = "BestTimedScore";

    public static GameManager Instance { get; private set; }


    public event EventHandler OnStateChanged;
    public event EventHandler OnGamePaused;
    public event EventHandler OnGameUnpaused;
    public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnHealhChanged;
    
    private enum State { WaitingToStart, CountdownToStart, GamePlaying, GameOver }
    private State state;
    public enum Gamemode { Timer, Survival }
    [SerializeField] private Gamemode gamemode;

    private float countdownToStartTimer;
    [SerializeField] float countdownToStartTimerMax;
    private float gamePlayingTimer;
    [SerializeField] float gamePlayingTimerMax;

    [Space]

    [SerializeField] float healthBarFillSpeed;
    [SerializeField] float healthBarEmptySpeed;
    [SerializeField] private AnimationCurve recipeSpawnrateDifficultyCurve;
    [SerializeField] private float maxDifficultyScaleTime;

    private float currentHealthBarValue = 0f;
    private bool isGamePaused = false;

    private bool newBest = false;


    

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
        gamePlayingTimer = gamemode == Gamemode.Timer  ?  gamePlayingTimerMax : 0f;
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
                if (gamemode == Gamemode.Timer)
                {
                    gamePlayingTimer -= gamePlayingTimer > 0  ?  Time.deltaTime : 0f;
                    if (gamePlayingTimer <= 0f) {
                        if (PlayerPrefs.GetInt(PLAYER_PREFS_BEST_TIMED_SCORE, 0) < DeliveryManager.Instance.GetSuccessfulRecipesAmount()) 
                        {
                            PlayerPrefs.SetInt(PLAYER_PREFS_BEST_TIMED_SCORE, DeliveryManager.Instance.GetSuccessfulRecipesAmount());
                            PlayerPrefs.Save();

                            newBest = true;
                        }

                        state = State.GameOver;
                        InvokeStateChanged();
                    }
                }
                else
                {
                    gamePlayingTimer += Time.deltaTime;

                    if (DeliveryManager.Instance.RecipeListIsFull())
                    {
                        // fill health bar
                        currentHealthBarValue += currentHealthBarValue < 1  ?  healthBarFillSpeed * Time.deltaTime : 0f;
                        InvokeHealthBarChanged(currentHealthBarValue);

                        if (currentHealthBarValue >= 1) {
                            if (PlayerPrefs.GetInt(PLAYER_PREFS_BEST_SURVIVAL_SCORE, 0) < DeliveryManager.Instance.GetSuccessfulRecipesAmount()) 
                            {
                                PlayerPrefs.SetInt(PLAYER_PREFS_BEST_SURVIVAL_SCORE, DeliveryManager.Instance.GetSuccessfulRecipesAmount());
                                PlayerPrefs.Save();

                                newBest = true;
                            }

                            state = State.GameOver;
                            InvokeStateChanged();
                        }
                    }
                    else
                    {
                        // empty health bar
                        currentHealthBarValue = currentHealthBarValue > 0  ?  currentHealthBarValue - healthBarEmptySpeed * Time.deltaTime : 0f;
                        InvokeHealthBarChanged(currentHealthBarValue);
                    }
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

    private void InvokeHealthBarChanged(float _progressNormalized)
    {
        OnHealhChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs {
            progressNormalized = _progressNormalized
        });
    }

    public float GetEvaluatedRecipeDifficulty()
    {
        return recipeSpawnrateDifficultyCurve.Evaluate(gamePlayingTimer / maxDifficultyScaleTime);
    }

    public Gamemode GetGamemode()
    {
        return gamemode;
    }

    public bool IsNewBest()
    {
        return newBest;
    }
}
