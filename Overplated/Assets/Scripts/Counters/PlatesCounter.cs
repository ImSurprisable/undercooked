using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlatesCounter : BaseCounter
{

    public event EventHandler OnPlateSpawned;
    public event EventHandler OnPlateRemoved;

    [SerializeField] private KitchenObjectSO plateKitchenObjectSO;
    [SerializeField] private PlateIngredientsSO plateIngredientsSO;

    [SerializeField] private float spawnPlateTimerCooldown;
    private float spawnPlateTimer;
    private int platesSpawnedAmount;
    [SerializeField] int platesSpawnedAmountMax;
    [SerializeField] float firstPlateSpawnedMultiplier;



    private void Start()
    {
        spawnPlateTimer = spawnPlateTimerCooldown * firstPlateSpawnedMultiplier; // THIS IS A SET VALUE... modify the first plate spawn time
    }

    private void Update()
    {
        if (!IsServer) return;

        spawnPlateTimer -= (spawnPlateTimer > 0f && GameManager.Instance.IsGamePlaying())  ?  Time.deltaTime : 0f;

        if (spawnPlateTimer <= 0f)
        {
            if (platesSpawnedAmount < platesSpawnedAmountMax)
            {
                SpawnPlateServerRpc();
            }

            spawnPlateTimer = spawnPlateTimerCooldown;
        }
    }

    [ServerRpc]
    private void SpawnPlateServerRpc()
    {
        SpawnPlateClientRpc();
    }

    [ClientRpc]
    private void SpawnPlateClientRpc()
    {
        platesSpawnedAmount++;
        OnPlateSpawned?.Invoke(this, EventArgs.Empty);
    }

    public override void Interact(PlayerController player)
    {
        if (!player.HasKitchenObject())
        {
            // Player is empty handed
            if (platesSpawnedAmount > 0)
            {
                // There is a plate
                KitchenObject.SpawnKitchenObject(plateKitchenObjectSO, player);

                InteractLogicServerRpc();
            }
        }
        /* MECHANIC REMOVED OUT UNTIL PLATE INGREDIENT SYNC LOGIC IS ADDED
        else if (PlayerIngredientIsValid(player.GetKitchenObject().GetKitchenObjectSO()))
        {
            InteractPlateLogicServerRpc(player.GetNetworkObject());

            InteractLogicServerRpc();
        }
        */
    }

    [ServerRpc(RequireOwnership = false)]
    private void InteractLogicServerRpc()
    {
        InteractLogicClientRpc();
    }

    [ClientRpc]
    private void InteractLogicClientRpc()
    {
        platesSpawnedAmount--;
        OnPlateRemoved?.Invoke(this, EventArgs.Empty);
    }

    [ServerRpc(RequireOwnership = false)]
    private void InteractPlateLogicServerRpc(NetworkObjectReference playerNetworkObjectReference)
    {
        InteractPlateLogicClientRpc(playerNetworkObjectReference);
    }

    [ClientRpc]
    private void InteractPlateLogicClientRpc(NetworkObjectReference playerNetworkObjectReference)
    {
        playerNetworkObjectReference.TryGet(out NetworkObject playerNetworkObject);
        IKitchenObjectParent player = playerNetworkObject.GetComponent<IKitchenObjectParent>();

        KitchenObject.SpawnKitchenObject(plateKitchenObjectSO, this);
        GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject);

        StartCoroutine(DelayInvoke(plateKitchenObject, player.GetKitchenObject()));
        player.GetKitchenObject().DestroySelf();

        plateKitchenObject.SetKitchenObjectParent(player);
    }


    IEnumerator DelayInvoke(PlateKitchenObject plateKitchenObject, KitchenObject ingredientKitchenObject)
    {
        yield return new WaitForEndOfFrame();
        plateKitchenObject.TryAddIngredient(ingredientKitchenObject.GetKitchenObjectSO());
    }

    public bool PlayerIngredientIsValid(KitchenObjectSO kitchenObjectSO)
    {
        return plateIngredientsSO.validKitchenObjectSOList.Contains(kitchenObjectSO);

    }
}
