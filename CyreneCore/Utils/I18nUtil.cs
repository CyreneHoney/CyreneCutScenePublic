using System.Reflection;
using I18N.DotNet;

namespace CyreneCore.Utils;

public static class I18NUtil
{
    public static void Init(LanguageTypeEnum lang)
    {
        var assembly = Assembly.GetExecutingAssembly();
        GlobalLocalizer.Localizer.LoadXML(assembly, CoreConst.TransPath, lang.ToString());
    }
}