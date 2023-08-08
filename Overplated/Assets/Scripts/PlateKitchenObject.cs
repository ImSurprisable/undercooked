using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlateKitchenObject : KitchenObject
{

    public event EventHandler<OnIngredientAddedEventArgs> OnIngredientAdded;
    public class OnIngredientAddedEventArgs : EventArgs {
        public KitchenObjectSO kitchenObjectSO;
    }


    [SerializeField] private PlateIngredientsSO plateIngredientsSO;

    private List<KitchenObjectSO> kitchenObjectSOList = new List<KitchenObjectSO>();
    

    public bool TryAddIngredient(KitchenObjectSO kitchenObjectSO)
    {
        if (!plateIngredientsSO.validKitchenObjectSOList.Contains(kitchenObjectSO)) {
            // Not a valid ingredient
            return false;
        }

        if (kitchenObjectSOList.Contains(kitchenObjectSO))
        {
            return false;
        }
        else
        {
            foreach (PlateIngredientsSO.BannedCombinations combination in plateIngredientsSO.bannedKitchenObjectCombinations) // Check every combination
            {
                int ingredientMatches = 0;
                foreach (KitchenObjectSO _kitchenObjectSO in combination.kitchenObjectSOs) // Check the ingredients of that combination
                {
                    foreach (KitchenObjectSO plateKitchenObjectSO in kitchenObjectSOList) // Check if any of the objects already on the plate are also part of that combination
                    {
                        if (plateKitchenObjectSO == _kitchenObjectSO) { // There is an ingredient on the plate that is in that combination; add a match
                            ingredientMatches++;
                        }
                    }
                    if (kitchenObjectSO == _kitchenObjectSO) { // Ingredient attempting to be added is part of that combination 
                        ingredientMatches++;
                    }
                }
                
                if (ingredientMatches > 1) { // There is at least one object already on the plate that is part of the attempted item's combination
                    return false;
                }
            }
        }

        AddIngredientServerRpc(GameMultiplayer.Instance.GetKitchenObjectSOIndex(kitchenObjectSO));

        return true;
    }

    [ServerRpc(RequireOwnership = false)]
    private void AddIngredientServerRpc(int kitchenObjectSOIndex)
    {
        AddIngredientClientRpc(kitchenObjectSOIndex);
    }

    [ClientRpc]
    private void AddIngredientClientRpc(int kitchenObjectSOIndex)
    {
        KitchenObjectSO kitchenObjectSO = GameMultiplayer.Instance.GetKitchenObjectSOFromIndex(kitchenObjectSOIndex);
        
        kitchenObjectSOList.Add(kitchenObjectSO);

        OnIngredientAdded?.Invoke(this, new OnIngredientAddedEventArgs {
            kitchenObjectSO = kitchenObjectSO
        });
    }

    public List<KitchenObjectSO> GetKitchenObjectSOList()
    {
        return kitchenObjectSOList;
    }

}
