using System;
using Mono.Addins;
using Mono.Addins.Description;

[assembly: Addin(
    "EditorConfig",
    Namespace = "EditorConfig.Addin",
    Version = "1.0"
)]

[assembly: AddinName("EditorConfig")]
[assembly: AddinCategory("IDE extensions")]
[assembly: AddinAuthor("Michael Schweitzer")]
[assembly: AddinDescription(
    "Adds support for EditorConfig http://editorconfig.org. "
    + "Files are modified on save, and there is a new Edit menu command.\n"
    + "\nSupported features:\n"
    + "\nindent_style"
    + "\nindent_size"
    + "\ntab_width"
    + "\nend_of_line"
    + "\ncharset"
    + "\ntrim_trailing_whitespace"
    + "\ninsert_final_newline"
)]
