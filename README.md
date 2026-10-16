# Zed Editor for Unity

Use [Zed](https://zed.dev) as Unity's external script editor.

在 Unity 中使用 [Zed](https://zed.dev) 作为外部脚本编辑器。

## How to install / 安装
- Unity -> Window -> Package Manager
- Click "+" at the top left corner / 点击左上角 "+"
- Add package from git URL
- Insert `https://github.com/LeonYew-Ley/com.leonyew.ide.zed.git`
- Add
- Edit -> Preferences -> External Tools -> External Script Editor -> **Zed** / **Zed Preview**
- Done

To update, click **Update** on the package in Package Manager.

更新：在 Package Manager 中选中本包并点击 **Update**。

## Zed setup / Zed 配置
Install the [C# extension](https://zed.dev/extensions/csharp) in Zed and use csharp-ls. The package writes `.zed/settings.json` in your Unity project so csharp-ls loads the generated `.sln` automatically.

在 Zed 中安装 C# 扩展并使用 csharp-ls。本包会在 Unity 项目里写入 `.zed/settings.json`，让 csharp-ls 自动加载生成的 `.sln`，无需手动配置。

```json
"languages": { "CSharp": { "language_servers": ["csharp-ls", "!roslyn", "!omnisharp"] } }
```

## Features / 功能
- Finds Zed, Zed Preview and Zed Nightly on Windows, macOS and Linux.
- Opens scripts and console log entries in Zed at the right line and column.
- Generates `.sln` / `.csproj` files and points csharp-ls at the solution via `.zed/settings.json`.
- No debugger integration.

## License
MIT. Derived from Unity's `com.unity.ide.visualstudio` and boxqkrtm's [com.unity.ide.cursor](https://github.com/boxqkrtm/com.unity.ide.cursor). See [LICENSE.md](LICENSE.md).