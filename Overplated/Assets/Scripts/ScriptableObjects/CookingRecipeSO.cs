using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class CookingRecipeSO : ScriptableObject
{
    public KitchenObjectSO recipeInput;
    public KitchenObjectSO recipeOutput;
    public float cookingTimerMax;
}
