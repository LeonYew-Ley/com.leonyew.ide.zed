# Changelog

## [1.2.0]
- Removed the separate `<Project>.zed.sln`; SDK-style projects never include Player projects, so the regular `.sln` works.
- `.zed/settings.json` is now written by default and keeps `solutionPathOverride` pointing at the generated `.sln`.

## [1.1.1]
- Author name set to LeonYew.

## [1.1.0]
- Renamed package to `com.leonyew.ide.zed` (the `com.unity.` prefix is reserved by Unity).
- Fixed EditorPrefs being read off the main thread during installation discovery.

## [1.0.0] - 2026-10-16
- Initial Zed port of com.unity.ide.cursor / com.unity.ide.visualstudio.
- Zed / Zed Preview / Zed Nightly discovery on Windows, macOS, Linux.
- Opens `"<projectRoot>" "<file>:<line>:<column>"` via the Zed CLI.
- SDK-style .sln/.csproj generation with the usual "Generate .csproj files for" options.
- Optional `<Project>.zed.sln` without `*.Player` projects (default on) for csharp-ls.
- Optional `.zed/settings.json` creation (default off).
