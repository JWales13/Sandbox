using System.IO;
using UnityEngine;

// Saves/loads the game to a JSON file on disk.
//   F5 = save, F9 = load (configurable).
// Persists progression state + the player's position and facing.
public class SaveManager : MonoBehaviour
{
    public PlayerProgression progression;
    public Inventory inventory;
    public Transform player;
    public KeyCode saveKey = KeyCode.O;   // F-keys are intercepted by macOS
    public KeyCode loadKey = KeyCode.P;

    string FilePath => Path.Combine(Application.persistentDataPath, "save.json");

    void Update()
    {
        if (Input.GetKeyDown(saveKey)) Save();
        if (Input.GetKeyDown(loadKey)) Load();
    }

    public void Save()
    {
        var data = new GameSaveData();
        if (progression != null) data.progression = progression.CaptureState();
        if (inventory != null) data.inventory = inventory.CaptureState();
        if (player != null)
        {
            data.playerPos = player.position;
            data.playerEuler = player.eulerAngles;
        }

        File.WriteAllText(FilePath, JsonUtility.ToJson(data, true));
        Debug.Log($"Game saved -> {FilePath}");
    }

    public void Load()
    {
        if (!File.Exists(FilePath))
        {
            Debug.LogWarning("No save file found yet. Press save first.");
            return;
        }

        var data = JsonUtility.FromJson<GameSaveData>(File.ReadAllText(FilePath));
        if (progression != null) progression.RestoreState(data.progression);
        if (inventory != null) inventory.RestoreState(data.inventory);
        if (player != null) TeleportPlayer(data.playerPos, data.playerEuler);
        Debug.Log("Game loaded.");
    }

    // A CharacterController fights direct position changes, so disable it briefly.
    void TeleportPlayer(Vector3 pos, Vector3 euler)
    {
        var cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        player.position = pos;
        player.eulerAngles = new Vector3(0f, euler.y, 0f);
        if (cc != null) cc.enabled = true;
    }
}