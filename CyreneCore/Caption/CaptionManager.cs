using System.Text;
using Newtonsoft.Json;
using CyreneCore.Utils;
using static I18N.DotNet.GlobalLocalizer;

namespace CyreneCore.Caption;

public class CaptionManager
{
    private const string CachePath = "Caption.cache";
    private const string GuideVideoPath = "ExcelOutput/GuideVideoConfig.json";
    private const string LoopCGPath = "ExcelOutput/LoopCGConfig.json";
    private const string VideoPath = "ExcelOutput/VideoConfig.json";
    private const string TextMapPath = "TextMap";
    public static CaptionCacheData Cache { get; set; } = new();

    public static bool LoadCaptionData(string resPath)
    {
        var cachePath = Path.Combine(resPath, CachePath);
        if (!File.Exists(cachePath))
        {
            if (!LoadFromFile(resPath))
            {
                Logger.Error(Localize("InitCapDataErr"));
                return false;
            }

            try
            {
                var data = JsonConvert.SerializeObject(Cache);
                File.WriteAllText(cachePath, data, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Logger.Error(LocalizeFormat("SaveCapCacheErr", ex.Message));
                return false;
            }
        }

        try
        {
            Cache = JsonUtil.DeserializeObject<CaptionCacheData>(cachePath) ?? new();
        }
        catch (Exception ex)
        {
            Logger.Error(LocalizeFormat("LoadCapCacheErr", ex.Message));
            return false;
        }

        return true;
    }

    private static bool LoadFromFile(string resPath)
    {
        #region Excel

        var guidePath = Path.Combine(resPath, GuideVideoPath);
        var loopCGPath = Path.Combine(resPath, LoopCGPath);
        var videoPath = Path.Combine(resPath, VideoPath);
        if (!File.Exists(guidePath) || !File.Exists(loopCGPath) || !File.Exists(videoPath))
        {
            Logger.Error(LocalizeFormat("FileNotFoundErr", $"{guidePath}\n{loopCGPath}\n{videoPath}"));
            return false;
        }

        var data = new List<VideoData>();
        try
        {
            data.AddRange(JsonUtil.DeserializeObject<List<VideoData>>(guidePath) ?? []);
            data.AddRange(JsonUtil.DeserializeObject<List<VideoData>>(loopCGPath) ?? []);
            data.AddRange(JsonUtil.DeserializeObject<List<VideoData>>(videoPath) ?? []);
        }
        catch (Exception ex)
        {
            Logger.Error(LocalizeFormat("DeserializeErr", "Excel", ex.Message));
            return false;
        }

        #endregion

        #region TextMap

        var textMapPath = Path.Combine(resPath, TextMapPath);
        if (!Directory.Exists(textMapPath))
        {
            Logger.Error(LocalizeFormat("FileNotFoundErr", textMapPath));
            return false;
        }

        var textMapData = new Dictionary<LanguageTypeEnum, TextMapData>();
        foreach (var path in Directory.GetFiles(textMapPath).Where(f => !f.Contains("Main")))
        {
            var lang = Path.GetFileNameWithoutExtension(path).Replace("TextMap", "");
            if (!Enum.TryParse<LanguageTypeEnum>(lang, out var type))
            {
                Logger.Error(LocalizeFormat("TextMapLangErr", path));
                return false;
            }

            try
            {
                textMapData.Add(type, JsonUtil.DeserializeObject<TextMapData>(path) ?? []);
            }
            catch (Exception ex)
            {
                Logger.Error(LocalizeFormat("DeserializeErr", "TextMap", ex.Message));
                return false;
            }
        }

        #endregion

        #region Caption

        foreach (var excel in data)
        {
            if (string.IsNullOrEmpty(excel.CaptionPath)) continue;

            var fullPath = Path.Combine(resPath, excel.CaptionPath);
            if (!File.Exists(fullPath))
            {
                Logger.Error(LocalizeFormat("FileNotFoundErr", fullPath));
                return false;
            }

            CaptionData captionData;
            try
            {
                captionData = JsonUtil.DeserializeObject<CaptionData>(fullPath) ?? new();
            }
            catch (Exception ex)
            {
                Logger.Error(LocalizeFormat("DeserializeErr", "Caption", ex.Message));
                return false;
            }

            var entries = new List<CaptionEntryCache>();
            foreach (var caption in captionData.CaptionList)
            {
                var texts = new Dictionary<LanguageTypeEnum, string>();
                foreach (var textMap in textMapData)
                {
                    textMap.Value.TryGetValue(caption.CaptionTextID.Hash, out var text);
                    texts.Add(textMap.Key, text ?? "");
                }

                entries.Add(new CaptionEntryCache
                {
                    Texts = texts,
                    StartTimestamp = caption.GetStartTimestamp(),
                    EndTimestamp = caption.GetEndTimestamp()
                });
            }

            var rawName = excel.VideoPath.Replace(".usm", "");
            if (!excel.IsPlayerInvolved)
                Cache.CacheMap.Add(rawName, entries);
            else
            {
                Cache.CacheMap.Add(rawName + "_m", entries);
                Cache.CacheMap.Add(rawName + "_f", entries);
            }
        }

        #endregion

        return true;
    }

    public static void GenCaption(string fileName, string outputDir)
    {
        if (!Cache.CacheMap.TryGetValue(fileName, out var data) || data.Count == 0) return;

        foreach (var lang in data[0].Texts.Keys)
        {
            var srtPath = Path.Combine(outputDir, $"{fileName}_{lang}.srt");

            try
            {
                using var writer = new StreamWriter(srtPath);
                for (var j = 0; j < data.Count; j++)
                {
                    var caption = data[j];
                    if (!caption.Texts.TryGetValue(lang, out var text)) text = "";

                    writer.WriteLine(j + 1);
                    writer.WriteLine($"{caption.StartTimestamp} --> {caption.EndTimestamp}");
                    writer.WriteLine(text);
                    writer.WriteLine();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(LocalizeFormat("GenCapErr", srtPath, ex.Message));
            }
        }
    }
}