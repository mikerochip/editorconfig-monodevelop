using MonoDevelop.Components.Commands;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui;
using System.Linq;

namespace EditorConfig.Addin
{
    public enum Commands
    {
        Reload,
        ReloadAll,
        Apply,
        ApplyAll,
        LetEolChange,
    }

    class StartupHandler : CommandHandler
    {
        protected override void Run()
        {
            IdeEventMgr.Initialize();
        }
    }

    class ReloadHandler : CommandHandler
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

            Engine.LoadSettingsAndApply(doc);
        }
    }

    class ReloadAllHandler : CommandHandler
    {
        protected override void Update(CommandInfo info)
        {
            info.Enabled = IdeApp.Workbench.Documents.Any(
                (Document doc) => doc.Editor != null
            );
        }

        protected override void Run()
        {
            Engine.LoadSettings(IdeApp.Workbench.Documents);
        }
    }

    class ApplyAllHandler : CommandHandler
    {
        protected override void Update(CommandInfo info)
        {
            info.Enabled = IdeApp.Workbench.Documents.Any(
                (Document doc) => doc.Editor != null
            );
        }

        protected override void Run()
        {
            Engine.LoadSettingsAndApply(IdeApp.Workbench.Documents);
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
