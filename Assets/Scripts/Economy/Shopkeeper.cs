using System.Collections.Generic;
using UnityEngine;

// A merchant NPC. Interact to open the shop. Its "stock" is the list of items
// it sells (data assets you assign).
public class Shopkeeper : Interactable
{
    public string shopName = "Merchant";
    public List<ItemSO> stock = new List<ItemSO>();

    void Reset() { prompt = "shop"; }

    public override void Interact(GameObject interactor)
    {
        if (ShopUI.Instance != null) ShopUI.Instance.Open(this);
    }
}