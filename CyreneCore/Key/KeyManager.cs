using System.Reflection;
using CyreneCore.Utils;
using static I18N.DotNet.GlobalLocalizer;

namespace CyreneCore.Key;

public static class KeyManager
{
    public static Dictionary<string, ulong> Keys { get; set; } = [];

    public static bool LoadKeyData(string keyPath)
    {
        Stream? stream;
        if (File.Exists(keyPath))
            stream = File.OpenRead(keyPath);
        else
            stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(CoreConst.KeyResPath);

        if (stream == null)
        {
            Logger.Error(LocalizeFormat("FileNotFoundErr", "VideoKey"));
            return false;
        }

        try
        {
            Keys = JsonUtil.DeserializeObject<Dictionary<string, ulong>>(stream) ?? [];
            if (Keys.Count == 0) throw new InvalidDataException("Key is empty.");
        }
        catch (Exception ex)
        {
            Logger.Error(LocalizeFormat("DeserializeErr", "VideoKey", ex.Message));
            return false;
        }

        return true;
    }

    public static ulong? GetKey(string fileName)
    {
        if (!Keys.TryGetValue(fileName, out var key))
        {
            Logger.Warn(LocalizeFormat("KeyNotFoundErr", fileName));
            return null;
        }

        return key;
    }
}
