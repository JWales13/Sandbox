using System.Collections.Generic;
using UnityEngine;

// One crafting recipe. Create via Create > Crafting > Recipe.
[CreateAssetMenu(menuName = "Crafting/Recipe", fileName = "NewRecipe")]
public class RecipeSO : ScriptableObject
{
    public string displayName = "New Recipe";

    public List<ItemAmount> inputs = new List<ItemAmount>();
    public ItemSO output;
    public int outputAmount = 1;

    [Header("Reward / gating")]
    public SubskillSO subskill;          // e.g. a Crafting subskill (Cooking, Smithing)
    public int xpReward = 20;
    public int requiredSubskillLevel = 0;
}

[System.Serializable]
public struct ItemAmount
{
    public ItemSO item;
    public int amount;
}