using EditorConfig.Core;
using MonoDevelop.Ide.Commands;
using MonoDevelop.Components.Commands;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EditorConfig.Addin
{
    public enum EditCommands
    {
        ApplyEditorConfig,
    }

    class ApplyEditorConfigHandler : CommandHandler
    {
        protected override void Update(CommandInfo info)
        {
            Document doc = IdeApp.Workbench.ActiveDocument;

            info.Enabled = ShouldBeEnabled(doc);
        }

        protected override void Run()
        {
            Document doc = IdeApp.Workbench.ActiveDocument;

            doc.Editor.InsertAtCaret("foo");

            string fileName = doc.FileName.FileName;

            EditorConfigParser parser = new EditorConfigParser();
            IEnumerable<FileConfiguration> configs = parser.Parse(fileName);
            FileConfiguration config = configs.First();
            ApplyEditorConfigToDoc(doc, config);
        }

        private bool ShouldBeEnabled(Document doc)
        {
            if (doc == null)
                return false;

            if (doc.Editor == null)
                return false;

            if (!doc.IsFile)
                return false;

            return true;
        }

        // support:
        // indent_style
        // indent_size
        // end_of_line
        // trim_trailing_whitespace
        // insert_final_newline
        private void ApplyEditorConfigToDoc(Document doc, FileConfiguration config)
        {
            if (config.Properties.Count == 0)
            {
                Log.ShowError(Log.Target.StatusBar | Log.Target.Dialog,
                              "Failed to parse an editorconfig for this file");
                return;
            }
        }
    }
}
