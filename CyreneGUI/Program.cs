using System.CodeDom.Compiler;
using CyreneGUI.Utils;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using WinRT;

namespace CyreneGUI;

#if DISABLE_XAML_GENERATED_MAIN

public static class Program
{
    [GeneratedCode("Microsoft.UI.Xaml.Markup.Compiler", " 3.0.0.2411")]
    public static void Main()
    {
        ComWrappersSupport.InitializeComWrappers();
        Application.Start((_) =>
        {
            SynchronizationContext.SetSynchronizationContext(
                new DispatcherQueueSynchronizationContext(AppUtil.Queue));
            new App();
        });
    }
}

#endif