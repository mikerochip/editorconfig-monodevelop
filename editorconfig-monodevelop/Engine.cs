using EditorConfig.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using MonoDevelop.Ide.Commands;
using MonoDevelop.Components.Commands;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui;

namespace EditorConfig.Addin
{
    public static class Engine
    {
        public static bool IsFile(Document doc)
        {
            if (doc == null)
                return false;

            if (doc.Editor == null)
                return false;

            if (!doc.IsFile)
                return false;

            return true;
        }

        public static FileConfiguration ParseEditorConfig(Document doc)
        {
            string fileName = doc.FileName.FileName;

            EditorConfigParser parser = new EditorConfigParser();
            IEnumerable<FileConfiguration> configs = parser.Parse(fileName);
            FileConfiguration config = configs.First();
            return config;
        }

        public static IEnumerable<FileConfiguration> ParseEditorConfig(IEnumerable<Document> docs)
        {
            string[] fileNames = docs.Select(
                (Document doc) =>
                {
                    if (!IsFile(doc))
                        return string.Empty;
                    return doc.FileName.FileName;
                }
            ).ToArray();

            EditorConfigParser parser = new EditorConfigParser();
            IEnumerable<FileConfiguration> configs = parser.Parse(fileNames);
            return configs;
        }

        public static void ApplyEditorConfig(Document doc)
        {
            FileConfiguration config = ParseEditorConfig(doc);
            ApplyEditorConfig(doc, config);
        }

        public static void ApplyEditorConfig(IEnumerable<Document> docs)
        {
            foreach (Document doc in docs)
            {
                FileConfiguration config = ParseEditorConfig(doc);
                ApplyEditorConfig(doc, config);
            }
        }

        // support:
        // indent_style
        // indent_size
        // end_of_line
        // trim_trailing_whitespace
        // insert_final_newline
        public static void ApplyEditorConfig(Document doc, FileConfiguration config)
        {
            if (!IsFile(doc))
                return;
            
            //if (config.Properties.Count == 0)
            //    return;

            Log.Info(Log.Target.Console, "ApplyEditorConfig doc={0} fileName=\"{1}\" props={2}",
                     doc, doc.FileName.FileName, config.Properties.Count);
        }
    }
}
