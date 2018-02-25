using System;
using System.Collections.Generic;
using MonoDevelop.Components.Commands;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Commands;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Projects;

namespace EditorConfig.Addin
{
    class CommandHookMgr
    {
        static CommandHookMgr instance;
        WorkbenchState preState = new WorkbenchState();
        WorkbenchState postState = new WorkbenchState();


        public static void Initialize()
        {
            if (instance != null)
                return;

            instance = new CommandHookMgr();
            instance.InitializeImpl();
        }

        public static CommandHookMgr Get()
        {
            return instance;
        }

        void InitializeImpl()
        {
            IdeApp.Workbench.DocumentOpened += (object sender, DocumentEventArgs e) =>
            {
                Log.Info(Log.Target.Console,
                         $"Workbench.DocumentOpened " +
                         $"sender={sender} " +
                         $"e.Document.Name={e.Document.Name}"
                        );

                Engine.LoadSettings(e.Document);

                e.Document.Saved += OnDocumentSaved;
                e.Document.Closed += OnDocumentClosed;
            };
            IdeApp.Workspace.FileRenamedInProject += (object sender, ProjectFileRenamedEventArgs e) => 
            {
                Log.Info(Log.Target.Console,
                         $"Workspace.FileRenamedInProject " +
                         $"sender={sender} " +
                         $"e={e}"
                        );
            };

            // mschweitzer NOTE: I really don't see a better way of hooking into
            // file operations. Reimplementing them (the only other alternative
            // AFAIK) is not a good option.
            IdeApp.CommandService.CommandActivating += instance.OnCommandActivating;
            IdeApp.CommandService.CommandActivated += instance.OnCommandActivated;
        }

        void OnDocumentOpened(object sender, DocumentEventArgs e)
        {
            Log.Info(Log.Target.Console,
                     $"OnDocumentOpened " +
                     $"sender={sender} " +
                     $"e.Document.Name={e.Document.Name}"
            );

            Engine.LoadSettings(e.Document);

            e.Document.Saved += OnDocumentSaved;
            e.Document.Closed += OnDocumentClosed;
        }

        void OnDocumentSaved(object sender, EventArgs e)
        {
            Log.Info(Log.Target.Console,
                     $"OnDocumentSaved " +
                     $"sender={sender} " +
                     $"e={e}"
            );

            Document document = sender as Document;
            if (document == null)
                return;
        }

        void OnDocumentClosed(object sender, EventArgs e)
        {
            Log.Info(Log.Target.Console,
                     $"OnDocumentClosed " +
                     $"sender={sender} " +
                     $"e={e}"
            );

            Document document = sender as Document;
            if (document == null)
                return;
        }

        void OnCommandActivating(object sender, CommandActivationEventArgs e)
        {
            Log.Info(Log.Target.Console,
                     $"OnCommandActivating " +
                     $"source={e.Source} target={e.Target} " +
                     $"dataItem={e.DataItem} commandId={e.CommandId} command={e.CommandInfo.Command} " +
                     $"enabled={e.CommandInfo.Enabled} text=\"{e.CommandInfo.Text}\""
            );

            preState.Save();
        }

        void OnCommandActivated(object sender, CommandActivationEventArgs e)
        {
            Log.Info(Log.Target.Console,
                     $"OnCommandActivated " +
                     $"source={e.Source} target={e.Target} " +
                     $"dataItem={e.DataItem} commandId={e.CommandId} command={e.CommandInfo.Command} " +
                     $"enabled={e.CommandInfo.Enabled} text=\"{e.CommandInfo.Text}\" "
            );

            postState.Save();

            //HandleActivatedCommand(sender, e);

            preState.Reset();
            postState.Reset();
        }

        void HandleActivatedCommand(object sender, CommandActivationEventArgs e)
        {
            // mschweitzer HACK: This is super not ideal, but was the best option
            // for me. Other options were:
            //
            // 1. Attempting to use the actual FileCommands enum values, which
            //    would've required a lot of gymnastics
            // 2. Making custom commands in the xml, which would incur a lot
            //    more wheel reinventing
            // 3. Adding global handlers, which didn't seem to work aside from
            //    the SaveAll case
            switch (e.CommandId)
            {
                case "MonoDevelop.Ide.Commands.FileCommands.NewFile":
                    OnPostNewFile();
                    break;

                case "MonoDevelop.Ide.Commands.FileCommands.NewProject":
                    OnPostNewProject();
                    break;

                case "MonoDevelop.Ide.Commands.FileCommands.Save":
                    // the states are equivalent in this case
                    OnPostFileSave();
                    break;

                case "MonoDevelop.Ide.Commands.FileCommands.SaveAs":
                    if (!preState.Equals(postState))
                        OnPostFileSave();
                    break;

                case "MonoDevelop.Ide.Commands.FileCommands.SaveAll":
                    // this checks states within the command
                    OnPostFileSaveAll();
                    break;

                case "MonoDevelop.Ide.Commands.FileCommands.OpenFile":
                case "MonoDevelop.Ide.Commands.FileCommands.ReloadFile":
                case "MonoDevelop.Ide.Commands.FileCommands.RecentFileList":
                    OnPostFileOpen();
                    break;

                case "MonoDevelop.Ide.Commands.FileCommands.RecentProjectList":
                    OnPostRecentProjectList();
                    break;
            }
        }

        void OnPostNewFile()
        {
            Document doc = IdeApp.Workbench.ActiveDocument;

            Engine.LoadSettings(doc);
        }

        void OnPostNewProject()
        {
            Engine.LoadSettings(IdeApp.Workbench.Documents);
        }

        void OnPostFileSave()
        {
            Document doc = IdeApp.Workbench.ActiveDocument;

            Engine.LoadSettingsAndApply(doc);

            // mschweitzer TODO: prevent infinite recursion
            //doc.Save();
        }

        void OnPostFileSaveAll()
        {
            Engine.LoadSettingsAndApply(IdeApp.Workbench.Documents);

            // mschweitzer TODO: prevent infinite recursion
            //foreach (Document doc in IdeApp.Workbench.Documents)
            //    doc.Save();
        }

        void OnPostFileOpen()
        {
            // mschweiter MOJO: When there are no open files or projects in
            // MonoDevelop, ActiveDocument is null and the first document is
            // the newly opened file.
            Document doc = IdeApp.Workbench.ActiveDocument;
            if (doc == null && IdeApp.Workbench.Documents.Count > 0)
                doc = IdeApp.Workbench.Documents[0];

            Engine.LoadSettings(doc);
        }

        void OnPostRecentProjectList()
        {
            Engine.LoadSettings(IdeApp.Workbench.Documents);
        }
    }
}
