using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DeliveryManager : MonoBehaviour
{

    public event EventHandler OnRecipeSpawned;
    public event EventHandler OnRecipeCompleted;
    public event EventHandler OnRecipeSuccess;
    public event EventHandler OnRecipeFailed;
    
    public static DeliveryManager Instance { get; private set; }
    [SerializeField] private RecipeListSO recipeListSO;

    private List<RecipeSO> waitingRecipeSOList = new List<RecipeSO>();

    private float spawnRecipeTimer;
    [SerializeField] Vector2 spawnRecipeTimerMinMax;
    float spawnRecipeTimerMax;
    [SerializeField] int waitingRecipesMax;
    private int successfulRecipesAmount;
    private int failedRecipesAmount;
    [SerializeField] private float firstRecipeSpawnMultiplier;



    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        spawnRecipeTimerMax = GenerateRandomSpawnTime(spawnRecipeTimerMinMax);
        
        spawnRecipeTimer = spawnRecipeTimerMax * firstRecipeSpawnMultiplier;
    }

    private void Update()
    {
        spawnRecipeTimer -= (spawnRecipeTimer > 0f && GameManager.Instance.IsGamePlaying() && !RecipeListIsFull())  ?  Time.deltaTime : 0f;

        if (spawnRecipeTimer <= 0f)
        {
            spawnRecipeTimerMax = GenerateRandomSpawnTime(spawnRecipeTimerMinMax * GameManager.Instance.GetEvaluatedRecipeDifficulty());
            spawnRecipeTimer = spawnRecipeTimerMax;

            if (waitingRecipeSOList.Count < waitingRecipesMax)
            {
                RecipeSO waitingRecipeSO = recipeListSO.recipeSOList[UnityEngine.Random.Range(0, recipeListSO.recipeSOList.Count)];
                waitingRecipeSOList.Add(waitingRecipeSO);

                OnRecipeSpawned?.Invoke(this, EventArgs.Empty);
            }
        }
    }



    public void DeliverRecipe(PlateKitchenObject plateKitchenObject)
    {
        for (int i = 0; i < waitingRecipeSOList.Count; i++)
        {
            RecipeSO waitingRecipeSO = waitingRecipeSOList[i];

            if (waitingRecipeSO.kitchenObjectSOList.Count == plateKitchenObject.GetKitchenObjectSOList().Count)
            {
                // Same # of ingredients
                bool plateContentsMatchesRecipe = true;
                foreach (KitchenObjectSO recipeKitchenObjectSO in waitingRecipeSO.kitchenObjectSOList)
                {
                    // Cycle all ingredients in Recipe
                    bool ingredientFound = false;
                    foreach (KitchenObjectSO plateKitchenObjectSO in plateKitchenObject.GetKitchenObjectSOList())
                    {
                        // Cycle all ingredients on Plate
                        if (plateKitchenObjectSO == recipeKitchenObjectSO)
                        {
                            // Ingredient match
                            ingredientFound = true;
                            break;
                        }
                    }
                    if (!ingredientFound) {
                        plateContentsMatchesRecipe = false;
                    }
                }

                if (plateContentsMatchesRecipe)
                {
                    // Correct recipe delivered
                    waitingRecipeSOList.RemoveAt(i);

                    successfulRecipesAmount++;

                    OnRecipeSuccess?.Invoke(this, EventArgs.Empty);
                    OnRecipeCompleted?.Invoke(this, EventArgs.Empty);
                    return;
                }
            }
        }
        // No correct recipe delivered
        OnRecipeFailed?.Invoke(this, EventArgs.Empty);

        failedRecipesAmount++;
    }
    

    private float GenerateRandomSpawnTime(Vector2 minMax)
    {
        return UnityEngine.Random.Range(minMax.x, minMax.y);
    }


    public List<RecipeSO> GetWaitingRecipeSOList()
    {
        return waitingRecipeSOList;
    }

    public int GetSuccessfulRecipesAmount()
    {
        return successfulRecipesAmount;
    }
    public int GetFailedRecipesAmount()
    {
        return failedRecipesAmount;
    }

    public bool RecipeListIsFull()
    {
        return waitingRecipeSOList.Count >= waitingRecipesMax;
    }
}
