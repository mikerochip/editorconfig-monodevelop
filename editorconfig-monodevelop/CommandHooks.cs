using EditorConfig.Core;
using System;
using System.Collections.Generic;
using MonoDevelop.Components.Commands;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Commands;

namespace EditorConfig.Addin
{
    static class CommandHooks
    {
        public static void Initialize()
        {
            // mschweitzer NOTE: I really don't see a better way of hooking into
            // file saves
            IdeApp.CommandService.CommandActivating += OnCommandActivating;
        }

        static void OnCommandActivating(object sender, CommandActivationEventArgs e)
        {
            //Log.Info(Log.Target.Console, "source={0} target={1} commandId={2} enabled={3} text=\"{4}\"",
            //         e.Source, e.Target, e.CommandId, e.CommandInfo.Enabled, e.CommandInfo.Text);

            // mschweitzer HACK MAYBE: This is super not ideal, but was the best
            // option for me. Other options were:
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
            Engine.ApplyEditorConfig(IdeApp.Workbench.ActiveDocument);
        }

        static void OnPreFileSaveAll(CommandInfo info)
        {
            Engine.ApplyEditorConfig(IdeApp.Workbench.Documents);
        }
    }
}
