using EditorConfig.Core;
using System;
using System.Collections.Generic;
using MonoDevelop.Components.Commands;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Commands;
using MonoDevelop.Ide.Gui;

namespace EditorConfig.Addin
{
    static class CommandHooks
    {
        public static void Initialize()
        {
            // mschweitzer NOTE: I really don't see a better way of hooking into
            // file operations. Reimplementing them (the only other alternative
            // AFAIK) is not a good option.
            IdeApp.CommandService.CommandActivating += OnCommandActivating;
            IdeApp.CommandService.CommandActivated += OnCommandActivated;
        }

        static void OnCommandActivating(object sender, CommandActivationEventArgs e)
        {
            Log.Info(Log.Target.Console,
                     $"OnCommandActivating " +
                     $"sender={sender} source={e.Source} target={e.Target} " +
                     $"dataItem={e.DataItem} commandId={e.CommandId} " +
                     $"enabled={e.CommandInfo.Enabled} text=\"{e.CommandInfo.Text}\""
            );

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
                case "MonoDevelop.Ide.Commands.FileCommands.Save":
                case "MonoDevelop.Ide.Commands.FileCommands.SaveAs":
                    OnPreFileSave(e.CommandInfo);
                    break;

                case "MonoDevelop.Ide.Commands.FileCommands.SaveAll":
                    OnPreFileSaveAll(e.CommandInfo);
                    break;
            }
        }

        static void OnPreFileSave(CommandInfo info)
        {
            Document doc = IdeApp.Workbench.ActiveDocument;

            Engine.Apply(doc);
        }

        static void OnPreFileSaveAll(CommandInfo info)
        {
            Engine.Apply(IdeApp.Workbench.Documents);
        }

        static void OnCommandActivated(object sender, CommandActivationEventArgs e)
        {
            Log.Info(Log.Target.Console,
                     $"OnCommandActivated " +
                     $"sender={sender} source={e.Source} target={e.Target} " +
                     $"dataItem={e.DataItem} commandId={e.CommandId} " +
                     $"enabled={e.CommandInfo.Enabled} text=\"{e.CommandInfo.Text}\""
            );

            // see mschweitzer HACK in OnCommandActivating
            switch (e.CommandId)
            {
                case "MonoDevelop.Ide.Commands.FileCommands.NewFile":
                    OnPostNewFile(e.CommandInfo);
                    break;

                case "MonoDevelop.Ide.Commands.FileCommands.NewProject":
                    OnPostNewProject(e.CommandInfo);
                    break;

                case "MonoDevelop.Ide.Commands.FileCommands.Save":
                case "MonoDevelop.Ide.Commands.FileCommands.SaveAs":
                    OnPostFileSave(e.CommandInfo);
                    break;

                case "MonoDevelop.Ide.Commands.FileCommands.SaveAll":
                    OnPostFileSaveAll(e.CommandInfo);
                    break;

                case "MonoDevelop.Ide.Commands.FileCommands.OpenFile":
                case "MonoDevelop.Ide.Commands.FileCommands.ReloadFile":
                case "MonoDevelop.Ide.Commands.FileCommands.RecentFileList":
                    OnPostFileOpen(e.CommandInfo);
                    break;

                case "MonoDevelop.Ide.Commands.FileCommands.RecentProjectList":
                    OnPostRecentProjectList(e.CommandInfo);
                    break;
            }
        }

        static void OnPostNewFile(CommandInfo info)
        {
            Document doc = IdeApp.Workbench.ActiveDocument;

            Engine.LoadSettings(doc);
        }

        static void OnPostNewProject(CommandInfo info)
        {
            Engine.LoadSettings(IdeApp.Workbench.Documents);
        }

        static void OnPostFileSave(CommandInfo info)
        {
            Document doc = IdeApp.Workbench.ActiveDocument;

            Engine.LoadSettings(doc);
        }

        static void OnPostFileSaveAll(CommandInfo info)
        {
            Engine.LoadSettings(IdeApp.Workbench.Documents);
        }

        static void OnPostFileOpen(CommandInfo info)
        {
            // mschweiter MOJO: When there are no open files or projects in
            // MonoDevelop, ActiveDocument is null and the first document is
            // the newly opened file.
            Document doc = IdeApp.Workbench.ActiveDocument;
            if (doc == null && IdeApp.Workbench.Documents.Count > 0)
                doc = IdeApp.Workbench.Documents[0];
            
            Engine.LoadSettings(doc);
        }

        static void OnPostRecentProjectList(CommandInfo info)
        {
            Engine.LoadSettings(IdeApp.Workbench.Documents);
        }
    }
}
