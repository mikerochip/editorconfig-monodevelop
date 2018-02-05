using MonoDevelop.Core;
using MonoDevelop.Ide;
using System;

namespace EditorConfig.Addin
{
    static class Log
    {
        [Flags]
        public enum Target
        {
            Invalid = 0,
            StatusBar = 1,
            Dialog = 2,
            Console = 4,
        }

        public static void ShowError(Target target, string format, params object[] args)
        {
            bool statusBar = (target & Target.StatusBar) != 0;
            bool dialog = (target & Target.Dialog) != 0;
            bool console = (target & Target.Console) != 0;

            if (!statusBar && !dialog && !console)
                return;

            string message = string.Format(format, args);

            if (statusBar)
                IdeApp.Workbench.StatusBar.ShowError(message);

            // MessageService errors implicitly go to the LoggingService
            if (dialog)
                MessageService.ShowError(message);
            else if (console)
                LoggingService.LogError(message);
        }
    }
}
