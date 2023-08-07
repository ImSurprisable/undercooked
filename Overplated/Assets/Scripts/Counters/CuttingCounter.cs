using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CuttingCounter : BaseCounter, IHasProgress
{

    public static event EventHandler OnAnyCut;

    new public static void ResetStaticData() {
        OnAnyCut = null;
    }

    public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;

    public event EventHandler OnCut;

    [SerializeField] private CuttingRecipeSO[] cuttingRecipesSO;

    private int cuttingProgress;



    public override void Interact(PlayerController player)
    {
        if (!HasKitchenObject())
        {
            // No kitchen object
            if (player.HasKitchenObject() && !player.GetKitchenObject().TryGetPlate(out _))
            {
                // Player is carrying a non-plate object
                player.GetKitchenObject().SetKitchenObjectParent(this);
            }
        }
        else
        {
            CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());

            // Counter has a kitchen object
            if (player.HasKitchenObject() && player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject))
            {
                // Player is holding a plate
                if (plateKitchenObject.TryAddIngredient(GetKitchenObject().GetKitchenObjectSO())) {
                GetKitchenObject().DestroySelf();
                }
            }
            else if (player.HasKitchenObject() && player.CanSwapItemsOnCounters())
            {
                // Player is holding a non-plate object & swapping is allowed
                KitchenObject playersKitchenObject = player.GetKitchenObject();
                player.GetKitchenObject().ClearKitchenObjectFromParent(player);
                GetKitchenObject().SetKitchenObjectParent(player);
                playersKitchenObject.SetKitchenObjectParent(this);
            }
            else
            {
                // Player is not holding anything
                GetKitchenObject().SetKitchenObjectParent(player);
            }
            cuttingProgress = 0;

            TriggerOnProgressChangedEvent(cuttingRecipeSO);
        }
    }

    public override void InteractAlternate(PlayerController player)
    {
        if (HasKitchenObject() && !player.HasKitchenObject())
        {
            CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());

            if (cuttingRecipeSO != null)
            {
                cuttingProgress++;
                
                OnCut?.Invoke(this, EventArgs.Empty);
                OnAnyCut?.Invoke(this, EventArgs.Empty);
                TriggerOnProgressChangedEvent(cuttingRecipeSO);

                if (cuttingProgress >= cuttingRecipeSO.cuttingProgressMax)
                {
                    KitchenObjectSO cuttingRecipeOutputSO = cuttingRecipeSO.recipeOutput;
                    GetKitchenObject().DestroySelf();

                    KitchenObject.SpawnKitchenObject(cuttingRecipeOutputSO, this);
                }
            }
            else
            {
                Debug.Log("Invalid recipe.");
            }
        }
    }

    private KitchenObjectSO GetOutputForInput(KitchenObjectSO inputKitchenObjectSO)
    {
        CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(inputKitchenObjectSO);

        if (cuttingRecipeSO != null) {
            return cuttingRecipeSO.recipeOutput;
        } else {
            return null;
        }
    }

    private CuttingRecipeSO GetCuttingRecipeSOWithInput(KitchenObjectSO inputKitchenObjectSO)
    {
        foreach(CuttingRecipeSO cuttingRecipeSO in cuttingRecipesSO)
        {
            if (cuttingRecipeSO.recipeInput == inputKitchenObjectSO) {
                return cuttingRecipeSO;
            }
        }
        return null;
    }

    private void TriggerOnProgressChangedEvent(CuttingRecipeSO cuttingRecipeSO)
    {
        if (cuttingRecipeSO != null) {
            OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs {
                progressNormalized = (float)cuttingProgress / cuttingRecipeSO.cuttingProgressMax
            });
        }
    }
}

