using System;
using System.Collections.Generic;
using UnityEngine;

// The brain of the skill system. Every gameplay system grants progress with
// AddSubskillXP(...) and reads bonuses with GetStat(...) / GetAttribute(...).
// Perk points are pooled PER DISCIPLINE: points earned from a subskill can only
// buy perks within that subskill's discipline.
public class PlayerProgression : MonoBehaviour, ISaveable, IStatSource
{
    // Unlocked perks contribute their modifiers to the Stats pipeline.
    public void CollectModifiers(System.Collections.Generic.List<StatModifier> into)
    {
        foreach (var p in unlocked)
            foreach (var m in p.modifiers)
                into.Add(m);
    }

    public string SaveId => "progression";
    public string WriteState() => JsonUtility.ToJson(CaptureData());
    public void ReadState(string data) => RestoreData(JsonUtility.FromJson<ProgressionSaveData>(data));

    [Header("Content (assign all disciplines the player can train)")]
    public List<DisciplineSO> disciplines = new List<DisciplineSO>();

    [Header("Character level (1-100)")]
    public int maxCharacterLevel = 100;
    public float characterBaseXP = 200f;
    public float characterExponent = 1.4f;
    [Tooltip("Character XP per subskill level-up = this * the new subskill level.")]
    public float charXpPerSubskillLevel = 10f;

    // --- Live state (not Inspector-serialized; handled by the save system later) ---
    public int AttributePoints { get; private set; }
    public int CharacterLevel { get; private set; } = 1;
    public int CharacterXpIntoLevel { get; private set; }

    public static PlayerProgression Instance { get; private set; }

    public event Action OnChanged;   // fired whenever anything changes (for UI/HUD)

    readonly Dictionary<DisciplineSO, int> perkPoints = new Dictionary<DisciplineSO, int>();
    readonly Dictionary<SubskillSO, int> levels = new Dictionary<SubskillSO, int>();
    readonly Dictionary<SubskillSO, int> xpIntoLevel = new Dictionary<SubskillSO, int>();
    readonly HashSet<SkillPerkSO> unlocked = new HashSet<SkillPerkSO>();
    readonly Dictionary<AttributeType, int> attributes = new Dictionary<AttributeType, int>();
    readonly Dictionary<SkillPerkSO, SubskillSO> perkOwner = new Dictionary<SkillPerkSO, SubskillSO>();
    readonly Dictionary<SubskillSO, DisciplineSO> subskillDiscipline = new Dictionary<SubskillSO, DisciplineSO>();

    // Name lookups for save/load (assets keyed by their asset name).
    readonly Dictionary<string, DisciplineSO> disciplineByName = new Dictionary<string, DisciplineSO>();
    readonly Dictionary<string, SubskillSO> subskillByName = new Dictionary<string, SubskillSO>();
    readonly Dictionary<string, SkillPerkSO> perkByName = new Dictionary<string, SkillPerkSO>();

    void Awake()
    {
        Instance = this;

        foreach (var d in disciplines)
        {
            if (d == null) continue;
            if (!perkPoints.ContainsKey(d)) perkPoints[d] = 0;
            disciplineByName[d.name] = d;
            foreach (var s in d.subskills)
            {
                if (s == null) continue;
                subskillDiscipline[s] = d;
                subskillByName[s.name] = s;
                if (!levels.ContainsKey(s)) { levels[s] = 1; xpIntoLevel[s] = 0; }
                foreach (var p in s.perks)
                    if (p != null) { perkOwner[p] = s; perkByName[p.name] = p; }
            }
        }
    }

    // ---------- Save / load ----------

    public ProgressionSaveData CaptureData()
    {
        var data = new ProgressionSaveData
        {
            characterLevel = CharacterLevel,
            characterXpIntoLevel = CharacterXpIntoLevel,
            attributePoints = AttributePoints
        };
        foreach (var kv in attributes) data.attributes.Add(new IntEntry { key = (int)kv.Key, value = kv.Value });
        foreach (var kv in perkPoints) data.perkPoints.Add(new StringIntEntry { key = kv.Key.name, value = kv.Value });
        foreach (var kv in levels) data.subskillLevels.Add(new StringIntEntry { key = kv.Key.name, value = kv.Value });
        foreach (var kv in xpIntoLevel) data.subskillXp.Add(new StringIntEntry { key = kv.Key.name, value = kv.Value });
        foreach (var p in unlocked) data.unlockedPerks.Add(p.name);
        return data;
    }

    public void RestoreData(ProgressionSaveData data)
    {
        if (data == null) return;

        CharacterLevel = Mathf.Max(1, data.characterLevel);
        CharacterXpIntoLevel = data.characterXpIntoLevel;
        AttributePoints = data.attributePoints;

        attributes.Clear();
        foreach (var e in data.attributes) attributes[(AttributeType)e.key] = e.value;

        foreach (var key in new List<DisciplineSO>(perkPoints.Keys)) perkPoints[key] = 0;
        foreach (var e in data.perkPoints)
            if (disciplineByName.TryGetValue(e.key, out var d)) perkPoints[d] = e.value;

        foreach (var e in data.subskillLevels)
            if (subskillByName.TryGetValue(e.key, out var s)) levels[s] = e.value;
        foreach (var e in data.subskillXp)
            if (subskillByName.TryGetValue(e.key, out var s)) xpIntoLevel[s] = e.value;

        unlocked.Clear();
        foreach (var n in data.unlockedPerks)
            if (perkByName.TryGetValue(n, out var p)) unlocked.Add(p);

        OnChanged?.Invoke();
    }

    // ---------- XP / leveling ----------

    public void AddSubskillXP(SubskillSO s, int amount)
    {
        if (s == null || amount <= 0) return;
        if (!levels.ContainsKey(s)) { levels[s] = 1; xpIntoLevel[s] = 0; }

        xpIntoLevel[s] += amount;
        while (levels[s] < s.maxLevel && xpIntoLevel[s] >= s.XpForLevel(levels[s]))
        {
            xpIntoLevel[s] -= s.XpForLevel(levels[s]);
            levels[s]++;
            AwardPerkPoint(s);                                     // subskill level -> perk point (in its discipline)
            GainCharacterXP(Mathf.RoundToInt(charXpPerSubskillLevel * levels[s]));
        }
        OnChanged?.Invoke();
    }

    void AwardPerkPoint(SubskillSO s)
    {
        if (subskillDiscipline.TryGetValue(s, out var disc))
            perkPoints[disc] = GetPerkPoints(disc) + 1;
    }

    void GainCharacterXP(int amount)
    {
        if (CharacterLevel >= maxCharacterLevel) return;
        CharacterXpIntoLevel += amount;
        while (CharacterLevel < maxCharacterLevel && CharacterXpIntoLevel >= CharacterXpForNext())
        {
            CharacterXpIntoLevel -= CharacterXpForNext();
            CharacterLevel++;
            AttributePoints++;                                     // character level -> attribute point
        }
    }

    public int CharacterXpForNext() => Mathf.RoundToInt(characterBaseXP * Mathf.Pow(CharacterLevel, characterExponent));

    public int GetSubskillLevel(SubskillSO s) => s != null && levels.TryGetValue(s, out var l) ? l : 0;
    public int GetSubskillXp(SubskillSO s) => s != null && xpIntoLevel.TryGetValue(s, out var x) ? x : 0;
    public int GetSubskillXpForNext(SubskillSO s) => s != null ? s.XpForLevel(GetSubskillLevel(s)) : 0;

    // ---------- Perks (per-discipline point pools) ----------

    public int GetPerkPoints(DisciplineSO d) => d != null && perkPoints.TryGetValue(d, out var v) ? v : 0;
    public DisciplineSO GetPerkDiscipline(SkillPerkSO p) =>
        p != null && perkOwner.TryGetValue(p, out var s) && subskillDiscipline.TryGetValue(s, out var d) ? d : null;

    public bool IsUnlocked(SkillPerkSO p) => p != null && unlocked.Contains(p);

    public bool CanUnlock(SkillPerkSO p)
    {
        if (p == null || IsUnlocked(p)) return false;

        var disc = GetPerkDiscipline(p);
        if (disc == null || GetPerkPoints(disc) < p.cost) return false;

        if (perkOwner.TryGetValue(p, out var owner) && GetSubskillLevel(owner) < p.requiredSubskillLevel) return false;

        foreach (var pre in p.prerequisites)
            if (pre != null && !IsUnlocked(pre)) return false;

        return true;
    }

    public bool TryUnlock(SkillPerkSO p)
    {
        if (!CanUnlock(p)) return false;
        var disc = GetPerkDiscipline(p);
        unlocked.Add(p);
        perkPoints[disc] = GetPerkPoints(disc) - p.cost;
        OnChanged?.Invoke();
        return true;
    }

    // Sum of a stat across all unlocked perks. Systems call this for bonuses.
    public float GetStat(StatType stat)
    {
        float total = 0f;
        foreach (var p in unlocked)
            foreach (var m in p.modifiers)
                if (m.stat == stat) total += m.value;
        return total;
    }

    public bool HasTag(string tag)
    {
        foreach (var p in unlocked)
            if (p.unlockTags.Contains(tag)) return true;
        return false;
    }

    // ---------- Attributes (abstract numbers for now) ----------

    public int GetAttribute(AttributeType a) => attributes.TryGetValue(a, out var v) ? v : 0;

    public bool InvestAttribute(AttributeType a)
    {
        if (AttributePoints <= 0) return false;
        attributes[a] = GetAttribute(a) + 1;
        AttributePoints--;
        OnChanged?.Invoke();
        return true;
    }
}