using System.IO;
using UnityEngine;

// Owns save-slot file paths and quick metadata. Used by both the main menu
// (to list slots) and SaveManager (to read/write the current slot).
public static class SaveSlots
{
    public const int Count = 3;

    public static string PathFor(int slot) =>
        Path.Combine(Application.persistentDataPath, $"save_{slot}.json");

    public static bool Exists(int slot) => File.Exists(PathFor(slot));

    public static void Delete(int slot)
    {
        if (Exists(slot)) File.Delete(PathFor(slot));
    }

    // Short description for the slot list: "Level N" if there's a save, else "Empty".
    public static string Summary(int slot)
    {
        if (!Exists(slot)) return "Empty";
        try
        {
            var file = JsonUtility.FromJson<SaveFile>(File.ReadAllText(PathFor(slot)));
            foreach (var e in file.entries)
                if (e.id == "progression")
                {
                    var p = JsonUtility.FromJson<ProgressionSaveData>(e.json);
                    return $"Level {p.characterLevel}";
                }
            return "Saved";
        }
        catch { return "Saved"; }
    }
}