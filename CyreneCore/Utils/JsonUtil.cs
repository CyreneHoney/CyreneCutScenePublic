using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CyreneCore.Utils;

public class JsonUtil
{
    public static readonly JsonSerializerSettings Settings = new()
    {
        Formatting = Formatting.Indented,
        TypeNameHandling = TypeNameHandling.Auto,
        ObjectCreationHandling = ObjectCreationHandling.Replace,
        Converters = { new StringEnumConverter() }
    };

    public static T? DeserializeObject<T>(string path)
    {
        string text;
        var file = new FileInfo(path);
        using (var reader = new StreamReader(file.OpenRead())) { text = reader.ReadToEnd(); }

        return JsonConvert.DeserializeObject<T>(text, Settings);
    }

    public static T? DeserializeObject<T>(Stream stream)
    {
        string text;
        using (var reader = new StreamReader(stream)) { text = reader.ReadToEnd(); }

        return JsonConvert.DeserializeObject<T>(text, Settings);
    }
}