using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PreGameSettingsUI : NetworkBehaviour
{

    public static PreGameSettingsUI Instance { get; private set; }

    [SerializeField] private TMP_Dropdown gamemodeDropdown;
    [SerializeField] private Toggle playerCollisionToggle;
    [SerializeField] private Button closeButton;

    private NetworkVariable<int> gamemode = new NetworkVariable<int>();
    private NetworkVariable<bool> playerCollisionsEnabled = new NetworkVariable<bool>();



    private void Awake()
    {
        Instance = this;

        closeButton.onClick.AddListener(Hide);
        
        InitializeDropdownOptions(gamemodeDropdown, GameManager.gamemode);
        gamemodeDropdown.onValueChanged.AddListener(delegate { gamemode.Value = gamemodeDropdown.value; });
        gamemode.Value = gamemodeDropdown.value;

        playerCollisionToggle.onValueChanged.AddListener(delegate { playerCollisionsEnabled.Value = playerCollisionToggle.isOn; });
        playerCollisionsEnabled.Value = playerCollisionToggle.isOn;
    }

    private void Start()
    {
        Hide();
    }



    [ServerRpc()]
    public void UpdateGameSettingsServerRpc()
    {
        UpdateGameSettingsClientRpc();
    }
    [ClientRpc()]
    private void UpdateGameSettingsClientRpc()
    {
        GameManager.gamemode = (GameManager.Gamemode)gamemode.Value;
        GameManager.playerCollisionsEnabled = playerCollisionsEnabled.Value;
    }



    private void InitializeDropdownOptions(TMP_Dropdown dropdown, Enum targetEnum)
    {
        Type enumType = targetEnum.GetType();
        List<TMP_Dropdown.OptionData> newOptions = new List<TMP_Dropdown.OptionData>();

        for (int i = 0; i < Enum.GetNames(enumType).Length; i++)
        {
            newOptions.Add(new TMP_Dropdown.OptionData(Enum.GetName(enumType, i)));
        }

        dropdown.ClearOptions();
        dropdown.AddOptions(newOptions);
    }



    public void Show()
    {
        gameObject.SetActive(true);
    }
    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
