namespace CyreneGUI.Utils;

public enum PuzzleState
{
    None,
    SYALocked,
    SYTLocked,
    SYAUnlocked,
    SYTUnlocked,
    SYAFinished
}

public static class PuzzleStateExtensions
{
    public static bool IsLocked(this PuzzleState state)
    {
        return state == PuzzleState.SYALocked || state == PuzzleState.SYTLocked;
    }

    public static bool IsUnlocked(this PuzzleState state)
    {
        return state == PuzzleState.SYAUnlocked || state == PuzzleState.SYTUnlocked || state == PuzzleState.SYAFinished;
    }

    public static PuzzleState GetUnlockState(this PuzzleState state)
    {
        if (!state.IsLocked()) return state;
        return state == PuzzleState.SYALocked ? PuzzleState.SYAUnlocked : PuzzleState.SYTUnlocked;
    }

    public static List<string> GetLetter(this PuzzleState state)
    {
        if (!state.IsUnlocked()) return [];
        return state == PuzzleState.SYTUnlocked ? AppConst.SYTLetter : AppConst.SYALetter;
    }
}