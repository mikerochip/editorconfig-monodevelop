using EditorConfig.Core;
using Microsoft.CodeAnalysis.Text;
using MonoDevelop.Core.Text;
using MonoDevelop.Ide.Editor;
using MonoDevelop.Ide.Gui;
using System.Collections.Generic;
using System.Text;

using TextChange = Microsoft.CodeAnalysis.Text.TextChange;

namespace EditorConfig.Addin
{
    public static class Engine
    {
        public static bool LetEolApply { get; set; } = false;


        public static FileConfiguration ParseConfig(Document doc)
        {
            if (doc == null)
                return null;

            EditorConfigParser parser = new EditorConfigParser();
            FileConfiguration config = parser.Parse(doc.Name);
            return config;
        }


        public static void LoadSettings(Document doc)
        {
            FileConfiguration config = ParseConfig(doc);
            LoadSettings(doc, config);
        }

        public static void LoadSettings(Document doc, FileConfiguration config)
        {
            if (doc == null)
                return;
            if (config == null)
                return;
            if (config.Properties.Count == 0)
                return;

            //Log.Info(Log.Target.Console,
            //         $"LoadSettings doc={doc} name=\"{doc.Name}\" props={config.Properties.Count}"
            //);

            TextEditor editor = doc.Editor;
            if (editor == null)
                return;

            CustomEditorOptions options = new CustomEditorOptions(editor.Options);
            LoadSettings_EndOfLine(options, config);
            LoadSettings_IndentStyle(options, config);
            LoadSettings_IndentSizeTabWidth(options, config);
            editor.Options = options;
        }

        public static void LoadSettings(IEnumerable<Document> docs)
        {
            foreach (Document doc in docs)
                LoadSettings(doc);
        }

        static void LoadSettings_EndOfLine(CustomEditorOptions options, FileConfiguration config)
        {
            if (config.EndOfLine == null)
                return;

            string eolMarker = null;
            switch (config.EndOfLine.Value)
            {
                case EndOfLine.CR:
                    eolMarker = "\r";
                    break;

                case EndOfLine.LF:
                    eolMarker = "\n";
                    break;

                case EndOfLine.CRLF:
                    eolMarker = "\r\n";
                    break;
            }
            if (eolMarker == null)
                return;

            options.OverrideDocumentEolMarker = true;
            options.DefaultEolMarker = eolMarker;
        }

        static void LoadSettings_IndentStyle(CustomEditorOptions options, FileConfiguration config)
        {
            if (config.IndentStyle == null)
                return;

            switch (config.IndentStyle.Value)
            {
                case Core.IndentStyle.Tab:
                    options.TabsToSpaces = false;
                    break;

                case Core.IndentStyle.Space:
                    options.TabsToSpaces = true;
                    break;
            }
        }

        static void LoadSettings_IndentSizeTabWidth(CustomEditorOptions options, FileConfiguration config)
        {
            if (config.IndentSize.UseTabWidth &&
                config.TabWidth == null &&
                config.IndentSize.NumberOfColumns == null)
                return;

            if (!config.IndentSize.UseTabWidth &&
                config.IndentSize.NumberOfColumns == null)
                return;

            //int size = options.IndentationSize;
            int size = options.TabSize;

            if (config.IndentSize.UseTabWidth)
            {
                if (config.TabWidth != null)
                    size = config.TabWidth.Value;
                else if (config.IndentSize.NumberOfColumns != null)
                    size = config.IndentSize.NumberOfColumns.Value;
            }
            else
            {
                size = config.IndentSize.NumberOfColumns.Value;
            }

            //options.IndentationSize = size;
            options.TabSize = size;
        }


        public static void Apply(Document doc)
        {
            FileConfiguration config = ParseConfig(doc);
            Apply(doc, config);
        }

        public static void Apply(IEnumerable<Document> docs)
        {
            foreach (Document doc in docs)
            {
                FileConfiguration config = ParseConfig(doc);
                Apply(doc, config);
            }
        }

        public static void Apply(Document doc, FileConfiguration config)
        {
            //Log.Info(Log.Target.Console,
            //         $"Apply doc={doc} name=\"{doc.Name}\" props={config.Properties.Count}"
            //);

            if (doc == null)
                return;
            if (config == null)
                return;
            if (config.Properties.Count == 0)
                return;

            TextEditor editor = doc.Editor;
            if (editor == null)
                return;

            Apply_Charset(editor, config);
            Apply_TrimTrailingWhitespace(editor, config);
            Apply_InsertFinalNewline(editor, config);
            if (LetEolApply)
                Apply_EndOfLine(editor, config);
        }

        static void Apply_Charset(
            TextEditor editor,
            FileConfiguration config)
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

        static void Apply_TrimTrailingWhitespace(
            TextEditor editor,
            FileConfiguration config)
        {
            if (config.TrimTrailingWhitespace == null)
                return;

            if (!config.TrimTrailingWhitespace.Value)
                return;

            List<TextChange> changes = new List<TextChange>();

            foreach (IDocumentLine line in editor.GetLines())
                Apply_TrimTrailingWhitespace(editor, line, changes);

            editor.ApplyTextChanges(changes);
        }

        static void Apply_TrimTrailingWhitespace(
            TextEditor editor,
            IDocumentLine line,
            List<TextChange> changes)
        {
            int offset = line.EndOffset;
            for (; offset > line.Offset; --offset)
            {
                char c = editor.GetCharAt(offset - 1);

                if (!char.IsWhiteSpace(c))
                    break;
            }

            TextChange change = ChangeFromBounds(offset, line.EndOffset, string.Empty);
            if (change.Span.Length == 0)
                return;

            changes.Add(change);
        }

        static void Apply_InsertFinalNewline(
            TextEditor editor,
            FileConfiguration config)
        {
            if (config.InsertFinalNewline == null)
                return;

            List<TextChange> changes = new List<TextChange>();

            Apply_InsertFinalNewline(editor, config, changes);

            editor.ApplyTextChanges(changes);
        }

        static void Apply_InsertFinalNewline(
            TextEditor editor,
            FileConfiguration config,
            List<TextChange> changes)
        {
            IDocumentLine lastLine = editor.GetLine(editor.LineCount);

            if (config.InsertFinalNewline.Value)
            {
                if (lastLine.Length == 0)
                    return;

                string newlineString = GetBestNewlineString(editor, config, lastLine);
                TextChange change = ChangeAtOffset(lastLine.EndOffset, newlineString);
                changes.Add(change);
            }
            else
            {
                int offset = lastLine.EndOffset;
                for (int i = editor.LineCount; i > 0; --i)
                {
                    IDocumentLine currLine = editor.GetLine(i);
                    if (currLine.Length != 0)
                    {
                        offset = currLine.EndOffset;
                        break;
                    }
                }

                // remove all trailing empty lines in one change
                TextChange change = ChangeFromBounds(offset, lastLine.EndOffset, string.Empty);
                changes.Add(change);
            }
        }

        static string GetBestNewlineString(
            TextEditor editor,
            FileConfiguration config,
            IDocumentLine line)
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
                // we don't have an end_of_line option AND this is the first line
                // of the document...just use the editor default
                if (line.PreviousLine == null)
                    return editor.EolMarker;

                lineWithDelimiter = line.PreviousLine;
            }

            int delimiterOffset = lineWithDelimiter.EndOffset + 1;
            int delimiterEndOffset = lineWithDelimiter.EndOffsetIncludingDelimiter;
            string delimiter = editor.GetTextBetween(delimiterOffset, delimiterEndOffset);
            return delimiter;
        }

        static void Apply_EndOfLine(
            TextEditor editor,
            FileConfiguration config)
        {
            if (config.EndOfLine == null)
                return;

            List<TextChange> changes = new List<TextChange>();

            for (int i = 1; i < editor.LineCount; ++i)
            {
                IDocumentLine line = editor.GetLine(i);
                Apply_EndOfLine(editor, config, line, changes);
            }

            editor.ApplyTextChanges(changes);
        }

        static void Apply_EndOfLine(
            TextEditor editor,
            FileConfiguration config,
            IDocumentLine line,
            List<TextChange> changes)
        {
            string eolMarker = null;
            switch (config.EndOfLine.Value)
            {
                case EndOfLine.CR:
                    if (line.UnicodeNewline != UnicodeNewline.CR)
                        eolMarker = "\r";
                    break;

                case EndOfLine.LF:
                    if (line.UnicodeNewline != UnicodeNewline.LF)
                        eolMarker = "\n";
                    break;

                case EndOfLine.CRLF:
                    if (line.UnicodeNewline != UnicodeNewline.CRLF)
                        eolMarker = "\r\n";
                    break;
            }
            if (eolMarker == null)
                return;

            TextChange change = ChangeFromBounds(line.EndOffset, line.EndOffsetIncludingDelimiter, eolMarker);
            changes.Add(change);
        }

        static TextChange ChangeFromBounds(int offset, int endOffset, string text)
        {
            TextSpan span = new TextSpan(offset, endOffset - offset);
            TextChange change = new TextChange(span, text);
            return change;
        }

        static TextChange ChangeAtOffset(int offset, string text)
        {
            TextSpan span = new TextSpan(offset, 0);
            TextChange change = new TextChange(span, text);
            return change;
        }


        public static void LoadSettingsAndApply(Document doc)
        {
            FileConfiguration config = ParseConfig(doc);
            LoadSettings(doc, config);
            Apply(doc, config);
        }

        public static void LoadSettingsAndApply(IEnumerable<Document> docs)
        {
            foreach (Document doc in docs)
                LoadSettingsAndApply(doc);
        }
    }
}
