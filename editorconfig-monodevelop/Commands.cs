using EditorConfig.Core;
using MonoDevelop.Components.Commands;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui;

namespace EditorConfig.Addin
{
    public enum Commands
    {
        ApplyEditorConfig,
    }

    class ApplyEditorConfigHandler : CommandHandler
    {
        protected override void Update(CommandInfo info)
        {
            Document doc = IdeApp.Workbench.ActiveDocument;

            info.Enabled = Engine.IsFile(doc);
        }

        protected override void Run()
        {
            Document doc = IdeApp.Workbench.ActiveDocument;

            Engine.ApplyEditorConfig(doc);
        }
    }

    class StartupHandler : CommandHandler
    {
        protected override void Run()
        {
            CommandHooks.Initialize();
        }
    }
}
