using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameOverUI : MonoBehaviour
{
    
    [SerializeField] TextMeshProUGUI recipesDeliveredText;
    [SerializeField] TextMeshProUGUI recipesFailedText;


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
