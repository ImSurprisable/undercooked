using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.Netcode;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{

    private const string PLAYER_PREFS_BEST_SURVIVAL_SCORE = "BestSurvivalScore";
    private const string PLAYER_PREFS_BEST_TIMED_SCORE = "BestTimedScore";

    public static GameManager Instance { get; private set; }


    public event EventHandler OnStateChanged;
    public event EventHandler OnGamePaused;
    public event EventHandler OnGameUnpaused;
    public event EventHandler OnLocalPlayerReadyChanged;
    public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnHealhChanged;
    
    private enum State { WaitingToStart, CountdownToStart, GamePlaying, GameOver }
    private NetworkVariable<State> state = new NetworkVariable<State>(State.WaitingToStart);

    public enum Gamemode { Survival, Timer }

    private Dictionary<ulong, bool> playerReadyDictionary = new Dictionary<ulong, bool>();

    [SerializeField] private Transform playerPrefab;

    [SerializeField] private float countdownToStartTimerMax;
    private NetworkVariable<float> countdownToStartTimer = new NetworkVariable<float>(3f);
    [SerializeField] float gamePlayingTimerMax;
    private NetworkVariable<float> gamePlayingTimer = new NetworkVariable<float>(0f);


    [Space]

    [SerializeField] float healthBarFillSpeed;
    [SerializeField] float healthBarEmptySpeed;
    [SerializeField] private AnimationCurve recipeSpawnrateDifficultyCurve;
    [SerializeField] private float maxDifficultyScaleTime;

    private NetworkVariable<float> currentHealthBarValue = new NetworkVariable<float>(0f);
    private bool isGamePaused = false;
    private bool isLocalPlayerReady;
    private bool newBest = false;

    public static bool playerCollisionsEnabled;
    public static Gamemode gamemode;

    

    private void Awake()
    {
        Instance = this;
    }



    private void Start()
    {
        GameInput.Instance.OnPauseAction += GameInput_OnPauseAction;
        GameInput.Instance.OnInteractAction += GameInput_OnInteractAction;

        countdownToStartTimer.Value = countdownToStartTimerMax;
        gamePlayingTimer.Value = gamemode == Gamemode.Timer  ?  gamePlayingTimerMax : 0f;
    }

    public override void OnNetworkSpawn()
    {
        state.OnValueChanged += State_OnValueChanged;
        currentHealthBarValue.OnValueChanged += CurrentHealthBarValue_OnValueChanged;

        if (IsServer)
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;
        }
    }

    private void CurrentHealthBarValue_OnValueChanged(float previousValue, float newValue)
    {
        InvokeHealthBarChanged(newValue);
    }

    private void SceneManager_OnLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            Transform playerTransform = Instantiate(playerPrefab);
            playerTransform.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
        }
        
    }

    private void State_OnValueChanged(State previousValue, State newValue)
    {
        InvokeStateChanged();
    }

    private void GameInput_OnInteractAction(object sender, EventArgs e)
    {
        if (state.Value == State.WaitingToStart) 
        {
            isLocalPlayerReady = true;
            OnLocalPlayerReadyChanged?.Invoke(this, EventArgs.Empty);

            SetPlayerReadyServerRpc();

        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerReadyServerRpc(ServerRpcParams serverRpcParams = default)
    {
        playerReadyDictionary[serverRpcParams.Receive.SenderClientId] = true;

        bool allClientsReady = true;
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!playerReadyDictionary.ContainsKey(clientId) || !playerReadyDictionary[clientId])
            {
                // This player is NOT ready
                allClientsReady = false;
                break;
            }
        }

        if (allClientsReady) {
            state.Value = State.CountdownToStart;
        }
    }

    private void GameInput_OnPauseAction(object sender, EventArgs e)
    {
        TogglePauseGame();
    }

    private void Update()
    {
        if (!IsServer) return;

        switch (state.Value) {
            case State.WaitingToStart:
                break;
            case State.CountdownToStart:
                countdownToStartTimer.Value -= countdownToStartTimer.Value > 0  ?  Time.deltaTime : 0f;
                if (countdownToStartTimer.Value <= 0f) {
                    state.Value = State.GamePlaying;
                }
                break;
            case State.GamePlaying:
                if (gamemode == Gamemode.Timer)
                {
                    gamePlayingTimer.Value -= gamePlayingTimer.Value > 0  ?  Time.deltaTime : 0f;
                    if (gamePlayingTimer.Value <= 0f) {
                        if (PlayerPrefs.GetInt(PLAYER_PREFS_BEST_TIMED_SCORE, 0) < DeliveryManager.Instance.GetSuccessfulRecipesAmount()) 
                        {
                            PlayerPrefs.SetInt(PLAYER_PREFS_BEST_TIMED_SCORE, DeliveryManager.Instance.GetSuccessfulRecipesAmount());
                            PlayerPrefs.Save();

                            newBest = true;
                        }

                        state.Value = State.GameOver;
                    }
                }
                else
                {
                    gamePlayingTimer.Value += Time.deltaTime;

                    if (DeliveryManager.Instance.RecipeListIsFull())
                    {
                        // fill health bar
                        currentHealthBarValue.Value += currentHealthBarValue.Value < 1  ?  healthBarFillSpeed * Time.deltaTime : 0f;

                        if (currentHealthBarValue.Value >= 1) {
                            if (PlayerPrefs.GetInt(PLAYER_PREFS_BEST_SURVIVAL_SCORE, 0) < DeliveryManager.Instance.GetSuccessfulRecipesAmount()) 
                            {
                                PlayerPrefs.SetInt(PLAYER_PREFS_BEST_SURVIVAL_SCORE, DeliveryManager.Instance.GetSuccessfulRecipesAmount());
                                PlayerPrefs.Save();

                                newBest = true;
                            }

                            state.Value = State.GameOver;
                        }
                    }
                    else
                    {
                        // empty health bar
                        currentHealthBarValue.Value = currentHealthBarValue.Value > 0  ?  currentHealthBarValue.Value - healthBarEmptySpeed * Time.deltaTime : 0f;
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


    public bool IsTutorialOpen()
    {
        return state.Value == State.WaitingToStart;
    }
    public bool IsWaitingToStart()
    {
        return state.Value == State.WaitingToStart;
    }
    public bool IsCountdownToStartActive()
    {
        return state.Value == State.CountdownToStart;
    }
    public bool IsGamePlaying()
    {
        return state.Value == State.GamePlaying;
    }
    public bool IsGameOver()
    {
        return state.Value == State.GameOver;
    }

    public bool IsLocalPlayerReady()
    {
        return isLocalPlayerReady;  
    }

    public float GetCountdownToStartTimer()
    {
        return countdownToStartTimer.Value;
    }

    public float GetGamePlayingTimerNormalized()
    {
        return gamePlayingTimer.Value / gamePlayingTimerMax;
    }

    public void TogglePauseGame()
    {
        isGamePaused = !isGamePaused;
        if (isGamePaused)
        {
            // Time.timeScale = 0f;
            OnGamePaused?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            // Time.timeScale = 1f;
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
        return recipeSpawnrateDifficultyCurve.Evaluate(gamePlayingTimer.Value / maxDifficultyScaleTime);
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
