using System;
using System.Collections;
using System.Collections.Generic;
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
        spawnPlateTimer -= (spawnPlateTimer > 0f && GameManager.Instance.IsGamePlaying())  ?  Time.deltaTime : 0f;

        if (spawnPlateTimer <= 0f)
        {
            if (platesSpawnedAmount < platesSpawnedAmountMax)
            {
                platesSpawnedAmount++;
                OnPlateSpawned?.Invoke(this, EventArgs.Empty);
            }

            spawnPlateTimer = spawnPlateTimerCooldown;
        }
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

                platesSpawnedAmount--;
                OnPlateRemoved?.Invoke(this, EventArgs.Empty);
            }
        }
        else if (PlayerIngredientIsValid(player.GetKitchenObject().GetKitchenObjectSO()))
        {
            KitchenObject.SpawnKitchenObject(plateKitchenObjectSO, this);
            GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject);

            StartCoroutine(DelayInvoke(plateKitchenObject, player.GetKitchenObject()));
            player.GetKitchenObject().DestroySelf();

            plateKitchenObject.SetKitchenObjectParent(player);

            platesSpawnedAmount--;
            OnPlateRemoved?.Invoke(this, EventArgs.Empty);
        }
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
