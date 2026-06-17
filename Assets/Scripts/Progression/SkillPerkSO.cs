using System.Collections.Generic;
using UnityEngine;

// One unlockable perk. Pure data — create these as assets via
// Create > Skills > Perk. No code changes needed to add new perks.
[CreateAssetMenu(menuName = "Skills/Perk", fileName = "NewPerk")]
public class SkillPerkSO : ScriptableObject
{
    public string displayName = "New Perk";
    [TextArea] public string description;

    [Tooltip("Where this node sits in the skill-tree UI (relative to the tree's center).")]
    public Vector2 treePosition;

    [Tooltip("Perk points required to unlock.")]
    public int cost = 1;

    [Tooltip("Minimum level in the owning subskill before this can be unlocked.")]
    public int requiredSubskillLevel = 0;

    [Tooltip("Other perks that must be unlocked first.")]
    public List<SkillPerkSO> prerequisites = new List<SkillPerkSO>();

    [Tooltip("Passive bonuses this perk grants while unlocked.")]
    public List<StatModifier> modifiers = new List<StatModifier>();

    [Tooltip("Named abilities/actions this perk unlocks (queried via HasTag).")]
    public List<string> unlockTags = new List<string>();
}

[System.Serializable]
public struct StatModifier
{
    public StatType stat;
    public float value;
}