using EditorConfig.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis.Text;
using MonoDevelop.Core.Text;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Editor;
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
            string fileName = doc.Name;

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
                    
                    return doc.Name;
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
        // charset
        // indent_style
        // indent_size
        // end_of_line
        // trim_trailing_whitespace
        // insert_final_newline
        public static void ApplyEditorConfig(Document doc, FileConfiguration config)
        {
            if (!IsFile(doc))
                return;
            
            Log.Info(Log.Target.Console, "ApplyEditorConfig doc={0} fileName=\"{1}\" props={2}",
                     doc, doc.FileName.FileName, config.Properties.Count);

            if (config.Properties.Count == 0)
                return;

            TextEditor editor = doc.Editor;

            HandleCharset(editor, config);

            foreach (IDocumentLine line in editor.GetLines())
                ApplyEditorConfig(editor, line, config);
        }

        public static void ApplyEditorConfig(TextEditor editor, IDocumentLine line, FileConfiguration config)
        {
            if (line.UnicodeNewline == UnicodeNewline.Unknown)
                HandleInsertFinalNewline(editor, line, config);
            else
                HandleEndOfLine(editor, line, config);

            HandleTrimTrailingWhitespace(editor, line, config);
        }

        private static void HandleCharset(TextEditor editor, FileConfiguration config)
        {
            if (config.Charset == null)
                return;

            switch (config.Charset.Value)
            {
                case Charset.Latin1:
                    editor.Encoding = Encoding.GetEncoding("ISO-8859-1");
                    break;

                case Charset.UTF16BE:
                    editor.Encoding = Encoding.BigEndianUnicode;
                    break;

                case Charset.UTF16LE:
                    editor.Encoding = Encoding.Unicode;
                    break;

                case Charset.UTF8:
                    editor.Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
                    break;

                case Charset.UTF8BOM:
                    editor.Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
                    break;
            }
        }

        private static void HandleInsertFinalNewline(TextEditor editor, IDocumentLine line, FileConfiguration config)
        {
            if (config.InsertFinalNewline == null)
                return;
            
            if (config.InsertFinalNewline.Value)
            {
                if (line.Length != 0)
                {
                    string newlineStr = GetBestNewlineString(editor, line, config);
                    editor.InsertText(line.EndOffset + 1, newlineStr);
                }
            }
            else
            {
                if (line.Length == 0)
                    editor.RemoveText(line.SegmentIncludingDelimiter);
            }
        }

        public static string GetBestNewlineString(TextEditor editor, IDocumentLine line, FileConfiguration config)
        {
            if (config.EndOfLine != null)
            {
                switch (config.EndOfLine.Value)
                {
                    case EndOfLine.CR:
                        return "\r";

                    case EndOfLine.LF:
                        return "\n";

                    case EndOfLine.CRLF:
                        return "\r\n";
                }
            }

            IDocumentLine lineWithDelimiter = line;
            if (line.UnicodeNewline == UnicodeNewline.Unknown)
            {
                // first line of document with no end_of_line option...just use
                // the editor default
                if (line.PreviousLine == null)
                    return editor.EolMarker;
                
                lineWithDelimiter = line.PreviousLine;
            }

            int delimiterOffset = lineWithDelimiter.EndOffset + 1;
            int delimiterEndOffset = lineWithDelimiter.EndOffsetIncludingDelimiter;
            string delimiter = editor.GetTextBetween(delimiterOffset, delimiterEndOffset);
            return delimiter;
        }

        private static void HandleEndOfLine(TextEditor editor, IDocumentLine line, FileConfiguration config)
        {
            if (config.EndOfLine == null)
                return;
            
            switch (config.EndOfLine.Value)
            {
                case EndOfLine.CR:
                    if (line.UnicodeNewline != UnicodeNewline.CR)
                        editor.ReplaceText(line.EndOffset + 1, line.DelimiterLength, "\r");
                    break;

                case EndOfLine.LF:
                    if (line.UnicodeNewline != UnicodeNewline.LF)
                        editor.ReplaceText(line.EndOffset + 1, line.DelimiterLength, "\n");
                    break;

                case EndOfLine.CRLF:
                    if (line.UnicodeNewline != UnicodeNewline.CRLF)
                        editor.ReplaceText(line.EndOffset + 1, line.DelimiterLength, "\r\n");
                    break;
            }
        }

        private static void HandleTrimTrailingWhitespace(TextEditor editor, IDocumentLine line, FileConfiguration config)
        {
            if (config.TrimTrailingWhitespace == null)
                return;

            if (!config.TrimTrailingWhitespace.Value)
                return;

            for (int i = line.EndOffset; i >= line.Offset; --i)
            {
                char c = editor.GetCharAt(i);

                if (!char.IsWhiteSpace(c))
                {
                    editor.RemoveText(i + 1, line.EndOffset - i);
                    return;
                }
            }
        }
    }
}
