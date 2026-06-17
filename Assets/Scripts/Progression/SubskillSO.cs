using System.Collections.Generic;
using UnityEngine;

// A skill you level by doing (Cultivation, Ranching, Logging...).
// Create via Create > Skills > Subskill.
[CreateAssetMenu(menuName = "Skills/Subskill", fileName = "NewSubskill")]
public class SubskillSO : ScriptableObject
{
    public string displayName = "New Subskill";
    [TextArea] public string description;

    [Header("XP curve: xp to next level = baseXP * level^exponent")]
    public float baseXP = 100f;
    public float exponent = 1.5f;
    public int maxLevel = 100;

    [Tooltip("Perks belonging to this subskill.")]
    public List<SkillPerkSO> perks = new List<SkillPerkSO>();

    // XP required to advance FROM the given level to the next.
    public int XpForLevel(int level)
    {
        return Mathf.RoundToInt(baseXP * Mathf.Pow(Mathf.Max(1, level), exponent));
    }
}