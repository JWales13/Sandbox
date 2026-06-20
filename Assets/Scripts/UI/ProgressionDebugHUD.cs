using UnityEngine;

// Temporary debug tool: simulates "doing tasks" and shows the whole progression
// loop on screen, before any real farming exists. Delete once farming drives XP.
//   [T] gain XP in the test subskill   [U] unlock the test perk   [1-5] invest attribute points
public class ProgressionDebugHUD : MonoBehaviour
{
    public PlayerProgression progression;
    public SubskillSO testSubskill;   // e.g. Cultivation
    public SkillPerkSO testPerk;      // a perk to try unlocking
    public int xpPerPress = 40;

    void Update()
    {
        if (progression == null) return;

        if (Input.GetKeyDown(KeyCode.T)) progression.AddSubskillXP(testSubskill, xpPerPress);
        if (Input.GetKeyDown(KeyCode.U) && testPerk != null) progression.TryUnlock(testPerk);
        if (Input.GetKeyDown(KeyCode.Alpha1)) progression.InvestAttribute(AttributeType.Strength);
        if (Input.GetKeyDown(KeyCode.Alpha2)) progression.InvestAttribute(AttributeType.Intelligence);
        if (Input.GetKeyDown(KeyCode.Alpha3)) progression.InvestAttribute(AttributeType.Stamina);
        if (Input.GetKeyDown(KeyCode.Alpha4)) progression.InvestAttribute(AttributeType.Charm);
        if (Input.GetKeyDown(KeyCode.Alpha5)) progression.InvestAttribute(AttributeType.Luck);
    }

    void OnGUI()
    {
        if (progression == null) return;

        var style = new GUIStyle(GUI.skin.label) { fontSize = 15, richText = true };
        GUILayout.BeginArea(new Rect(10, 10, 470, 300), GUI.skin.box);
        GUILayout.Label("<b>PROGRESSION DEBUG</b>", style);
        GUILayout.Label($"Character Level: {progression.CharacterLevel}/100    " +
                        $"XP: {progression.CharacterXpIntoLevel}/{progression.CharacterXpForNext()}", style);
        GUILayout.Label($"Attribute Points: {progression.AttributePoints}", style);
        foreach (var d in progression.Disciplines)
            if (d != null)
                GUILayout.Label($"  {d.displayName} perk points: {progression.GetPerkPoints(d)}", style);
        GUILayout.Label($"STR {progression.GetAttribute(AttributeType.Strength)}   " +
                        $"INT {progression.GetAttribute(AttributeType.Intelligence)}   " +
                        $"STA {progression.GetAttribute(AttributeType.Stamina)}   " +
                        $"CHA {progression.GetAttribute(AttributeType.Charm)}   " +
                        $"LCK {progression.GetAttribute(AttributeType.Luck)}", style);

        if (testSubskill != null)
            GUILayout.Label($"{testSubskill.displayName}: Lv {progression.GetSubskillLevel(testSubskill)}   " +
                            $"XP {progression.GetSubskillXp(testSubskill)}/{progression.GetSubskillXpForNext(testSubskill)}", style);

        if (testPerk != null)
        {
            string state = progression.IsUnlocked(testPerk) ? "<color=lime>UNLOCKED</color>"
                         : progression.CanUnlock(testPerk) ? "<color=yellow>available — press U</color>"
                         : "<color=grey>locked</color>";
            GUILayout.Label($"Perk '{testPerk.displayName}': {state}", style);
        }

        GUILayout.Space(6);
        GUILayout.Label("<b>[T]</b> gain XP    <b>[U]</b> unlock perk    <b>[1-5]</b> invest attribute", style);
        GUILayout.EndArea();
    }
}