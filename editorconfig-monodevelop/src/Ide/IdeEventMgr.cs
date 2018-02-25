using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MonoDevelop.Components.Commands;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Commands;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Projects;

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
            IdeApp.Workspace.FileRenamedInProject += OnFileRenamedInProject;
        }

        void OnDocumentOpened(object sender, DocumentEventArgs e)
        {
            Log.Info(Log.Target.Console,
                     $"OnDocumentOpened " +
                     $"sender={sender} " +
                     $"e.Document.Name={e.Document.Name} "
                    );

            Document document = e.Document;

            Engine.LoadSettings(document);

            document.Saved += OnDocumentSaved;
            document.Closed += OnDocumentClosed;
        }

        async void OnDocumentSaved(object sender, EventArgs e)
        {
            Log.Info(Log.Target.Console,
                     $"OnDocumentSaved " +
                     $"sender={sender} "
                    );

            Document document = sender as Document;
            if (document == null)
                return;

            document.Saved -= OnDocumentSaved;
            Engine.LoadSettingsAndApply(document);
            try
            {
                await document.Save();
            }
            catch (Exception ex)
            {
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
            document.Saved += OnDocumentSaved;
        }

        void OnDocumentClosed(object sender, EventArgs e)
        {
            Log.Info(Log.Target.Console,
                     $"OnDocumentClosed " +
                     $"sender={sender} "
                    );

            Document document = sender as Document;
            if (document == null)
                return;
            
            document.Saved -= OnDocumentSaved;
            document.Closed -= OnDocumentClosed;
        }

        void OnFileRenamedInProject(object sender, ProjectFileRenamedEventArgs e)
        {
            Log.Info(Log.Target.Console,
                     $"OnFileRenamedInProject " +
                     $"sender={sender} " +
                     $"e={e}"
                    );
            foreach (ProjectFileRenamedEventInfo info in e)
            {
                Log.Info(Log.Target.Console,
                         $"OnFileRenamedInProject " +
                         $"info.OldName={info.OldName} " +
                         $"info.NewName={info.NewName} " +
                         $"info.ProjectFile={info.ProjectFile} " +
                         $"info.ProjectFile.Name={info.ProjectFile.Name}"
                        );
            }
        }
    }
}
