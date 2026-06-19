// Implement this on any component that should be saved. SaveManager finds all
// ISaveables automatically — no edits to SaveManager when you add a new one.
//
//   SaveId      : a unique, STABLE key (don't change it once saves exist).
//   WriteState(): return this component's state as a string (usually JSON).
//   ReadState() : restore from a string produced by WriteState().
public interface ISaveable
{
    string SaveId { get; }
    string WriteState();
    void ReadState(string data);
}