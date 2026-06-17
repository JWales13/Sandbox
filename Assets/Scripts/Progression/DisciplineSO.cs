using System.Collections.Generic;
using UnityEngine;

// A top-level identity that groups subskills (Agriculture, Adventuring,
// Crafting, Scholar, Merchant). Create via Create > Skills > Discipline.
[CreateAssetMenu(menuName = "Skills/Discipline", fileName = "NewDiscipline")]
public class DisciplineSO : ScriptableObject
{
    public string displayName = "New Discipline";
    [TextArea] public string description;

    public List<SubskillSO> subskills = new List<SubskillSO>();
}