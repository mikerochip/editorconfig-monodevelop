using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using MonoDevelop.Components.Commands;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Commands;
using MonoDevelop.Ide.Gui;

namespace EditorConfig.Addin
{
    class CommandHookMgr
    {
        class WorkbenchState
        {
            public ImmutableList<Document> Documents
            {
                get;
                private set;
            } = ImmutableList<Document>.Empty;

            public Document ActiveDocument
            {
                get;
                private set;
            }

            public bool IsActiveDocumentDirty
            {
                get;
                private set;
            }

            public void Save()
            {
                Documents = IdeApp.Workbench.Documents.ToImmutableList();
                ActiveDocument = IdeApp.Workbench.ActiveDocument;
                if (ActiveDocument != null)
                    IsActiveDocumentDirty = ActiveDocument.IsDirty;
            }

            public void Reset()
            {
                Documents = ImmutableList<Document>.Empty;
                ActiveDocument = null;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(this, obj))
                    return true;

                WorkbenchState state = obj as WorkbenchState;
                if (state == null)
                    return false;

                if (Documents.SequenceEqual(state.Documents))
                    return false;

                if (ActiveDocument == null && state.ActiveDocument != null)
                    return false;

                if (ActiveDocument != null && state.ActiveDocument == null)
                    return false;

                if (!ActiveDocument.Equals(state.ActiveDocument))
                    return false;

                if (IsActiveDocumentDirty != state.IsActiveDocumentDirty)
                    return false;

                return true;
            }

            // see https://stackoverflow.com/questions/263400/what-is-the-best-algorithm-for-an-overridden-system-object-gethashcode
            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = 17;
                    hash = hash * 29 + Documents.GetHashCode();
                    if (ActiveDocument != null)
                        hash = hash * 29 + ActiveDocument.GetHashCode();
                    return hash;
                }
            }
        }


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
            // mschweitzer NOTE: I really don't see a better way of hooking into
            // file operations. Reimplementing them (the only other alternative
            // AFAIK) is not a good option.
            IdeApp.CommandService.CommandActivating += instance.OnCommandActivating;
            IdeApp.CommandService.CommandActivated += instance.OnCommandActivated;
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

            if (preState.Equals(postState))
            {
                preState.Reset();
                postState.Reset();
                return;
            }

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
                case "MonoDevelop.Ide.Commands.FileCommands.SaveAs":
                    OnPostFileSave();
                    break;

                case "MonoDevelop.Ide.Commands.FileCommands.SaveAll":
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

            preState.Reset();
            postState.Reset();
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
