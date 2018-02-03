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
[assembly: AddinDescription("EditorConfig addin http://editorconfig.org")]
[assembly: AddinAuthor("Michael Schweitzer")]
