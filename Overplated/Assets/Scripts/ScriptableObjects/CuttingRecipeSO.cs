using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class CuttingRecipeSO : ScriptableObject
{
    public KitchenObjectSO recipeInput;
    public KitchenObjectSO recipeOutput;
    public int cuttingProgressMax;
}
