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
    "Adds support for EditorConfig http://editorconfig.org"
    + "\n"
    + "\nSupported features:\n"
    + "\nindent_style"
    + "\nindent_size"
    + "\ntab_width"
    + "\nend_of_line"
    + "\ncharset"
    + "\ntrim_trailing_whitespace"
    + "\ninsert_final_newline"
    + "\n"
    + "\nImplementation generally follows this post: https://github.com/editorconfig/editorconfig/issues/248#issuecomment-166980703"
    + "\nThese are file open features:"
    + "\n indent_style"
    + "\n indent_size"
    + "\n tab_width"
    + "\n end_of_line"
    + "\nThese are file save features:"
    + "\n charset"
    + "\n trim_trailing_whitespace"
    + "\n insert_final_newline"
    + "\n end_of_line (enabled via an option)"
)]
