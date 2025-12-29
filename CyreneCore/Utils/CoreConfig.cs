namespace CyreneCore.Utils;

public class CoreConfig
{
    public PathData Path { get; set; } = new();
    public ExtractData Extract { get; set; } = new();
    public LanguageTypeEnum Language { get; set; } = LanguageTypeEnum.CHS;
}

public class PathData
{
    public string KeyPath { get; set; } = "VideoKeys.json";
    public string FFmpegPath { get; set; } = "FFmpeg";
    public string ResPath { get; set; } = "Resources";
    public string LogPath { get; set; } = "Logs";
}

public class ExtractData
{
    public List<string> Input { get; set; } = [];
    public string Output { get; set; } = "Video";
    public bool Convert { get; set; } = true;
    public bool MergeAudio { get; set; } = true;
    public bool MergeCaption { get; set; } = true;
}