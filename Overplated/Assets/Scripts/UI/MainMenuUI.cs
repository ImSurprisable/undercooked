using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    
    [SerializeField] private Button playSingleplayerButton;
    [SerializeField] private Button playMultiplayerButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;



    private void Awake()
    {
        playSingleplayerButton.onClick.AddListener(() => {
            GameMultiplayer.playSingleplayer = true;
            Loader.Load(Loader.Scene.LobbyScene);
        });
        playMultiplayerButton.onClick.AddListener(() => {
            GameMultiplayer.playSingleplayer = false;
            Loader.Load(Loader.Scene.LobbyScene);
        });
        settingsButton.onClick.AddListener(() => {
            OptionsUI.Instance.Show();
        });
        quitButton.onClick.AddListener(() => {
            Application.Quit();
        });

        Time.timeScale = 1f;
    }

}
