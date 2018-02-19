using EditorConfig.Core;
using MonoDevelop.Components.Commands;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui;

namespace EditorConfig.Addin
{
    public enum Commands
    {
        Reload,
        Apply,
        LetEolChange,
    }

    class StartupHandler : CommandHandler
    {
        protected override void Run()
        {
            CommandHooks.Initialize();
        }
    }

    class LoadSettingsHandler : CommandHandler
    {
        protected override void Update(CommandInfo info)
        {
            Document doc = IdeApp.Workbench.ActiveDocument;

            info.Enabled = (doc != null && doc.Editor != null);
        }

        protected override void Run()
        {
            Document doc = IdeApp.Workbench.ActiveDocument;

            Engine.LoadSettings(doc);
        }
    }

    class ApplyHandler : CommandHandler
    {
        protected override void Update(CommandInfo info)
        {
            Document doc = IdeApp.Workbench.ActiveDocument;

            info.Enabled = (doc != null && doc.Editor != null);
        }

        protected override void Run()
        {
            Document doc = IdeApp.Workbench.ActiveDocument;

            FileConfiguration config = Engine.ParseConfig(doc);
            Engine.LoadSettings(doc, config);
            Engine.Apply(doc, config);
        }
    }

    class LetEolChangeHandler : CommandHandler
    {
        protected override void Update(CommandInfo info)
        {
            info.Checked = Engine.LetEolApply;
        }

        protected override void Run()
        {
            Engine.LetEolApply = !Engine.LetEolApply;
        }
    }
}
