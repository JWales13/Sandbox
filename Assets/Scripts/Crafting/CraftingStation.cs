using System.Collections.Generic;
using UnityEngine;

// A crafting station (workbench, cooking pot, forge...). Interact to open the
// crafting menu showing the recipes this station can make.
public class CraftingStation : Interactable
{
    public string stationName = "Workbench";
    public List<RecipeSO> recipes = new List<RecipeSO>();

    void Reset() { prompt = "craft"; }

    public override void Interact(GameObject interactor)
    {
        if (CraftingUI.Instance != null) CraftingUI.Instance.Open(this);
    }
}