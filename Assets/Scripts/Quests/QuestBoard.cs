using System.Collections.Generic;
using UnityEngine;

// A notice board the player reads to pick up side quests. It just holds the
// postings; the QuestBoardUI shows the ones that are currently available.
public class QuestBoard : Interactable
{
    [SerializeField] string boardName = "Quest Board";
    [SerializeField] List<QuestSO> postings = new List<QuestSO>();

    public string BoardName => boardName;
    public IReadOnlyList<QuestSO> Postings => postings;

    void Reset() { prompt = "read"; }

    public override void Interact(GameObject interactor)
    {
        if (QuestBoardUI.Instance != null) QuestBoardUI.Instance.Open(this);
    }
}