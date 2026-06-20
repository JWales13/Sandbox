using UnityEngine;

// Procedurally shapes the attached Terrain: gentle layered-noise hills, with a
// flattened disc in the middle for the town. Tune the values, then right-click
// the component → "Generate Terrain". Editor-only helper (no runtime cost).
[RequireComponent(typeof(Terrain))]
public class TerrainGenerator : MonoBehaviour
{
    [Header("Hills")]
    [Tooltip("Bigger = broader, smoother hills.")]
    [SerializeField] float noiseScale = 4f;
    [Tooltip("Max hill height as a fraction of the terrain's height (0..1). Keep small for gentle.")]
    [SerializeField, Range(0f, 0.3f)] float heightAmount = 0.06f;
    [SerializeField, Range(1, 5)] int octaves = 3;
    [SerializeField] float seed = 0f;

    [Header("Flatten town area (normalized 0..1 across the terrain)")]
    [SerializeField] bool flattenCenter = true;
    [SerializeField] Vector2 flatCenter = new Vector2(0.5f, 0.5f);
    [SerializeField, Range(0f, 0.5f)] float flatRadius = 0.18f;
    [SerializeField, Range(0.01f, 0.3f)] float flatBlend = 0.12f;
    [Tooltip("Height of the town clearing as a fraction of terrain height. 0 = terrain base (≈ world y 0), so objects sitting near y=0 land on the surface.")]
    [SerializeField, Range(0f, 0.3f)] float townLevel = 0f;

    [ContextMenu("Generate Terrain")]
    public void Generate()
    {
        var data = GetComponent<Terrain>().terrainData;
        int res = data.heightmapResolution;
        var heights = new float[res, res];

        for (int y = 0; y < res; y++)
        {
            for (int x = 0; x < res; x++)
            {
                float nx = (float)x / res;
                float ny = (float)y / res;

                float h = Fractal(nx, ny) * heightAmount;

                if (flattenCenter)
                {
                    float d = Vector2.Distance(new Vector2(nx, ny), flatCenter);
                    float t = Mathf.InverseLerp(flatRadius, flatRadius + flatBlend, d); // 0 inside town, 1 outside
                    h = Mathf.Lerp(townLevel, h, t);
                }

                heights[y, x] = h;
            }
        }

        data.SetHeights(0, 0, heights);
        Debug.Log("Terrain generated.");
    }

    float Fractal(float x, float y)
    {
        float sum = 0f, freq = noiseScale, amp = 1f, max = 0f;
        for (int o = 0; o < octaves; o++)
        {
            sum += Mathf.PerlinNoise(x * freq + seed, y * freq + seed) * amp;
            max += amp;
            freq *= 2f;
            amp *= 0.5f;
        }
        return sum / max;
    }
}