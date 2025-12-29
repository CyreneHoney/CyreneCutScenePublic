using CyreneCore.Utils;

namespace CyreneGUI.Utils;

public class AppConfig : CoreConfig
{
    public UIData UI { get; set; } = new();
}

public class UIData
{
    public bool SkipSplash { get; set; }
    public int Volume { get; set; } = 50;
    public PuzzleState Puzzle { get; set; }
}