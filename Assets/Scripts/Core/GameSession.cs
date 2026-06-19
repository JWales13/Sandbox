// Tiny cross-scene state. Set by the main menu, read by SaveManager in the game scene.
public static class GameSession
{
    public static bool LoadOnStart;   // true = load the save on entering the game scene
    public static int CurrentSlot;    // which save slot we're playing (0..SaveSlots.Count-1)
}