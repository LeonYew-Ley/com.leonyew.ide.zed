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

## Recommended Zed setup / 推荐的 Zed 配置
Use [csharp-ls](https://github.com/razzmatazz/csharp-language-server) and point it at the generated solution in Zed's `settings.json`:

在 Zed 的 `settings.json` 中使用 csharp-ls 并指定生成的解决方案：

```json
"languages": { "CSharp": { "language_servers": ["csharp-ls", "!roslyn", "!omnisharp"] } },
"lsp": { "csharp-ls": { "settings": { "solutionPathOverride": "<ProjectPath>/<ProjectName>.zed.sln" } } }
```

## Features / 功能
- Finds Zed, Zed Preview and Zed Nightly on Windows, macOS and Linux.
- Opens scripts and console log entries in Zed at the right line and column.
- Generates `.sln` / `.csproj` files, plus a `<ProjectName>.zed.sln` without Player projects for csharp-ls.
- No debugger integration.

## License
MIT. Derived from Unity's `com.unity.ide.visualstudio` and boxqkrtm's [com.unity.ide.cursor](https://github.com/boxqkrtm/com.unity.ide.cursor). See [LICENSE.md](LICENSE.md).