using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SurvivalHealthBarUI : MonoBehaviour
{
    
    [SerializeField] private Image fillImage;
    [SerializeField] private Gradient fillGradient;



    private void Start()
    {
        GameManager.Instance.OnHealhChanged += GameManager_OnHealthChanged;

        if (GameManager.Instance.GetGamemode() != GameManager.Gamemode.Survival)
        {
            gameObject.SetActive(false);
        }
    }

    private void GameManager_OnHealthChanged(object sender, IHasProgress.OnProgressChangedEventArgs e)
    {
        fillImage.fillAmount = e.progressNormalized;
        fillImage.color = fillGradient.Evaluate(e.progressNormalized);
    }
}
