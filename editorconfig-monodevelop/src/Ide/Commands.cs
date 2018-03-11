using EditorConfig.Core;
using MonoDevelop.Components.Commands;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui;
using System.Linq;
using System.Text;

namespace EditorConfig.Addin
{
    public enum Commands
    {
        Reload,
        ReloadAll,
        Apply,
        ApplyAll,
        LetEolApply,
        ShowSettings,
    }

    class StartupHandler : CommandHandler
    {
        protected override void Run()
        {
            IdeEventMgr.Initialize();
            PropertyMgr.Initialize();
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

    class LetEolApplyHandler : CommandHandler
    {
        protected override void Update(CommandInfo info)
        {
            info.Checked = Engine.LetEolApply;
        }

        protected override void Run()
        {
            Engine.LetEolApply = !Engine.LetEolApply;

            if (PropertyMgr.Get() != null)
                PropertyMgr.Get().SaveLetEolApply();
        }
    }

    class ShowSettingsHandler : CommandHandler
    {
        protected override void Update(CommandInfo info)
        {
            Document doc = IdeApp.Workbench.ActiveDocument;

            info.Enabled = (doc != null && doc.Editor != null);
        }

        protected override void Run()
        {
            FileConfiguration config = Engine.ParseConfig(IdeApp.Workbench.ActiveDocument);

            StringBuilder builder = new StringBuilder();

            builder.AppendFormat($"EditorConfig settings for {config.FileName}:\n");
            foreach (var pair in config.Properties)
                builder.AppendFormat($"\n{pair.Key} = {pair.Value}");

            string text = builder.ToString();
            MessageService.ShowMessage(text);
        }
    }
}
