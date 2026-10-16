# Zed Editor for Unity (`com.leonyew.ide.zed`)

[中文](#中文) | [English](#english)

## 中文

让 Unity 把 [Zed](https://zed.dev) 作为外部脚本编辑器：自动发现 Zed 安装、双击脚本/控制台日志时在 Zed 中以项目文件夹打开并跳转到行列，并生成 `.sln` / `.csproj` 供 csharp-ls 等语言服务器使用。

### 安装
1. 打开 Unity 菜单 `Window > Package Manager`。
2. 点左上角 `+`，选择 `Add package from git URL...`。
3. 填入下面的地址并点 `Add`：
   ```
   https://github.com/LeonYew-Ley/com.leonyew.ide.zed.git#v1.1.0
   ```
   也可以直接在 `Packages/manifest.json` 的 `dependencies` 里加入：
   ```json
   "com.leonyew.ide.zed": "https://github.com/LeonYew-Ley/com.leonyew.ide.zed.git#v1.1.0"
   ```

**更新**：把地址末尾的 `#v1.1.0` 换成 [Releases / Tags](https://github.com/LeonYew-Ley/com.leonyew.ide.zed/tags) 中更新的版本号即可。去掉 `#版本号` 会跟随 `main` 分支，但 Unity 会锁定安装时的提交，需要在 Package Manager 里点 `Update` 才会拉取最新代码。
然后打开 `Edit > Preferences > External Tools`，在 **External Script Editor** 中选择 `Zed` / `Zed Preview`（或 `Browse...` 选择 `Zed.exe`）。

### 功能
- 发现安装：Windows `%LOCALAPPDATA%\Programs\Zed\Zed.exe`、`Zed Preview\Zed.exe`、`Zed Nightly`、Program Files、scoop、PATH 中的 `zed.exe`；macOS `/Applications/Zed.app`、`Zed Preview.app`（以及 `~/Applications`）；Linux PATH 中的 `zed`/`zeditor`、`~/.local/zed.app/bin/zed`、Flatpak。
- 打开方式：`zed "<项目根目录>" "<文件>:<行>:<列>"`（Windows 优先使用 `bin\zed.exe` CLI，macOS 使用 `Contents/MacOS/cli`）。
- "Generate .csproj files for"：Embedded / Local / Registry / Git / Built-in / Local tarball / Unknown / Player projects，以及 **Regenerate project files** 按钮。
- **Generate \<Project\>.zed.sln**（默认开启）：每次生成工程文件时额外写出不含 `*.Player` 工程的 `<项目名>.zed.sln`，避免 csharp-ls 因重复工程名（如 `Unity.Timeline` 与 `Unity.Timeline.Player`）加载失败。在 Zed 设置中：
  ```json
  "lsp": { "csharp-ls": { "settings": { "solutionPathOverride": "<你的项目路径>\\<项目名>.zed.sln" } } }
  ```
- **Write .zed/settings.json**（默认关闭）：仅当项目根目录下不存在 `.zed/settings.json` 时创建一个（含 `solutionPathOverride` 与扫描排除项），不会覆盖已有文件。

### 注意
- 与 `com.unity.ide.visualstudio` / `com.unity.ide.rider` / Cursor 包可以共存，但"Generate .csproj files for"的选项是本包独立保存的（EditorPrefs 键 `zed_unity_project_generation_flag`）。
- 不包含调试器附加功能。

## English

Use [Zed](https://zed.dev) as Unity's external script editor: installation discovery, opening files at line/column inside the project workspace, and `.sln`/`.csproj` generation for csharp-ls and other language servers.

### Install
1. Open `Window > Package Manager` in Unity.
2. Click `+` in the top-left corner and choose `Add package from git URL...`.
3. Paste this URL and click `Add`:
   ```
   https://github.com/LeonYew-Ley/com.leonyew.ide.zed.git#v1.1.0
   ```
   Or add it to `dependencies` in `Packages/manifest.json`:
   ```json
   "com.leonyew.ide.zed": "https://github.com/LeonYew-Ley/com.leonyew.ide.zed.git#v1.1.0"
   ```

**Updating**: replace `#v1.1.0` with a newer version from [Tags](https://github.com/LeonYew-Ley/com.leonyew.ide.zed/tags). Without a `#version` suffix the package follows `main`, but Unity pins the commit it installed, so click `Update` in Package Manager to fetch the latest.
Then go to `Edit > Preferences > External Tools` and choose `Zed` / `Zed Preview` as **External Script Editor** (or `Browse...` to `Zed.exe`).

### Features
- Discovery: Windows (`%LOCALAPPDATA%\Programs\Zed`, `Zed Preview`, `Zed Nightly`, Program Files, scoop, `zed.exe` on PATH), macOS (`/Applications/Zed.app`, `Zed Preview.app`, `~/Applications`), Linux (`zed`/`zeditor` on PATH, `~/.local/zed.app/bin/zed`, Flatpak).
- Opens with `zed "<projectRoot>" "<file>:<line>:<column>"` (uses the CLI `bin\zed.exe` on Windows and `Contents/MacOS/cli` on macOS when available).
- "Generate .csproj files for" toggles (Embedded, Local, Registry, Git, Built-in, Local tarball, Unknown, Player projects) and a **Regenerate project files** button.
- **Generate \<Project\>.zed.sln** (default on): also writes `<ProjectName>.zed.sln` without `*.Player` projects whenever project files are regenerated, so csharp-ls doesn't fail on duplicate project names. Point `lsp.csharp-ls.settings.solutionPathOverride` at it.
- **Write .zed/settings.json** (default off): creates `.zed/settings.json` in the project root only if it doesn't exist yet.

### Notes
- Can coexist with the Visual Studio / Rider / Cursor packages; this package keeps its own generation flags (`zed_unity_project_generation_flag`).
- No debugger integration.

## License
MIT — derived from Unity Technologies' `com.unity.ide.visualstudio` (© Unity Technologies, © Microsoft) and boxqkrtm's `com.unity.ide.cursor`. See [LICENSE.md](LICENSE.md).
