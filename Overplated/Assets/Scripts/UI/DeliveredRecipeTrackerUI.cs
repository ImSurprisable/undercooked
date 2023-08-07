using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DeliveredRecipeTrackerUI : MonoBehaviour
{

    private const string FAIL = "Fail";
    private const string SUCCESS = "Success";

    [SerializeField] private TextMeshProUGUI recipeValueText;
    
    private Animator animator;



    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        DeliveryManager.Instance.OnRecipeSuccess += DeliveryManager_OnRecipeSuccess;
        DeliveryManager.Instance.OnRecipeFailed += DeliveryManager_OnRecipeFailed;

        recipeValueText.text = DeliveryManager.Instance.GetSuccessfulRecipesAmount().ToString();
    }

    private void DeliveryManager_OnRecipeFailed(object sender, EventArgs e)
    {
        animator.SetTrigger(FAIL);
    }

    private void DeliveryManager_OnRecipeSuccess(object sender, EventArgs e)
    {
        recipeValueText.text = DeliveryManager.Instance.GetSuccessfulRecipesAmount().ToString();
        animator.SetTrigger(SUCCESS);
    }
}
