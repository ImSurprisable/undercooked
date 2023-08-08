using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.Mathematics;
using UnityEngine;

public class ClearCounter : BaseCounter
{
    public override void Interact(PlayerController player)
    {
        if (!HasKitchenObject())
        {
            // Counter does not have a kitchen object
            if (player.HasKitchenObject())
            {
                // Player is carrying something
                player.GetKitchenObject().SetKitchenObjectParent(this);
            }
        }
        else
        {
            // Counter has a kitchen object
            if (player.HasKitchenObject() && player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject) && TryAddIngredientToPlate(plateKitchenObject, this))
            {
                // Player has a plate & the counter has a valid ingredient
                KitchenObject.DestroyKitchenObject(GetKitchenObject());
            }
            else if (GetKitchenObject().TryGetPlate(out PlateKitchenObject counterPlateKitchenObject) && TryAddIngredientToPlate(counterPlateKitchenObject, player))
            {
                // Counter has a plate & the player has a valid ingredient
                KitchenObject.DestroyKitchenObject(player.GetKitchenObject());
            }
            /* DISABLED UNTIL SWITCHING IS FIXED
            else if (player.HasKitchenObject() && player.CanSwapItemsOnCounters())
            {
                // Player is holding a non-plate object & swapping is allowed
                KitchenObject playersKitchenObject = player.GetKitchenObject();
                player.GetKitchenObject().ClearKitchenObjectFromParent(player);
                GetKitchenObject().SetKitchenObjectParent(player);
                playersKitchenObject.SetKitchenObjectParent(this);
            }
            */
            else if (!player.HasKitchenObject())
            {
                // Player is not holding anything
                GetKitchenObject().SetKitchenObjectParent(player);
            }
        }
    }

    private bool TryAddIngredientToPlate(PlateKitchenObject plateKitchenObject, IKitchenObjectParent kitchenObjectParent)
    {
        if (plateKitchenObject == null)
            return false;
        else
        {   if (kitchenObjectParent.GetKitchenObject() == null)
                return false;

            if (plateKitchenObject.TryAddIngredient(kitchenObjectParent.GetKitchenObject().GetKitchenObjectSO())) {
                return true;
            }
            else 
                return false;
        }
    }
}