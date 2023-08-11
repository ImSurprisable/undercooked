using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GamePauseUI : MonoBehaviour
{

    public static GamePauseUI Instance { get; private set; }

    [SerializeField] private Button resumeButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button optionsButton;

    

    private void Awake()
    {
        Instance = this;

        resumeButton.onClick.AddListener(() => {
            GameManager.Instance.TogglePauseGame();
        });
        mainMenuButton.onClick.AddListener(() => {
            NetworkManager.Singleton.Shutdown();
            Loader.Load(Loader.Scene.MainMenuScene);
        });
        optionsButton.onClick.AddListener(() => {
            OptionsUI.Instance.Show();
            Hide();
        });
    }

    private void Start()
    {
        GameManager.Instance.OnGamePaused += GameManager_OnGamePaused;
        GameManager.Instance.OnGameUnpaused += GameManager_OnGameUnpaused;

        Hide();
    }

    private void GameManager_OnGamePaused(object sender, EventArgs e)
    {
        Show();

        if (GameMultiplayer.playSingleplayer) {
            Time.timeScale = 0f;
        }
    }

    private void GameManager_OnGameUnpaused(object sender, EventArgs e)
    {
        Hide();

        Time.timeScale = 1f;
    }



    public void Show()
    {
        gameObject.SetActive(true);

        if (GameInput.isUsingController) resumeButton.Select();

        if (GameMultiplayer.playSingleplayer) {
            Time.timeScale = 0f;
        }
    }
    private void Hide()
    {
        gameObject.SetActive(false);
    }

}
