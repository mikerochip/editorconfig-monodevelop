using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Projects;
using System;

namespace EditorConfig.Addin
{
    class IdeEventMgr
    {
        static IdeEventMgr instance;


        public static void Initialize()
        {
            if (instance != null)
                return;

            instance = new IdeEventMgr();
            instance.InitializeImpl();
        }

        public static IdeEventMgr Get()
        {
            return instance;
        }


        void InitializeImpl()
        {
            IdeApp.Workbench.DocumentOpened += OnDocumentOpened;
            IdeApp.Workbench.DocumentClosed += OnDocumentClosed;
            IdeApp.Workspace.FileRenamedInProject += OnFileRenamedInProject;
        }

        void OnDocumentOpened(object sender, DocumentEventArgs e)
        {
            if (e.Document is not Document document)
                return;

            Engine.LoadSettings(document);

            document.Saved += OnDocumentSaved;
        }

        void OnDocumentClosed(object sender, DocumentEventArgs e)
        {
            if (e.Document is not Document document)
                return;

            document.Saved -= OnDocumentSaved;
        }

        void OnDocumentSaved(object sender, EventArgs e)
        {
            if (sender is not Document document)
                return;

            // remove save hook so we can apply EditorConfigs and re-save
            document.Saved -= OnDocumentSaved;

            Engine.LoadSettingsAndApply(document);
            try
            {
                document.Save().Ignore();
            }
            catch (AggregateException aggregateException)
            {
                Exception ex = aggregateException.InnerException;

                string message =
                    $"Failed to save .editorconfig changes to " +
                    $"{document.FileName.FileName}";
                string fullMessage =
                    $"Failed to save .editorconfig changes to " +
                    $"{document.Name}. " +
                    $"Exception={ex.Message}";

                Log.Info(Log.Target.StatusBar, message);
                Log.Info(Log.Target.Console, fullMessage);
            }

            // restore save hook
            document.Saved += OnDocumentSaved;
        }

        void OnFileRenamedInProject(object sender, ProjectFileRenamedEventArgs e)
        {
            // extensions may have changed so reload EditorConfigs
            foreach (ProjectFileRenamedEventInfo info in e)
            {
                Document document = IdeApp.Workbench.GetDocument(info.OldName);
                if (document == null)
                    continue;

                document.Reload().Ignore();

                document = IdeApp.Workbench.GetDocument(info.NewName);
                if (document == null)
                    continue;

                Engine.LoadSettings(document);
            }
        }
    }
}
