using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

// Generic save/load. Finds every ISaveable in the scene and writes an id -> json
// blob. Adding a new saveable system requires NO changes here — just implement
// ISaveable on the component.
public class SaveManager : MonoBehaviour
{
    string FilePath => Path.Combine(Application.persistentDataPath, "save.json");

    void Start()
    {
        if (GameSession.LoadOnStart)
        {
            GameSession.LoadOnStart = false;
            StartCoroutine(LoadNextFrame());
        }
    }

    IEnumerator LoadNextFrame()
    {
        yield return null;   // let all other Start()/Awake() run first
        Load();
    }

    static IEnumerable<ISaveable> FindSaveables()
    {
        return FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<ISaveable>();
    }

    public void Save()
    {
        var file = new SaveFile();
        foreach (var s in FindSaveables())
            file.entries.Add(new SaveEntry { id = s.SaveId, json = s.WriteState() });

        File.WriteAllText(FilePath, JsonUtility.ToJson(file, true));
        Debug.Log($"Game saved ({file.entries.Count} objects) -> {FilePath}");
    }

    public void Load()
    {
        if (!File.Exists(FilePath))
        {
            Debug.LogWarning("No save file found yet.");
            return;
        }

        var file = JsonUtility.FromJson<SaveFile>(File.ReadAllText(FilePath));
        var map = new Dictionary<string, string>();
        foreach (var e in file.entries) map[e.id] = e.json;

        // Progression first, so MaxHealth is recalculated before health/other systems load.
        foreach (var s in FindSaveables().OrderBy(s => s.SaveId == "progression" ? 0 : 1))
            if (map.TryGetValue(s.SaveId, out var json))
                s.ReadState(json);

        Debug.Log("Game loaded.");
    }
}