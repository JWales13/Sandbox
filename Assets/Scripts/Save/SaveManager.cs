using System.Collections;
using System.IO;
using UnityEngine;

// Saves/loads the game to a JSON file on disk.
//   F5 = save, F9 = load (configurable).
// Persists progression state + the player's position and facing.
public class SaveManager : MonoBehaviour
{
    public PlayerProgression progression;
    public Inventory inventory;
    public PlayerHealth playerHealth;
    public Wallet wallet;
    public Transform player;

    string FilePath => Path.Combine(Application.persistentDataPath, "save.json");

    void Start()
    {
        // If the menu chose "Continue", load the save once everything has initialized.
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

    public void Save()
    {
        var data = new GameSaveData();
        if (progression != null) data.progression = progression.CaptureState();
        if (inventory != null) data.inventory = inventory.CaptureState();
        if (playerHealth != null) data.playerCurrentHealth = playerHealth.CurrentHealth;
        if (wallet != null) data.coins = wallet.coins;
        if (player != null)
        {
            data.playerPos = player.position;
            data.playerEuler = player.eulerAngles;
        }

        data.farmPlots.Clear();
        foreach (var plot in FindObjectsByType<FarmPlot>(FindObjectsSortMode.None))
            data.farmPlots.Add(new FarmPlotSaveData
            {
                key = plot.SaveKey,
                state = plot.StateIndex,
                growTimer = plot.GrowTimer
            });

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

        // After progression restore (so MaxHealth is correct), apply saved HP.
        if (playerHealth != null) playerHealth.SetCurrent(data.playerCurrentHealth);
        if (wallet != null && data.coins >= 0) wallet.SetCoins(data.coins);

        var plotMap = new System.Collections.Generic.Dictionary<string, FarmPlotSaveData>();
        foreach (var pd in data.farmPlots) plotMap[pd.key] = pd;
        foreach (var plot in FindObjectsByType<FarmPlot>(FindObjectsSortMode.None))
            if (plotMap.TryGetValue(plot.SaveKey, out var pd))
                plot.RestoreState(pd.state, pd.growTimer);
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