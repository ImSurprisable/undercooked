using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BaseCounter : NetworkBehaviour, IKitchenObjectParent
{

    public static event EventHandler OnAnyObjectPlacedHere;

    public static void ResetStaticData() {
        OnAnyObjectPlacedHere = null;
    }

    [SerializeField] private Transform counterTopPoint;
    [SerializeField] private Transform counterFrontPoint;
    private KitchenObject kitchenObject;

    
    public virtual void Interact(PlayerController player)
    {
        Debug.LogWarning("This counter does not support Interact();");
    }
    public virtual void InteractAlternate(PlayerController player)
    {
        Debug.LogWarning("This counter does not support InteractAlternate();");
    }

    public Transform GetKitchenObjectFollowTransform()
    {
        return counterTopPoint;
    }

    public void SetKitchenObject(KitchenObject kitchenObject, bool playSound = true)
    {
        this.kitchenObject = kitchenObject;

        if (kitchenObject != null && playSound)
        {
            OnAnyObjectPlacedHere?.Invoke(this, EventArgs.Empty);
        }
    }

    public KitchenObject GetKitchenObject()
    {
        return kitchenObject;
    }

    public void ClearKitchenObject()
    {
        kitchenObject = null;
    }

    public bool HasKitchenObject()
    {
        return kitchenObject != null;
    }

    public Transform GetCounterFrontPoint()
    {
        return counterFrontPoint;
    }
    
    public NetworkObject GetNetworkObject()
    {
        return NetworkObject;
    }
}
