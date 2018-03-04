# EditorConfig Plugin for MonoDevelop and Visual Studio for Mac

This is an [EditorConfig][] plugin for [MonoDevelop][] and [Visual Studio for Mac][].

## Installation

### Install from the Extension Manager

1. Open the Extension Manager in your IDE
2. Click the gallery tab
3. Search for EditorConfig
4. Select the extension and click install

### Install from Source

Source builds are currently only supported using Visual Studio for Mac.

#### Building the Solution

1. Clone the repo
2. Open the solution in Visual Studio for Mac
3. Project > Active Configuration > Release
4. Build > Build All

You should end up with some assemblies in your bin/Release folder. We'll need to use editorconfig-monodevelop.dll in the next step.

#### Building the Extension File (.mpack)

1. Find vstool in your Visual Studio for Mac app folder e.g. /Applications/Visual Studio.app/Contents/MacOS/
2. Run vstool on editorconfig-monodevelop.dll from the previous steps e.g.
> ./vstool setup pack /path/to/editorconfig-monodevelop/editorconfig-monodevelop/bin/Release/net461/editorconfig-monodevelop.dll

This should have generated an .mpack file e.g. EditorConfig.Addin.EditorConfig_1.0.mpack, which you can put wherever you want.

#### Installing the Extension in Visual Studio for Mac

1. Run Visual Studio for Mac
2. Visual Studio for Mac > Extensions...
3. Click Install from file...
4. Navigate to your .mpack and click Open
5. Click Install

## Supported Properties

This plugin supports the following EditorConfig [properties][]:

* `indent_style`
* `indent_size`
* `tab_width`
* `end_of_line` with option to convert on save
* `charset`
* `trim_trailing_whitespace`
* `insert_final_newline`
* `root` (only used by [EditorConfig .NET Core][])

## Bugs and Feature Requests

Adding an issue in the issue tracker is probably fine for now.


[EditorConfig]: http://editorconfig.org
[EditorConfig .NET Core]: https://github.com/editorconfig/editorconfig-core-net
[properties]: http://editorconfig.org/#supported-properties
[MonoDevelop]: http://www.monodevelop.com/
[Visual Studio for Mac]: https://www.visualstudio.com/vs/visual-studio-mac/
