using UnityEngine;

// Auto-paints the terrain's splatmap by slope: flat ground -> grass layer,
// steep ground -> rock layer (with a blend band). Create your grass + rock
// TerrainLayers first, then right-click the component -> "Auto Texture By Slope".
[RequireComponent(typeof(Terrain))]
public class TerrainAutoTexture : MonoBehaviour
{
    [Tooltip("TerrainLayer index for flat ground.")]
    [SerializeField] int grassLayer = 0;
    [Tooltip("TerrainLayer index for steep ground.")]
    [SerializeField] int rockLayer = 1;

    [Tooltip("Slope (degrees) where rock starts blending in.")]
    [SerializeField] float grassSlope = 18f;
    [Tooltip("Slope (degrees) where it's fully rock.")]
    [SerializeField] float rockSlope = 32f;

    [ContextMenu("Auto Texture By Slope")]
    public void Apply()
    {
        var data = GetComponent<Terrain>().terrainData;
        int w = data.alphamapWidth, h = data.alphamapHeight, layers = data.alphamapLayers;

        if (layers < 2)
        {
            Debug.LogWarning("Need at least 2 terrain layers (grass + rock). Create them first.");
            return;
        }

        var maps = new float[h, w, layers];

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float nx = (float)x / (w - 1);
                float ny = (float)y / (h - 1);
                float slope = data.GetSteepness(nx, ny);          // degrees
                float rock = Mathf.InverseLerp(grassSlope, rockSlope, slope); // 0..1

                for (int l = 0; l < layers; l++) maps[y, x, l] = 0f;
                maps[y, x, Mathf.Clamp(rockLayer, 0, layers - 1)] = rock;
                maps[y, x, Mathf.Clamp(grassLayer, 0, layers - 1)] += 1f - rock;
            }
        }

        data.SetAlphamaps(0, 0, maps);
        Debug.Log("Terrain auto-textured by slope.");
    }
}