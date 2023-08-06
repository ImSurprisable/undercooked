using System;
using System.Collections;
using System.Collections.Generic;
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
            kitchenObjectSOList.Add(kitchenObjectSO);

            OnIngredientAdded?.Invoke(this, new OnIngredientAddedEventArgs {
                kitchenObjectSO = kitchenObjectSO
            });

            return true;
        }
    }
    
    public List<KitchenObjectSO> GetKitchenObjectSOList()
    {
        return kitchenObjectSOList;
    }

}
