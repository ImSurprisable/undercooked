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
        //PlayerController.Instance.OnSelectedCounterChange += PlayerController_OnSelectedCounterChanged;
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
