using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{

    private const string NEW_BEST = "NewBest";
    private const string PLAYER_PREFS_BEST_SURVIVAL_SCORE = "BestSurvivalScore";
    private const string PLAYER_PREFS_BEST_TIMED_SCORE = "BestTimedScore";
    
    [SerializeField] private TextMeshProUGUI recipesDeliveredText;
    [SerializeField] private TextMeshProUGUI recipesFailedText;
    [SerializeField] private TextMeshProUGUI recipesDeliveredBestText;
    [SerializeField] private Button mainMenuButton;

    private Animator animator;





    private void Awake()
    {
        animator = GetComponent<Animator>();

        mainMenuButton.onClick.AddListener(() => {
            NetworkManager.Singleton.Shutdown();
            Loader.Load(Loader.Scene.MainMenuScene);
        });
    }

    private void Start()
    {
        GameManager.Instance.OnStateChanged += GameManager_OnStateChanged;

        Hide();
    }

    private void GameManager_OnStateChanged(object sender, EventArgs e)
    {
        if (GameManager.Instance.IsGameOver()) {
            Show();

            recipesDeliveredText.text = DeliveryManager.Instance.GetSuccessfulRecipesAmount().ToString();
            recipesFailedText.text = DeliveryManager.Instance.GetFailedRecipesAmount().ToString();

            if (GameManager.Instance.GetGamemode() == GameManager.Gamemode.Timer) 
            {
                recipesDeliveredBestText.text = PlayerPrefs.GetInt(PLAYER_PREFS_BEST_TIMED_SCORE, 0).ToString();
                
            }
            else if (GameManager.Instance.GetGamemode() == GameManager.Gamemode.Survival)
            {
                recipesDeliveredBestText.text = PlayerPrefs.GetInt(PLAYER_PREFS_BEST_SURVIVAL_SCORE, 0).ToString();
            }

            if (GameManager.Instance.IsNewBest()) {
                animator.SetBool(NEW_BEST, true);
            }
        }
        else {
            Hide();
        }
    }



    private void Show()
    {
        gameObject.SetActive(true);
    }
    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
