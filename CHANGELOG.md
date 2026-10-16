# Changelog

## [1.0.0] - 2026-10-16
- Initial Zed port of com.unity.ide.cursor / com.unity.ide.visualstudio.
- Zed / Zed Preview / Zed Nightly discovery on Windows, macOS, Linux.
- Opens `"<projectRoot>" "<file>:<line>:<column>"` via the Zed CLI.
- SDK-style .sln/.csproj generation with the usual "Generate .csproj files for" options.
- Optional `<Project>.zed.sln` without `*.Player` projects (default on) for csharp-ls.
- Optional `.zed/settings.json` creation (default off).
