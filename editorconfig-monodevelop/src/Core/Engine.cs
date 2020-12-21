using EditorConfig.Core;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor.OptionsExtensionMethods;
using Microsoft.VisualStudio.Text.Editor;
using MonoDevelop.Core.Text;
using MonoDevelop.Ide.Editor;
using MonoDevelop.Ide.Gui;
using System.Collections.Generic;
using System.Text;

using ITextDocument = Microsoft.VisualStudio.Text.ITextDocument;
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

            if (doc.GetContent<ITextView>() is not ITextView view)
                return;

            LoadSettings_EndOfLine(view.Options, config);
            LoadSettings_IndentStyle(view.Options, config);
            LoadSettings_IndentSizeTabWidth(view.Options, config);
        }

        public static void LoadSettings(IEnumerable<Document> docs)
        {
            foreach (Document doc in docs)
                LoadSettings(doc);
        }

        static void LoadSettings_EndOfLine(IEditorOptions options, FileConfiguration config)
        {
            if (config.EndOfLine == null)
                return;

            string eol = null;
            switch (config.EndOfLine.Value)
            {
                case EndOfLine.CR:
                    eol = "\r";
                    break;

                case EndOfLine.LF:
                    eol = "\n";
                    break;

                case EndOfLine.CRLF:
                    eol = "\r\n";
                    break;
            }
            if (eol == null)
                return;

            options.SetOptionValue(DefaultOptions.NewLineCharacterOptionId, eol);
        }

        static void LoadSettings_IndentStyle(IEditorOptions options, FileConfiguration config)
        {
            if (config.IndentStyle == null)
                return;

            switch (config.IndentStyle.Value)
            {
                case Core.IndentStyle.Tab:
                    options.SetOptionValue(DefaultOptions.ConvertTabsToSpacesOptionId, false);
                    break;

                case Core.IndentStyle.Space:
                    options.SetOptionValue(DefaultOptions.ConvertTabsToSpacesOptionId, true);
                    break;
            }
        }

        static void LoadSettings_IndentSizeTabWidth(IEditorOptions options, FileConfiguration config)
        {
            if (config.IndentSize.UseTabWidth &&
                config.TabWidth == null &&
                config.IndentSize.NumberOfColumns == null)
                return;

            if (!config.IndentSize.UseTabWidth &&
                config.IndentSize.NumberOfColumns == null)
                return;

            int size = options.GetTabSize();

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

            options.SetOptionValue(DefaultOptions.TabSizeOptionId, size);
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
            if (doc == null)
                return;
            if (config == null)
                return;
            if (config.Properties.Count == 0)
                return;

            Apply_Charset(doc, config);
            Apply_TrimTrailingWhitespace(doc, config);
            //Apply_InsertFinalNewline(doc.Editor, config);
            //if (LetEolApply)
            //    Apply_EndOfLine(doc.Editor, config);
        }

        static void Apply_Charset(Document doc, FileConfiguration config)
        {
            if (config.Charset == null)
                return;

            ITextDocument textDoc = doc.GetContent<ITextDocument>();

            switch (config.Charset.Value)
            {
                case Charset.Latin1:
                    textDoc.Encoding = Encoding.GetEncoding("ISO-8859-1");
                    break;

                case Charset.UTF16BE:
                    textDoc.Encoding = Encoding.BigEndianUnicode;
                    break;

                case Charset.UTF16LE:
                    textDoc.Encoding = Encoding.Unicode;
                    break;

                case Charset.UTF8:
                    textDoc.Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
                    break;

                case Charset.UTF8BOM:
                    textDoc.Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
                    break;
            }
        }

        static void Apply_TrimTrailingWhitespace(Document doc, FileConfiguration config)
        {
            if (config.TrimTrailingWhitespace == null)
                return;

            if (!config.TrimTrailingWhitespace.Value)
                return;

            ITextEdit edit = doc.TextBuffer.CreateEdit();

            foreach (ITextSnapshotLine line in doc.TextBuffer.CurrentSnapshot.Lines)
            {
                if (line.Length == 0)
                    continue;

                int firstWhitespaceIndex = 1 + line.IndexOfPreviousNonWhiteSpaceCharacter(line.Length);
                SnapshotPoint firstWhitespace = line.Start + firstWhitespaceIndex;

                Span span = new Span(firstWhitespace, line.End - firstWhitespace);
                if (span.Length == 0)
                    continue;

                edit.Replace(span, string.Empty);
            }

            edit.Apply();
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
                Apply_EndOfLine(config, line, changes);
            }

            editor.ApplyTextChanges(changes);
        }

        static void Apply_EndOfLine(
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
