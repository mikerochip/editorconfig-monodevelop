using Mono.Addins;
using Mono.Addins.Description;

[assembly: Addin(
    "EditorConfig",
    Namespace = "EditorConfig.Addin",
    Version = "1.3"
)]

[assembly: AddinName("EditorConfig")]
[assembly: AddinCategory("IDE extensions")]
[assembly: AddinAuthor("Michael Schweitzer")]
[assembly: AddinDescription(
    "Author: Michael Schweitzer"
    + "\n"
    + "\nAdds support for EditorConfig http://editorconfig.org"
    + "\n"
    + "\nSupported features:"
    + "\n"
    + "\nindent_style"
    + "\nindent_size"
    + "\ntab_width"
    + "\nend_of_line"
    + "\ncharset"
    + "\ntrim_trailing_whitespace"
    + "\ninsert_final_newline"
    + "\n"
    + "\nThese change editor settings on file open:"
    + "\n"
    + "\nindent_style"
    + "\nindent_size"
    + "\ntab_width"
    + "\nend_of_line"
    + "\n"
    + "\nThese modify files on save:"
    + "\n"
    + "\ncharset"
    + "\ntrim_trailing_whitespace"
    + "\ninsert_final_newline"
    + "\nend_of_line (can enable via an option)"
    + "\n"
    + "\nImplementation generally follows this post: https://github.com/editorconfig/editorconfig/issues/248#issuecomment-166980703"
)]
