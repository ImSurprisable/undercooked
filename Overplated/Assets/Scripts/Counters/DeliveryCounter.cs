using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeliveryCounter : BaseCounter
{

    public static DeliveryCounter Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public override void Interact(PlayerController player)
    {
        if (player.HasKitchenObject())
        {
            // Player has an object
            if (player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject))
            {
                // Player specifically has a plate

                DeliveryManager.Instance.DeliverRecipe(plateKitchenObject);

                KitchenObject.DestroyKitchenObject(plateKitchenObject);
            }
        }
    }

    
}
