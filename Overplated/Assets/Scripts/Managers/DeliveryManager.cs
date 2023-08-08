using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

public class DeliveryManager : NetworkBehaviour
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
        if (!IsServer) return;

        spawnRecipeTimer -= (spawnRecipeTimer > 0f && GameManager.Instance.IsGamePlaying() && !RecipeListIsFull())  ?  Time.deltaTime : 0f;

        if (spawnRecipeTimer <= 0f)
        {
            spawnRecipeTimerMax = GenerateRandomSpawnTime(spawnRecipeTimerMinMax * GameManager.Instance.GetEvaluatedRecipeDifficulty());
            spawnRecipeTimer = spawnRecipeTimerMax;

            if (waitingRecipeSOList.Count < waitingRecipesMax)
            {
                int waitingRecipeSOIndex = UnityEngine.Random.Range(0, recipeListSO.recipeSOList.Count);

                SpawnNewWaitingRecipeClientRpc(waitingRecipeSOIndex);
            }
        }
    }

    [ClientRpc]
    private void SpawnNewWaitingRecipeClientRpc(int waitingRecipeSOIndex)
    {
        RecipeSO waitingRecipeSO = recipeListSO.recipeSOList[waitingRecipeSOIndex];

        waitingRecipeSOList.Add(waitingRecipeSO);

        OnRecipeSpawned?.Invoke(this, EventArgs.Empty);
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
                    DeliverCorrectRecipeServerRpc(i);
                    return;
                }
            }
        }
        // No correct recipe delivered
        DeliverIncorrectRecipeServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void DeliverIncorrectRecipeServerRpc()
    {
        DeliverIncorrectRecipeClientRpc();
    }

    [ClientRpc]
    private void DeliverIncorrectRecipeClientRpc()
    {
        OnRecipeFailed?.Invoke(this, EventArgs.Empty);

        failedRecipesAmount++;
    }

    [ServerRpc(RequireOwnership = false)]
    private void DeliverCorrectRecipeServerRpc(int waitingRecipeSOIndex)
    {
        DeliverCorrectRecipeClientRpc(waitingRecipeSOIndex);
    }

    [ClientRpc]
    private void DeliverCorrectRecipeClientRpc(int waitingRecipeSOIndex)
    {
        waitingRecipeSOList.RemoveAt(waitingRecipeSOIndex);

        successfulRecipesAmount++;

        OnRecipeSuccess?.Invoke(this, EventArgs.Empty);
        OnRecipeCompleted?.Invoke(this, EventArgs.Empty);
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
