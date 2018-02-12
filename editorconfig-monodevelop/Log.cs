using MonoDevelop.Core;
using MonoDevelop.Ide;
using System;

namespace EditorConfig.Addin
{
    static class Log
    {
        public enum Level
        {
            Info,
            Warning,
            Error,
        }

        [Flags]
        public enum Target
        {
            Invalid = 0,
            StatusBar = 1,
            Dialog = 2,
            Console = 4,
        }

        public static void Info(Target target, string format, params object[] args)
        {
            Show(Level.Info, target, format, args);
        }

        public static void Warning(Target target, string format, params object[] args)
        {
            Show(Level.Warning, target, format, args);
        }

        public static void Error(Target target, string format, params object[] args)
        {
            Show(Level.Error, target, format, args);
        }

        public static void Show(Level level, Target target, string format, params object[] args)
        {
            bool statusBar = (target & Target.StatusBar) != 0;
            bool dialog = (target & Target.Dialog) != 0;
            bool console = (target & Target.Console) != 0;

            if (!statusBar && !dialog && !console)
                return;

            string message = string.Format(format, args);

            if (statusBar)
            {
                switch (level)
                {
                    case Level.Info:
                        IdeApp.Workbench.StatusBar.ShowMessage(message);
                        break;

                    case Level.Warning:
                        IdeApp.Workbench.StatusBar.ShowWarning(message);
                        break;

                    case Level.Error:
                        IdeApp.Workbench.StatusBar.ShowError(message);
                        break;
                }
            }

            // MessageService errors implicitly go to the LoggingService
            if (dialog)
            {
                switch (level)
                {
                    case Level.Info:
                        MessageService.ShowMessage(message);
                        break;

                    case Level.Warning:
                        MessageService.ShowWarning(message);
                        break;

                    case Level.Error:
                        MessageService.ShowError(message);
                        break;
                }
            }
            else if (console)
            {
                switch (level)
                {
                    case Level.Info:
                        LoggingService.LogInfo(message);
                        break;

                    case Level.Warning:
                        LoggingService.LogWarning(message);
                        break;

                    case Level.Error:
                        LoggingService.LogError(message);
                        break;
                }
            }
        }
    }
}
