using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class BurningRecipeSO : ScriptableObject
{
    public KitchenObjectSO recipeInput;
    public KitchenObjectSO recipeOutput;
    public float burningTimerMax;
}
