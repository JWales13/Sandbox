using UnityEngine;

// One kind of item. Pure data — create via Create > Items > Item.
// Add new items as assets; no code changes needed.
[CreateAssetMenu(menuName = "Items/Item", fileName = "NewItem")]
public class ItemSO : ScriptableObject
{
    public string displayName = "New Item";
    [TextArea] public string description;
    public Sprite icon;              // optional; UI falls back to the name if empty
    public int maxStack = 99;
    public int sellPrice = 1;        // used by the Merchant system later
}