/*---------------------------------------------------------------------------------------------
 *  Zed Editor integration for Unity.
 *  Based on com.unity.ide.visualstudio (Unity Technologies, Microsoft) and
 *  com.unity.ide.cursor (boxqkrtm). Licensed under the MIT License.
 *--------------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using Debug = UnityEngine.Debug;
using IOPath = System.IO.Path;

namespace Zed.Unity.Editor
{
	internal class ZedInstallation : VisualStudioInstallation
	{
		// Created lazily: SdkStyleProjectGeneration reads EditorPrefs, which must happen on the main thread,
		// while installation discovery runs on a background thread.
		private static IGenerator _generator;

		public override bool SupportsAnalyzers => true;

		// csharp-ls uses a recent Roslyn; the effective LangVersion is min(this, Unity's supported version).
		public override Version LatestLanguageVersionSupported => new Version(12, 0);

		public override string[] GetAnalyzers() => Array.Empty<string>();

		public override IGenerator ProjectGenerator => _generator ?? (_generator = new SdkStyleProjectGeneration());

		// ---------------------------------------------------------------- discovery

		private static bool IsCandidateForDiscovery(string path)
		{
			if (string.IsNullOrEmpty(path))
				return false;
#if UNITY_EDITOR_OSX
			return Directory.Exists(path) && path.EndsWith(".app", StringComparison.OrdinalIgnoreCase)
				&& IOPath.GetFileName(path).StartsWith("Zed", StringComparison.OrdinalIgnoreCase);
#elif UNITY_EDITOR_WIN
			if (!File.Exists(path))
				return false;
			var name = IOPath.GetFileName(path);
			return name.Equals("zed.exe", StringComparison.OrdinalIgnoreCase);
#else
			if (!File.Exists(path))
				return false;
			var name = IOPath.GetFileName(path);
			return name == "zed" || name == "zeditor" || name == "zedit" || name == "zed-editor";
#endif
		}

		public static bool TryDiscoverInstallation(string editorPath, out IVisualStudioInstallation installation)
		{
			installation = null;

			if (!IsCandidateForDiscovery(editorPath))
				return false;

			var lower = editorPath.ToLowerInvariant();
			var channel = lower.Contains("preview") ? " Preview" : lower.Contains("nightly") ? " Nightly" : string.Empty;

			Version version = null;
#if UNITY_EDITOR_WIN
			try
			{
				var info = FileVersionInfo.GetVersionInfo(editorPath);
				if (!string.IsNullOrEmpty(info.ProductVersion))
					Version.TryParse(info.ProductVersion.Split('+', '-', ' ').First(), out version);
			}
			catch (Exception)
			{
				// version is cosmetic only
			}
#endif

			installation = new ZedInstallation
			{
				Name = "Zed" + channel + (version != null ? $" [{version.ToString(3)}]" : string.Empty),
				Path = editorPath,
				Version = version ?? new Version(),
				IsPrerelease = channel.Length > 0,
			};
			return true;
		}

		public static IEnumerable<IVisualStudioInstallation> GetInstallations()
		{
			var candidates = new List<string>();

#if UNITY_EDITOR_WIN
			var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
			var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
			foreach (var folder in new[] { "Zed", "Zed Preview", "Zed Nightly" })
			{
				candidates.Add(IOPath.Combine(localAppData, "Programs", folder, "Zed.exe"));
				candidates.Add(IOPath.Combine(programFiles, folder, "Zed.exe"));
			}
			candidates.Add(IOPath.Combine(userProfile, "scoop", "apps", "zed", "current", "zed.exe"));
			candidates.AddRange(FindOnPath("zed.exe").Select(MapWindowsCliToApp));
#elif UNITY_EDITOR_OSX
			var home = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			foreach (var app in new[] { "Zed.app", "Zed Preview.app", "Zed Nightly.app" })
			{
				candidates.Add(IOPath.Combine("/Applications", app));
				candidates.Add(IOPath.Combine(home, "Applications", app));
			}
#else
			var home = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			candidates.Add(IOPath.Combine(home, ".local", "zed.app", "bin", "zed"));
			candidates.Add(IOPath.Combine(home, ".local", "zed-preview.app", "bin", "zed"));
			candidates.Add(IOPath.Combine(home, ".local", "bin", "zed"));
			foreach (var name in new[] { "zed", "zeditor", "zedit", "zed-editor" })
				candidates.AddRange(FindOnPath(name));
			candidates.Add("/var/lib/flatpak/exports/bin/dev.zed.Zed");
#endif

			var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			foreach (var candidate in candidates)
			{
				string full;
				try { full = IOPath.GetFullPath(candidate); }
				catch (Exception) { continue; }

				if (!seen.Add(full))
					continue;

				if (TryDiscoverInstallation(full, out var installation))
					yield return installation;
			}
		}

		private static IEnumerable<string> FindOnPath(string fileName)
		{
			var path = Environment.GetEnvironmentVariable("PATH");
			if (string.IsNullOrEmpty(path))
				yield break;

			foreach (var dir in path.Split(IOPath.PathSeparator))
			{
				if (string.IsNullOrWhiteSpace(dir))
					continue;

				string candidate;
				try { candidate = IOPath.Combine(dir.Trim().Trim('"'), fileName); }
				catch (Exception) { continue; }

				if (File.Exists(candidate))
					yield return candidate;
			}
		}

#if UNITY_EDITOR_WIN
		// <install>\bin\zed.exe (CLI) -> <install>\Zed.exe (app), so both don't show up as separate installs
		private static string MapWindowsCliToApp(string cliPath)
		{
			try
			{
				var dir = IOPath.GetDirectoryName(cliPath);
				if (dir != null && IOPath.GetFileName(dir).Equals("bin", StringComparison.OrdinalIgnoreCase))
				{
					var app = IOPath.Combine(IOPath.GetDirectoryName(dir) ?? dir, "Zed.exe");
					if (File.Exists(app))
						return app;
				}
			}
			catch (Exception)
			{
			}
			return cliPath;
		}
#endif

		// ---------------------------------------------------------------- opening

		/// <summary>Returns the executable to launch and an optional argument prefix.</summary>
		private string ResolveLauncher(out string argumentPrefix)
		{
			argumentPrefix = string.Empty;
#if UNITY_EDITOR_WIN
			// Prefer the CLI (<install>\bin\zed.exe): it forwards to a running instance and returns immediately.
			var dir = IOPath.GetDirectoryName(Path);
			if (dir != null && !IOPath.GetFileName(dir).Equals("bin", StringComparison.OrdinalIgnoreCase))
			{
				var cli = IOPath.Combine(dir, "bin", "zed.exe");
				if (File.Exists(cli))
					return cli;
			}
			return Path;
#elif UNITY_EDITOR_OSX
			var cliPath = IOPath.Combine(Path, "Contents", "MacOS", "cli");
			if (File.Exists(cliPath))
				return cliPath;
			argumentPrefix = $"-a {Quote(Path)} --args ";
			return "open";
#else
			return Path;
#endif
		}

		private static string Quote(string s) => "\"" + s.Replace("\"", "\\\"") + "\"";

		public override bool Open(string path, int line, int column, string solution)
		{
			var projectDirectory = IOPath.GetFullPath(IOPath.GetDirectoryName(solution) ?? IOPath.GetFullPath(IOPath.Combine(Application.dataPath, "..")));

			var args = new StringBuilder();
			args.Append(Quote(projectDirectory));

			if (!string.IsNullOrEmpty(path))
			{
				var target = IOPath.GetFullPath(path);
				if (line > 0)
				{
					target += ":" + line;
					if (column > 0)
						target += ":" + column;
				}
				args.Append(' ').Append(Quote(target));
			}

			var exe = ResolveLauncher(out var prefix);
			try
			{
				var startInfo = new ProcessStartInfo
				{
					FileName = exe,
					Arguments = prefix + args,
					UseShellExecute = false,
					CreateNoWindow = true,
					WorkingDirectory = projectDirectory,
				};
				// Zed's CLI may wait on the app; never block Unity on it.
				Process.Start(startInfo);
				return true;
			}
			catch (Exception ex)
			{
				Debug.LogError($"[Zed] Failed to launch '{exe} {prefix}{args}': {ex.Message}");
				return false;
			}
		}

		// ---------------------------------------------------------------- extra files

		public override void CreateExtraFiles(string projectDirectory)
		{
			if (!ZedSettings.WriteZedProjectSettings)
				return;

			try
			{
				var zedDirectory = IOPath.Combine(projectDirectory, ".zed");
				var settingsFile = IOPath.Combine(zedDirectory, "settings.json");
				if (File.Exists(settingsFile))
					return; // never overwrite user-edited settings

				Directory.CreateDirectory(zedDirectory);

				var generation = ProjectGenerator as ProjectGeneration;
				var solution = ZedSettings.GenerateZedSolution && generation != null
					? generation.ZedSolutionFile()
					: ProjectGenerator.SolutionFile();
				solution = IOPath.GetFullPath(solution);

				var content = @"// Generated by the Unity 'Zed Editor' package (com.leonyew.ide.zed). It will not be overwritten.
{
  ""file_scan_exclusions"": [
    ""**/.git"",
    ""**/Library"",
    ""**/Temp"",
    ""**/Logs"",
    ""**/obj"",
    ""**/*.meta""
  ],
  ""lsp"": {
    ""csharp-ls"": {
      ""settings"": {
        ""solutionPathOverride"": " + JsonString(solution) + @"
      }
    }
  }
}
";
				File.WriteAllText(settingsFile, content, new UTF8Encoding(false));
			}
			catch (IOException ex)
			{
				Debug.LogWarning($"[Zed] Unable to write .zed/settings.json: {ex.Message}");
			}
		}

		private static string JsonString(string value)
		{
			return "\"" + value.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
		}
	}
}
