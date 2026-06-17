using UnityEngine;

// Defines one crop type. Create via Create > Farming > Crop.
[CreateAssetMenu(menuName = "Farming/Crop", fileName = "NewCrop")]
public class CropDataSO : ScriptableObject
{
    public string displayName = "New Crop";

    [Header("Items")]
    public ItemSO seedItem;        // consumed to plant
    public ItemSO produceItem;     // received on harvest
    public int produceAmount = 2;

    [Header("Growth")]
    public float growthSeconds = 10f;
    public Vector3 sproutScale = new Vector3(0.2f, 0.2f, 0.2f);
    public Vector3 fullScale = Vector3.one;

    [Header("Reward")]
    public SubskillSO subskill;    // e.g. Cultivation
    public int xpOnHarvest = 40;
}