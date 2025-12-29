using I18N.DotNet;
using Microsoft.UI.Xaml.Markup;

namespace CyreneGUI.Utils;

[MarkupExtensionReturnType(ReturnType = typeof(string))]
public sealed partial class TrExtension : MarkupExtension
{
    public string K { get; set; } = "";
    protected override object ProvideValue() => GlobalLocalizer.Localize(K);
}
