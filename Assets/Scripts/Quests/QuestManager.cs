using System;
using System.Collections.Generic;
using UnityEngine;

// The brain of the quest system. Tracks active/completed quests, advances
// objectives from gameplay (talk / collect / defeat), grants rewards, and
// persists via ISaveable. Adding a new quest = a new QuestSO asset; no changes
// here. New objective types = one new case in the relevant handler.
public class QuestManager : MonoBehaviour, ISaveable
{
    public static QuestManager Instance { get; private set; }
    public event Action OnChanged;

    [Tooltip("Every quest in the game. Needed to resolve quests by id when loading a save (and to auto-start intro quests).")]
    [SerializeField] List<QuestSO> allQuests = new List<QuestSO>();

    // Runtime record of a quest in progress.
    class Tracked
    {
        public QuestSO def;
        public int[] progress;
        public Tracked(QuestSO d) { def = d; progress = new int[d.objectives.Count]; }
    }

    readonly List<Tracked> active = new List<Tracked>();
    readonly HashSet<string> completed = new HashSet<string>();
    readonly Dictionary<string, QuestSO> byId = new Dictionary<string, QuestSO>();

    void Awake()
    {
        Instance = this;
        foreach (var q in allQuests)
            if (q != null && !byId.ContainsKey(q.questId)) byId[q.questId] = q;
    }

    void Start()
    {
        if (Inventory.Instance != null) Inventory.Instance.OnChanged += RecountItems;

        // Auto-start intro quests for a fresh game. If a save loads a moment
        // later, RestoreData overwrites this with the saved state.
        foreach (var q in allQuests)
            if (q != null && q.autoStart && IsAvailable(q)) Accept(q);
    }

    void OnEnable() { GameEvents.EnemyKilled += OnEnemyKilled; }
    void OnDisable() { GameEvents.EnemyKilled -= OnEnemyKilled; }
    void OnDestroy() { if (Inventory.Instance != null) Inventory.Instance.OnChanged -= RecountItems; }

    // ---------- Queries (used by UI) ----------

    public bool IsCompleted(QuestSO q) => q != null && completed.Contains(q.questId);
    public bool IsActive(QuestSO q) => FindActive(q) != null;

    public bool IsAvailable(QuestSO q)
    {
        if (q == null || IsActive(q) || IsCompleted(q)) return false;
        return q.requiredQuest == null || completed.Contains(q.requiredQuest.questId);
    }

    public IEnumerable<QuestSO> ActiveQuests
    {
        get { foreach (var t in active) yield return t.def; }
    }

    public int GetObjectiveProgress(QuestSO q, int i)
    {
        var t = FindActive(q);
        return (t != null && i >= 0 && i < t.progress.Length) ? t.progress[i] : 0;
    }

    public bool IsReadyToTurnIn(QuestSO q)
    {
        var t = FindActive(q);
        return t != null && AllComplete(t) && !string.IsNullOrEmpty(q.turnInNpcId);
    }

    Tracked FindActive(QuestSO q)
    {
        if (q == null) return null;
        foreach (var t in active) if (t.def == q) return t;
        return null;
    }

    // ---------- Accept ----------

    public bool Accept(QuestSO q)
    {
        if (!IsAvailable(q)) return false;
        active.Add(new Tracked(q));
        RecountItems();              // a collect goal might already be satisfied
        OnChanged?.Invoke();
        return true;
    }

    // ---------- Progress sources ----------

    // Called by QuestGiver when the player talks to a quest NPC. Advances any
    // "talk to X" objective and turns in any quest waiting to be handed in here.
    // Returns true if talking changed/completed anything (so the giver can pick a line).
    public bool NotifyNpcTalked(string npcId)
    {
        if (string.IsNullOrEmpty(npcId)) return false;

        bool changed = false;
        foreach (var t in active)
        {
            var objs = t.def.objectives;
            for (int i = 0; i < objs.Count; i++)
                if (objs[i].type == ObjectiveType.TalkToNpc && objs[i].targetId == npcId
                    && t.progress[i] < Required(objs[i]))
                { t.progress[i] = Required(objs[i]); changed = true; }
        }

        bool turnedIn = Resolve(npcId);
        if (changed || turnedIn) OnChanged?.Invoke();
        return changed || turnedIn;
    }

    void OnEnemyKilled(string enemyId)
    {
        bool changed = false;
        foreach (var t in active)
        {
            var objs = t.def.objectives;
            for (int i = 0; i < objs.Count; i++)
                if (objs[i].type == ObjectiveType.DefeatEnemy && objs[i].targetId == enemyId
                    && t.progress[i] < Required(objs[i]))
                { t.progress[i]++; changed = true; }
        }
        if (changed) { Resolve(null); OnChanged?.Invoke(); }
    }

    // Collect objectives mirror the live inventory count, so picking up or losing
    // items keeps progress honest. Hooked to Inventory.OnChanged.
    void RecountItems()
    {
        if (Inventory.Instance == null) return;

        bool changed = false;
        foreach (var t in active)
        {
            var objs = t.def.objectives;
            for (int i = 0; i < objs.Count; i++)
                if (objs[i].type == ObjectiveType.CollectItem && objs[i].targetItem != null)
                {
                    int c = Mathf.Min(Inventory.Instance.CountOf(objs[i].targetItem), Required(objs[i]));
                    if (t.progress[i] != c) { t.progress[i] = c; changed = true; }
                }
        }
        if (changed) { Resolve(null); OnChanged?.Invoke(); }
    }

    // ---------- Completion ----------

    // Completes/turns in any active quest whose objectives are all met.
    // talkedNpcId != null means we just spoke to that NPC (enables turn-ins there).
    bool Resolve(string talkedNpcId)
    {
        List<Tracked> done = null;
        for (int k = active.Count - 1; k >= 0; k--)
        {
            var t = active[k];
            if (!AllComplete(t)) continue;

            bool needsTurnIn = !string.IsNullOrEmpty(t.def.turnInNpcId);
            if (needsTurnIn && t.def.turnInNpcId != talkedNpcId) continue;   // wait at the giver

            active.RemoveAt(k);
            completed.Add(t.def.questId);
            (done ??= new List<Tracked>()).Add(t);
        }

        // Apply consumption + rewards AFTER updating active/completed. The
        // Inventory changes these trigger (-> OnChanged -> RecountItems -> Resolve)
        // then can't re-process a quest that's already finished.
        if (done != null)
            foreach (var t in done)
            {
                ConsumeObjectiveItems(t.def);
                GrantRewards(t.def);
            }
        return done != null;
    }

    // Removes the gathered items on hand-in for CollectItem objectives that opt in.
    void ConsumeObjectiveItems(QuestSO q)
    {
        if (Inventory.Instance == null) return;
        foreach (var o in q.objectives)
            if (o.type == ObjectiveType.CollectItem && o.consumeOnTurnIn && o.targetItem != null)
                Inventory.Instance.Remove(o.targetItem, Mathf.Max(1, o.requiredAmount));
    }

    bool AllComplete(Tracked t)
    {
        var objs = t.def.objectives;
        for (int i = 0; i < objs.Count; i++)
            if (t.progress[i] < Required(objs[i])) return false;
        return true;
    }

    static int Required(QuestObjective o) => Mathf.Max(1, o.requiredAmount);

    void GrantRewards(QuestSO q)
    {
        if (q.rewardCoins > 0 && Wallet.Instance != null) Wallet.Instance.Add(q.rewardCoins);

        if (q.rewardSubskill != null && q.rewardXp > 0 && PlayerProgression.Instance != null)
            PlayerProgression.Instance.AddSubskillXP(q.rewardSubskill, q.rewardXp);

        if (Inventory.Instance != null)
            foreach (var r in q.rewardItems)
                if (r != null && r.item != null) Inventory.Instance.Add(r.item, Mathf.Max(1, r.amount));

        Debug.Log($"Quest complete: {q.title}");
    }

    // ---------- Save / load ----------

    public string SaveId => "quests";
    public string WriteState() => JsonUtility.ToJson(CaptureData());
    public void ReadState(string data) => RestoreData(JsonUtility.FromJson<QuestSaveData>(data));

    QuestSaveData CaptureData()
    {
        var d = new QuestSaveData();
        foreach (var id in completed) d.completed.Add(id);
        foreach (var t in active)
        {
            var a = new ActiveQuestData { questId = t.def.questId };
            foreach (var p in t.progress) a.progress.Add(p);
            d.active.Add(a);
        }
        return d;
    }

    void RestoreData(QuestSaveData d)
    {
        if (d == null) return;

        completed.Clear();
        active.Clear();

        foreach (var id in d.completed) completed.Add(id);

        foreach (var a in d.active)
            if (byId.TryGetValue(a.questId, out var q))
            {
                var t = new Tracked(q);
                for (int i = 0; i < t.progress.Length && i < a.progress.Count; i++)
                    t.progress[i] = a.progress[i];
                active.Add(t);
            }

        RecountItems();   // resync collect goals to the just-loaded inventory
        OnChanged?.Invoke();
    }
}