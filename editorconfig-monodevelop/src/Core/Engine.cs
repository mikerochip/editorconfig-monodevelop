using EditorConfig.Core;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor.OptionsExtensionMethods;
using Microsoft.VisualStudio.Text.Editor;
using MonoDevelop.Ide.Gui;
using System.Collections.Generic;
using System.Text;

namespace EditorConfig.Addin
{
    public static class Engine
    {
        public static bool LetEolApply { get; set; } = false;


        static Dictionary<EndOfLine, string> EolStrings { get; } = new Dictionary<EndOfLine, string>
        {
            { EndOfLine.CR, "\r" },
            { EndOfLine.LF, "\n" },
            { EndOfLine.CRLF, "\r\n" },
        };


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

            string eol = EolStrings[config.EndOfLine.Value];

            options.SetOptionValue(DefaultOptions.NewLineCharacterOptionId, eol);
        }

        static void LoadSettings_IndentStyle(IEditorOptions options, FileConfiguration config)
        {
            if (config.IndentStyle == null)
                return;

            switch (config.IndentStyle.Value)
            {
                case IndentStyle.Tab:
                    options.SetOptionValue(DefaultOptions.ConvertTabsToSpacesOptionId, false);
                    break;

                case IndentStyle.Space:
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

            if (doc.GetContent<ITextView>() is not ITextView view)
                return;

            Apply_Charset(doc, config);
            Apply_TrimTrailingWhitespace(doc.TextBuffer, config);
            Apply_InsertFinalNewline(doc.TextBuffer, view.Options, config);
            if (LetEolApply)
                Apply_EndOfLine(doc.TextBuffer, config);
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

        static void Apply_TrimTrailingWhitespace(ITextBuffer textBuffer, FileConfiguration config)
        {
            if (config.TrimTrailingWhitespace == null)
                return;

            if (!config.TrimTrailingWhitespace.Value)
                return;

            ITextEdit edit = textBuffer.CreateEdit();

            foreach (ITextSnapshotLine line in textBuffer.CurrentSnapshot.Lines)
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
            ITextBuffer textBuffer,
            IEditorOptions options,
            FileConfiguration config)
        {
            if (config.InsertFinalNewline == null)
                return;
            if (!config.InsertFinalNewline.Value)
                return;

            ITextSnapshotLine secondToLastLine = null;
            ITextSnapshotLine lastLine = null;
            foreach (ITextSnapshotLine line in textBuffer.CurrentSnapshot.Lines)
            {
                secondToLastLine = lastLine;
                lastLine = line;
            }

            if (lastLine.Length == 0)
                return;

            string newlineString = GetBestNewlineString(options, config, secondToLastLine);

            ITextEdit edit = textBuffer.CreateEdit();
            edit.Insert(lastLine.End, newlineString);
            edit.Apply();
        }

        static string GetBestNewlineString(
            IEditorOptions options,
            FileConfiguration config,
            ITextSnapshotLine secondToLastLine)
        {
            if (config.EndOfLine != null)
                return EolStrings[config.EndOfLine.Value];

            // try to be smart and re-use the currently-last line break
            if (secondToLastLine != null)
                return secondToLastLine.GetLineBreakText();

            // there's only one line, so just pull the EOL from options
            return options.GetOptionValue(DefaultOptions.NewLineCharacterOptionId);
        }

        static void Apply_EndOfLine(ITextBuffer textBuffer, FileConfiguration config)
        {
            if (config.EndOfLine == null)
                return;

            string eol = EolStrings[config.EndOfLine.Value];

            ITextEdit edit = textBuffer.CreateEdit();

            foreach (ITextSnapshotLine line in textBuffer.CurrentSnapshot.Lines)
            {
                string currEol = line.GetLineBreakText();
                if (currEol == eol)
                    continue;
                if (currEol.Length == 0)
                    continue;

                Span span = new Span(line.End, line.EndIncludingLineBreak - line.End);
                edit.Replace(span, eol);
            }

            edit.Apply();
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
