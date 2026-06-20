using System.Collections.Generic;
using UnityEngine;

// A crafting station (workbench, cooking pot, forge...). Interact to open the
// crafting menu showing the recipes this station can make.
public class CraftingStation : Interactable
{
    [SerializeField] string stationName = "Workbench";
    [SerializeField] List<RecipeSO> recipes = new List<RecipeSO>();

    public string StationName => stationName;
    public IReadOnlyList<RecipeSO> Recipes => recipes;

    void Reset() { prompt = "craft"; }

    public override void Interact(GameObject interactor)
    {
        if (CraftingUI.Instance != null) CraftingUI.Instance.Open(this);
    }
}