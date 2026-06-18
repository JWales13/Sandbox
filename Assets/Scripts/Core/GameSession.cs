// Tiny cross-scene flag. Set by the main menu, read by SaveManager in the game scene.
public static class GameSession
{
    public static bool LoadOnStart;   // true = Continue (load save), false = New Game (fresh)
}