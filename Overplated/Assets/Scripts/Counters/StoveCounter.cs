using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class StoveCounter : BaseCounter, IHasProgress
{
    public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;

    public event EventHandler<OnStateChangedEventArgs> OnStateChanged;
    public class OnStateChangedEventArgs : EventArgs {
        public State state;
    }

    public enum State { Idle, Cooking, Cooked, Burnt }

    [SerializeField] private CookingRecipeSO[] cookingRecipeSOs;
    [SerializeField] private BurningRecipeSO[] burningRecipeSOs;
    [SerializeField] private float kitchenObjectSizeDecrease;


    private NetworkVariable<State> state = new NetworkVariable<State>(State.Idle);
    private NetworkVariable<float> cookingProgress = new NetworkVariable<float>(0f);
    private NetworkVariable<float> burningProgress = new NetworkVariable<float>(0f);
    private CookingRecipeSO cookingRecipeSO;
    private BurningRecipeSO burningRecipeSO;
    private KitchenObject kitchenObject;

    private Vector3 tmpKitchenObjectLocalScale;




    public override void OnNetworkSpawn()
    {
        cookingProgress.OnValueChanged += CookingProgress_OnValueChanged;
        burningProgress.OnValueChanged += BurningProgress_OnValueChanged;
        state.OnValueChanged += State_OnValueChanged;
    }

    private void State_OnValueChanged(State previousState, State newState)
    {
        OnStateChanged?.Invoke(this, new OnStateChangedEventArgs {
            state = state.Value
        });
    }

    private void CookingProgress_OnValueChanged(float previousValue, float newValue)
    {
        float cookingTimerMax = cookingRecipeSO != null  ?  cookingRecipeSO.cookingTimerMax : 1f;
        InvokeProgressBarChanged(cookingProgress.Value / cookingTimerMax);
    }

    private void BurningProgress_OnValueChanged(float previousValue, float newValue)
    {
        float burningTimerMax = burningRecipeSO != null  ?  burningRecipeSO.burningTimerMax : 1f;
        InvokeProgressBarChanged(burningProgress.Value / burningTimerMax);
    }

    private void Update()
    {
        if (!IsServer) return;

        if (HasKitchenObject())
        switch (state.Value) {
            case State.Idle:
                break;
            case State.Cooking:
                if (cookingRecipeSO != null) {
                    cookingProgress.Value += cookingProgress.Value < cookingRecipeSO.cookingTimerMax  ?  Time.deltaTime : 0f;

                    if (cookingProgress.Value > cookingRecipeSO.cookingTimerMax) 
                    {
                        KitchenObject.DestroyKitchenObject(GetKitchenObject());

                        KitchenObject.SpawnKitchenObject(cookingRecipeSO.recipeOutput, this);
                        kitchenObject = GetKitchenObject();

                        // StoreAndReplaceObjectScale(kitchenObject);

                        UpdateKitchenRecipeSOsClientRpc(GameMultiplayer.Instance.GetKitchenObjectSOIndex(kitchenObject.GetKitchenObjectSO()));

                        ResetProgressValues();
                        state.Value = State.Cooked;
                    }
                }
                break;
            case State.Cooked:
                if (burningRecipeSO != null)
                {
                    burningProgress.Value += burningProgress.Value < burningRecipeSO.burningTimerMax  ?  Time.deltaTime : 0f;

                    if (burningProgress.Value > burningRecipeSO.burningTimerMax) 
                    {
                        KitchenObject.DestroyKitchenObject(GetKitchenObject());

                        KitchenObject.SpawnKitchenObject(burningRecipeSO.recipeOutput, this);
                        kitchenObject = GetKitchenObject();

                        // StoreAndReplaceObjectScale(kitchenObject);

                        ResetProgressValues();
                        state.Value = State.Burnt;
                    }
                }
                break;
            case State.Burnt:
                break;
        }
    }


    public override void Interact(PlayerController player)
    {
        if (!HasKitchenObject())
        {
            // No kitchen object
            if (player.HasKitchenObject() && !player.GetKitchenObject().TryGetPlate(out _))
            {
                // Player is carrying a non-plate object
                kitchenObject = player.GetKitchenObject();
                kitchenObject.SetKitchenObjectParent(this);
                // StoreAndReplaceObjectScale(kitchenObject);

                InteractLogicPlaceObjectOnCounterServerRpc(GameMultiplayer.Instance.GetKitchenObjectSOIndex(kitchenObject.GetKitchenObjectSO()));
            }
        }
        else
        {
            // Has a kitchen object
            if (player.HasKitchenObject() && player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject))
            {
                // Player is holding a plate
                if (plateKitchenObject.TryAddIngredient(GetKitchenObject().GetKitchenObjectSO())) {
                    KitchenObject.DestroyKitchenObject(GetKitchenObject());

                    SetStateServerRpc(State.Idle);
                    ResetProgressValuesServerRpc();
                }
            }
            else if (player.HasKitchenObject() && player.CanSwapItemsOnCounters())
            {
                // Player is holding a non-plate object & swapping is enabled
                SwapItemsServerRpc(player.NetworkObject);
            }
            else if (!player.HasKitchenObject())
            {
                // Player is not holding an object
                // GetKitchenObject().transform.localScale = tmpKitchenObjectLocalScale;
                GetKitchenObject().SetKitchenObjectParent(player);

                SetStateServerRpc(State.Idle);
                ResetProgressValuesServerRpc();
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetStateServerRpc(State newStateValue)
    {
        state.Value = newStateValue;
    }
    [ServerRpc(RequireOwnership = false)]

    private void ResetProgressValuesServerRpc()
    {
        ResetProgressValues();
    }

    [ServerRpc(RequireOwnership = false)]
    private void InteractLogicPlaceObjectOnCounterServerRpc(int kitchenObjectSOIndex)
    {
        UpdateKitchenRecipeSOsClientRpc(kitchenObjectSOIndex);

        KitchenObjectSO kitchenObjectSO = GameMultiplayer.Instance.GetKitchenObjectSOFromIndex(kitchenObjectSOIndex);
        if (BurningRecipeSOWithOutputExists(kitchenObjectSO)) {
            state.Value = State.Burnt;
        } else if (cookingRecipeSO != null) {
            state.Value = State.Cooking;
        } else if (burningRecipeSO != null) {
            state.Value = State.Cooked;
        }

        ResetProgressValues();
    }

    [ClientRpc]
    private void UpdateKitchenRecipeSOsClientRpc(int kitchenObjectSOIndex)
    {
        KitchenObjectSO kitchenObjectSO = GameMultiplayer.Instance.GetKitchenObjectSOFromIndex(kitchenObjectSOIndex);
        UpdateKitchenRecipeSOs(kitchenObjectSO);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SwapItemsServerRpc(NetworkObjectReference playersNetworkObjectReference)
    {
        SwapItemsClientRpc(playersNetworkObjectReference);

        InteractLogicPlaceObjectOnCounterServerRpc(GameMultiplayer.Instance.GetKitchenObjectSOIndex(GetKitchenObject().GetKitchenObjectSO()));
    }

    [ClientRpc]
    private void SwapItemsClientRpc(NetworkObjectReference playersNetworkObjectReference)
    {
        playersNetworkObjectReference.TryGet(out NetworkObject playerNetworkObject);
        PlayerController player = playerNetworkObject.GetComponent<PlayerController>();

        KitchenObject playersKitchenObject = player.GetKitchenObject();
        // GetKitchenObject().transform.localScale = tmpKitchenObjectLocalScale;
        player.GetKitchenObject().ClearKitchenObjectFromParent(player);
        GetKitchenObject().SetKitchenObjectParent(player);
        playersKitchenObject.SetKitchenObjectParent(this);
        // StoreAndReplaceObjectScale(GetKitchenObject());
    }

    private KitchenObjectSO GetOutputForInput(KitchenObjectSO inputKitchenObjectSO)
    {
        CookingRecipeSO cookingRecipeSO = GetCookingRecipeSOWithInput(inputKitchenObjectSO);

        if (cookingRecipeSO != null) {
            return cookingRecipeSO.recipeOutput;
        } else {
            return null;
        }
    }

    private CookingRecipeSO GetCookingRecipeSOWithInput(KitchenObjectSO inputKitchenObjectSO)
    {
        foreach(CookingRecipeSO cookingRecipeSO in cookingRecipeSOs)    
        {
            if (cookingRecipeSO.recipeInput == inputKitchenObjectSO) {
                return cookingRecipeSO;
            }
        }
        return null;
    }
    private BurningRecipeSO GetBurningRecipeSOWithInput(KitchenObjectSO inputKitchenObjectSO)
    {
        foreach(BurningRecipeSO burningRecipeSO in burningRecipeSOs)    
        {
            if (burningRecipeSO.recipeInput == inputKitchenObjectSO) {
                return burningRecipeSO;
            }
        }
        return null;
    }

    private bool BurningRecipeSOWithOutputExists(KitchenObjectSO outputKitchenObjectSO)
    {
        foreach(BurningRecipeSO burningRecipeSO in burningRecipeSOs)    
        {
            if (burningRecipeSO.recipeOutput == outputKitchenObjectSO) {
                return true;
            }
        }
        return false;
    }

    private void StoreAndReplaceObjectScale(KitchenObject kitchenObject)
    {
        tmpKitchenObjectLocalScale = kitchenObject.transform.localScale;
        kitchenObject.transform.localScale *= kitchenObjectSizeDecrease;
    }

    private void UpdateKitchenRecipeSOs(KitchenObjectSO kitchenObjectSO)
    {
        cookingRecipeSO = GetCookingRecipeSOWithInput(kitchenObjectSO);
        burningRecipeSO = GetBurningRecipeSOWithInput(kitchenObjectSO);
    }

    private void ResetProgressValues()
    {
        cookingProgress.Value = 0f;
        burningProgress.Value = 0f;
    }

    private void InvokeProgressBarChanged(float _progressNormalized)
    {
        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs {
            progressNormalized = _progressNormalized
        });
    }

    public bool IsStoveBurning()
    {
        return state.Value == State.Cooked;
    }

}
