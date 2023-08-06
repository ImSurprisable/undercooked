using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
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


    [SerializeField] private State state;
    private float cookingProgress = 0f;
    private float burningProgress = 0f;
    private CookingRecipeSO cookingRecipeSO;
    private BurningRecipeSO burningRecipeSO;

    private Vector3 tmpKitchenObjectLocalScale;



    private void Start()
    {
        state = State.Idle;
    }

    private void Update()
    {
        if (HasKitchenObject())
        switch (state) {
            case State.Idle:
                break;
            case State.Cooking:
                if (cookingRecipeSO != null) {
                    cookingProgress += cookingProgress < cookingRecipeSO.cookingTimerMax  ?  Time.deltaTime : 0f;

                    InvokeProgressBarChanged(cookingProgress / cookingRecipeSO.cookingTimerMax);

                    if (cookingProgress > cookingRecipeSO.cookingTimerMax) 
                    {
                        GetKitchenObject().DestroySelf();
                        KitchenObject.SpawnKitchenObject(cookingRecipeSO.recipeOutput, this);
                        StoreAndReplaceObjectScale(GetKitchenObject());

                        UpdateKitchenRecipeSOs();

                        state = State.Cooked;
                        InvokeStateChanged();
                    }
                }
                break;
            case State.Cooked:
                if (burningRecipeSO != null)
                {
                    burningProgress += burningProgress < burningRecipeSO.burningTimerMax  ?  Time.deltaTime : 0f;

                    InvokeProgressBarChanged(burningProgress / burningRecipeSO.burningTimerMax);

                    if (burningProgress > burningRecipeSO.burningTimerMax) 
                    {
                        GetKitchenObject().DestroySelf();
                        KitchenObject.SpawnKitchenObject(burningRecipeSO.recipeOutput, this);
                        StoreAndReplaceObjectScale(GetKitchenObject());

                        state = State.Burnt;
                        InvokeStateChanged();
                        InvokeProgressBarChanged(0f);
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
                player.GetKitchenObject().SetKitchenObjectParent(this);
                StoreAndReplaceObjectScale(GetKitchenObject());

                UpdateKitchenRecipeSOs();

                if (BurningRecipeSOWithOutputExists(GetKitchenObject().GetKitchenObjectSO())) {
                    state = State.Burnt;
                    InvokeProgressBarChanged(0f);
                } else if (cookingRecipeSO != null) {
                    state = State.Cooking;
                } else if (burningRecipeSO != null) {
                    state = State.Cooked;
                }
                InvokeStateChanged();
                
            }
        }
        else
        {
            // Has a kitchen object
            if (player.HasKitchenObject() && player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject))
            {
                // Player is holding a plate
                if (plateKitchenObject.TryAddIngredient(GetKitchenObject().GetKitchenObjectSO())) {
                GetKitchenObject().DestroySelf();

                state = State.Idle;
                InvokeStateChanged();
                InvokeProgressBarChanged(0f);
                }
            }
            else if (player.HasKitchenObject() && player.CanSwapItemsOnCounters())
            {
                // Player is holding a non-plate object & swapping is enabled
                KitchenObject playersKitchenObject = player.GetKitchenObject();
                GetKitchenObject().transform.localScale = tmpKitchenObjectLocalScale;
                player.GetKitchenObject().ClearKitchenObjectFromParent(player);
                GetKitchenObject().SetKitchenObjectParent(player);
                playersKitchenObject.SetKitchenObjectParent(this);
                StoreAndReplaceObjectScale(GetKitchenObject());

                UpdateKitchenRecipeSOs();

                if (BurningRecipeSOWithOutputExists(GetKitchenObject().GetKitchenObjectSO())) {
                    state = State.Burnt;
                    InvokeProgressBarChanged(0f);
                } else if (cookingRecipeSO == null && burningRecipeSO == null) {
                    state = State.Idle;
                    InvokeProgressBarChanged(0f);
                } else if (cookingRecipeSO != null) {
                    state = State.Cooking;
                } else {
                    state = State.Cooked;
                }
                InvokeStateChanged();
            }
            else if (!player.HasKitchenObject())
            {
                // Player is not holding an object
                GetKitchenObject().transform.localScale = tmpKitchenObjectLocalScale;
                GetKitchenObject().SetKitchenObjectParent(player);

                state = State.Idle;
                InvokeStateChanged();
                InvokeProgressBarChanged(0f);
            }
        }
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

    private void UpdateKitchenRecipeSOs()
    {
        cookingRecipeSO = GetCookingRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());
        burningRecipeSO = GetBurningRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());
    }

    private void InvokeStateChanged()
    {
        cookingProgress = 0f;
        burningProgress = 0f;

        OnStateChanged?.Invoke(this, new OnStateChangedEventArgs {
            state = state
        });
    }

    private void InvokeProgressBarChanged(float _progressNormalized)
    {
        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs {
            progressNormalized = _progressNormalized
        });
    }

    public bool IsStoveBurning()
    {
        return state == State.Cooked;
    }

}
