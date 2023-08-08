using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using UnityEngine;

public class SelectedCounterVisual : MonoBehaviour
{

    [SerializeField] private BaseCounter baseCounter;
    [SerializeField] private GameObject[] visualGameObjects;


    
    private void Start()
    {
        if (PlayerController.LocalInstance != null) {
            PlayerController.LocalInstance.OnSelectedCounterChange += PlayerController_OnSelectedCounterChanged;
        } else {
            PlayerController.OnAnyPlayerSpawned += PlayerController_OnAnyPlayerSpawned;
        }
    }

    private void PlayerController_OnAnyPlayerSpawned(object sender, EventArgs e)
    {
        if (PlayerController.LocalInstance != null) {
            PlayerController.LocalInstance.OnSelectedCounterChange -= PlayerController_OnSelectedCounterChanged;
            PlayerController.LocalInstance.OnSelectedCounterChange += PlayerController_OnSelectedCounterChanged;
        }
    }

    private void PlayerController_OnSelectedCounterChanged(object sender, PlayerController.OnSelectedCounterChangeEventArgs e) 
    {
        if (e.selectedCounter == baseCounter) {
            Show();
        } else {
            Hide();
        }
    }


    private void Show()
    {
        foreach (GameObject visualGameObject in visualGameObjects) {
            visualGameObject.SetActive(true);
        }
    }
    private void Hide()
    {
        foreach (GameObject visualGameObject in visualGameObjects) {
            visualGameObject.SetActive(false);
        }
    }

}
