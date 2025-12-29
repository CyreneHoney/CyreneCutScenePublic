using CyreneCore.Utils;

namespace CyreneCore.Caption;

public class VideoData
{
    public int VideoID { get; set; }
    public bool IsPlayerInvolved { get; set; }
    public string VideoPath { get; set; } = "";
    public string CaptionPath { get; set; } = "";
}

public class TextMapData : Dictionary<int, string>;

public class CaptionData
{
    public List<CaptionEntry> CaptionList { get; set; } = [];
}

public class CaptionEntry
{
    public CaptionTextData CaptionTextID { get; set; } = new();
    public double StartTime { get; set; }
    public double EndTime { get; set; }

    private static string FormatTimestamp(double seconds)
    {
        var ts = TimeSpan.FromSeconds(seconds);
        return $"{ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2},{ts.Milliseconds:D3}"; 
    }

    public string GetStartTimestamp() => FormatTimestamp(StartTime);
    public string GetEndTimestamp() => FormatTimestamp(EndTime);
}

public class CaptionTextData
{
    public int Hash { get; set; }
}

public class CaptionCacheData
{
    public Dictionary<string, List<CaptionEntryCache>> CacheMap { get; set; } = [];
}

public class CaptionEntryCache
{
    public Dictionary<LanguageTypeEnum, string> Texts { get; set; } = [];
    public string StartTimestamp { get; set; } = "";
    public string EndTimestamp { get; set; } = "";
}