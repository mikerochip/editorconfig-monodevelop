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
[assembly: AddinDescription("Adds support for EditorConfig http://editorconfig.org. Files will be modified on save.")]
[assembly: AddinAuthor("Michael Schweitzer")]
