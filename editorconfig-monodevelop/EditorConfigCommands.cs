using EditorConfig.Core;
using MonoDevelop.Components.Commands;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Editor;
using MonoDevelop.Ide.Gui;
using System.Collections.Generic;
using System.Linq;

namespace EditorConfig.Addin
{
    public enum Commands
    {
        ExecuteEditorConfig,
    }

    class ExecuteEditorConfigCommandHandler : CommandHandler
    {
        protected override void Update(CommandInfo info)
        {
            Document activeDocument = IdeApp.Workbench.ActiveDocument;

            info.Enabled = ShouldBeEnabled(activeDocument);
        }

        protected override void Run()
        {
            Document activeDocument = IdeApp.Workbench.ActiveDocument;

            string date = System.DateTime.Now.ToString();
            activeDocument.Editor.InsertAtCaret(date);

            string fileName = activeDocument.FileName.FileName;

            EditorConfigParser parser = new EditorConfigParser();
            FileConfiguration config = parser.Parse(fileName).First();
            ChangeDocUsingConfig(activeDocument, config);
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
        private void ChangeDocUsingConfig(Document doc, FileConfiguration config)
        {
            if (config == null)
            {
                // how the f do you raise errors?
                MessageService.ShowError("Failed to find an editorconfig for this file");
                return;
            }
        }
    }
}
