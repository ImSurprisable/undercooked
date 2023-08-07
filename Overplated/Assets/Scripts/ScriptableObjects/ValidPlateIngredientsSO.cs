using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class PlateIngredientsSO : ScriptableObject
{
    public List<KitchenObjectSO> validKitchenObjectSOList;

    [Serializable]
    public class BannedCombinations {
        public List<KitchenObjectSO> kitchenObjectSOs;
    }

    public List<BannedCombinations> bannedKitchenObjectCombinations;
}
