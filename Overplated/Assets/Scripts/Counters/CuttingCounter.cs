using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
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
                KitchenObject kitchenObject = player.GetKitchenObject();
                kitchenObject.SetKitchenObjectParent(this);

                ResetCuttingProgressServerRpc(); // resets the progress bar
            }
        }
        else
        {

            // Counter has a kitchen object
            if (player.HasKitchenObject() && player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject))
            {
                // Player is holding a plate
                if (plateKitchenObject.TryAddIngredient(GetKitchenObject().GetKitchenObjectSO())) {
                    KitchenObject.DestroyKitchenObject(GetKitchenObject());
                }
            }
            else if (player.HasKitchenObject() && player.CanSwapItemsOnCounters())
            {
                // Player is holding a non-plate object & swapping is allowed
                ResetCuttingProgressServerRpc();
                SwapItemsServerRpc(player.NetworkObject);
            }
            else if (!player.HasKitchenObject())
            {
                // Player is not holding anything
                GetKitchenObject().SetKitchenObjectParent(player);

                ResetCuttingProgressServerRpc();
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ResetCuttingProgressServerRpc()
    {
        ResetCuttingProgressClientRpc();
    }

    [ClientRpc]
    private void ResetCuttingProgressClientRpc()
    {
        cuttingProgress = 0;

        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs {
                progressNormalized = 0f
            });
    }

    [ServerRpc(RequireOwnership = false)]
    private void SwapItemsServerRpc(NetworkObjectReference playersNetworkObjectReference)
    {
        SwapItemsClientRpc(playersNetworkObjectReference);
    }

    [ClientRpc]
    private void SwapItemsClientRpc(NetworkObjectReference playersNetworkObjectReference)
    {
        playersNetworkObjectReference.TryGet(out NetworkObject playerNetworkObject);
        PlayerController player = playerNetworkObject.GetComponent<PlayerController>();

        KitchenObject playersKitchenObject = player.GetKitchenObject();
        player.GetKitchenObject().ClearKitchenObjectFromParent(player);
        GetKitchenObject().SetKitchenObjectParent(player, false);
        playersKitchenObject.SetKitchenObjectParent(this);
    }

    public override void InteractAlternate(PlayerController player)
    {
        if (HasKitchenObject() && !player.HasKitchenObject())
        {
            CutObjectServerRpc();
            TestCuttingProgressDoneServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void CutObjectServerRpc()
    {
        CutObjectClientRpc();
    }

    [ClientRpc]
    private void CutObjectClientRpc()
    {
        CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());

        if (cuttingRecipeSO != null)
        {
            cuttingProgress++;
            
            OnCut?.Invoke(this, EventArgs.Empty);
            OnAnyCut?.Invoke(this, EventArgs.Empty);
            TriggerOnProgressChangedEvent(cuttingRecipeSO);
        }
        else
        {
            Debug.Log("Invalid recipe.");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void TestCuttingProgressDoneServerRpc()
    {
        CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());
        
        if (cuttingRecipeSO == null) return;
        
        if (cuttingProgress >= cuttingRecipeSO.cuttingProgressMax)
        {
            KitchenObjectSO cuttingRecipeOutputSO = cuttingRecipeSO.recipeOutput;

            KitchenObject.DestroyKitchenObject(GetKitchenObject());

            KitchenObject.SpawnKitchenObject(cuttingRecipeOutputSO, this);
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

