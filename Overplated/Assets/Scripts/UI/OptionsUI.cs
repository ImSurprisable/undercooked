using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class OptionsUI : MonoBehaviour
{

    private const string PLAYER_PREFS_BEST_SURVIVAL_SCORE = "BestSurvivalScore";
    private const string PLAYER_PREFS_BEST_TIMED_SCORE = "BestTimedScore";

    public static OptionsUI Instance { get; private set; }
    
    [SerializeField] private Slider musicSlider;
    [SerializeField] private TextMeshProUGUI musicValueText;
    [SerializeField] private Transform musicSliderFill;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private TextMeshProUGUI sfxValueText;
    [SerializeField] private Transform sfxSliderFill;
    [SerializeField] private Button closeButton;
    [SerializeField] private Transform pressToRebindKeyTransform;

    [Space]

    [SerializeField] private Button moveUpButton;
    [SerializeField] private TextMeshProUGUI moveUpText;
    [SerializeField] private Button moveLeftButton;
    [SerializeField] private TextMeshProUGUI moveLeftText;
    [SerializeField] private Button moveDownButton;
    [SerializeField] private TextMeshProUGUI moveDownText;
    [SerializeField] private Button moveRightButton;
    [SerializeField] private TextMeshProUGUI moveRightText;
    [SerializeField] private Button interactButton;
    [SerializeField] private TextMeshProUGUI interactText;
    [SerializeField] private Button interactAlternateButton;
    [SerializeField] private TextMeshProUGUI interactAlternateText;

    [Space]

    [SerializeField] private Button gamepadInteractButton;
    [SerializeField] private TextMeshProUGUI gamepadInteractText;
    [SerializeField] private Button gamepadInteractAlternateButton;
    [SerializeField] private TextMeshProUGUI gamepadInteractAlternateText;

    [Space]

    [SerializeField] private Button resetBestButton;


    private void Awake()
    {
        Instance = this;

        closeButton.onClick.AddListener(() => {
            PlayerPrefs.Save();
            Hide();
            if (GamePauseUI.Instance != null) {
                GamePauseUI.Instance.Show();
            }
        });
        resetBestButton.onClick.AddListener(() => {
            PlayerPrefs.SetInt(PLAYER_PREFS_BEST_SURVIVAL_SCORE, 0);
            PlayerPrefs.SetInt(PLAYER_PREFS_BEST_TIMED_SCORE, 0);
            PlayerPrefs.Save();

            Debug.LogWarning("Best score reset");
        });
        
        moveUpButton.onClick.AddListener(() => { RebindBinding(GameInput.Binding.Move_Up); });
        moveLeftButton.onClick.AddListener(() => { RebindBinding(GameInput.Binding.Move_Left); });
        moveDownButton.onClick.AddListener(() => { RebindBinding(GameInput.Binding.Move_Down); });
        moveRightButton.onClick.AddListener(() => { RebindBinding(GameInput.Binding.Move_Right); });
        interactButton.onClick.AddListener(() => { RebindBinding(GameInput.Binding.Interact); });
        interactAlternateButton.onClick.AddListener(() => { RebindBinding(GameInput.Binding.InteractAlternate); });
        gamepadInteractButton.onClick.AddListener(() => { RebindBinding(GameInput.Binding.Gamepad_Interact); });
        gamepadInteractAlternateButton.onClick.AddListener(() => { RebindBinding(GameInput.Binding.Gamepad_InteractAlternate); });
        
    }

    private void Start()
    {
        if (GameManager.Instance != null) {
            GameManager.Instance.OnGameUnpaused += GameManager_OnGameUnpaused;
        }

        float musicVolume = MusicManager.GetVolume();
        musicSlider.value = musicVolume * 20f; // 20f - slider max
        ChangeMusicVolumeValue(musicSlider.value);

        float sfxVolume = SoundManager.GetVolume();
        sfxSlider.value = sfxVolume * 20f; // 20f - slider max
        ChangeSFXVolumeValue(sfxSlider.value);

        Hide();
        HidePressToRebindKey();

        UpdateVisual();
    }

    private void GameManager_OnGameUnpaused(object sender, EventArgs e)
    {
        PlayerPrefs.Save();
        Hide();
    }



    public void ChangeMusicVolumeValue(float value)
    {
        MusicManager.Instance.ChangeVolume(value / 20); // 20f - slider max

        musicValueText.text = ((value * 5) / 100).ToString("F2"); // 5 & 100 - convert to range of slider max (20)

        if (value == 0) {
            musicSliderFill.gameObject.SetActive(false);
        } else if (!musicSliderFill.gameObject.activeSelf) {
            musicSliderFill.gameObject.SetActive(true);
        }
    }
    public void ChangeSFXVolumeValue(float value)
    {
        SoundManager.ChangeVolume(value / 20); // 20f - slider max

        sfxValueText.text = ((value * 5) / 100).ToString("F2"); // 5 & 100 - convert to range of slider max (20)

        if (value == 0) {
            sfxSliderFill.gameObject.SetActive(false);
        } else if (!sfxSliderFill.gameObject.activeSelf) {
            sfxSliderFill.gameObject.SetActive(true);
        }
    }

    private void UpdateVisual()
    {
        moveUpText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Move_Up);
        moveLeftText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Move_Left);
        moveDownText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Move_Down);
        moveRightText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Move_Right);
        interactText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Interact);
        interactAlternateText.text = GameInput.Instance.GetBindingText(GameInput.Binding.InteractAlternate);
        gamepadInteractText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Gamepad_Interact);
        gamepadInteractAlternateText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Gamepad_InteractAlternate);
    }


    public void Show()
    {
        gameObject.SetActive(true);

        if (GameInput.isUsingController) musicSlider.Select();
    }
    private void Hide()
    {
        gameObject.SetActive(false);
    }

    private void ShowPressToRebindKey()
    {
        pressToRebindKeyTransform.gameObject.SetActive(true);
    }
    private void HidePressToRebindKey()
    {
        pressToRebindKeyTransform.gameObject.SetActive(false);
    }

    private void RebindBinding(GameInput.Binding binding)
    {
        ShowPressToRebindKey();
        GameInput.Instance.RebindBinding(binding, () => {
            HidePressToRebindKey();
            UpdateVisual();
        });
    }
}
