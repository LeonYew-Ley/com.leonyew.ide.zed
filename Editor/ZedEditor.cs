/*---------------------------------------------------------------------------------------------
 *  Zed Editor integration for Unity.
 *  Based on com.unity.ide.visualstudio (Unity Technologies, Microsoft) and
 *  com.unity.ide.cursor (boxqkrtm). Licensed under the MIT License.
 *--------------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Unity.CodeEditor;

namespace Zed.Unity.Editor
{
	[InitializeOnLoad]
	public class ZedEditor : IExternalCodeEditor
	{
		CodeEditor.Installation[] IExternalCodeEditor.Installations => _discoverInstallations
			.Result
			.Values
			.Select(v => v.ToCodeEditorInstallation())
			.ToArray();

		private static readonly AsyncOperation<Dictionary<string, IVisualStudioInstallation>> _discoverInstallations;

		static ZedEditor()
		{
			if (!UnityInstallation.IsMainUnityEditorProcess)
				return;

			CodeEditor.Register(new ZedEditor());

			_discoverInstallations = AsyncOperation<Dictionary<string, IVisualStudioInstallation>>.Run(DiscoverInstallations);
		}

		private static Dictionary<string, IVisualStudioInstallation> DiscoverInstallations()
		{
			var result = new Dictionary<string, IVisualStudioInstallation>(StringComparer.OrdinalIgnoreCase);
			try
			{
				foreach (var installation in ZedInstallation.GetInstallations())
				{
					var key = Path.GetFullPath(installation.Path);
					if (!result.ContainsKey(key))
						result.Add(key, installation);
				}
			}
			catch (Exception ex)
			{
				Debug.LogError($"[Zed] Error detecting Zed installations: {ex}");
			}
			return result;
		}

		internal static bool IsEnabled => CodeEditor.CurrentEditor is ZedEditor && UnityInstallation.IsMainUnityEditorProcess;

		internal static string PackageVersion()
		{
			var package = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(ZedEditor).Assembly);
			return package?.version ?? "0.0.0";
		}

		public void CreateIfDoesntExist()
		{
			if (!TryGetVisualStudioInstallationForPath(CodeEditor.CurrentEditorInstallation, true, out var installation))
				return;

			var generator = installation.ProjectGenerator;
			if (!generator.HasSolutionBeenGenerated())
				generator.Sync();
		}

		public void Initialize(string editorInstallationPath)
		{
		}

		internal virtual bool TryGetVisualStudioInstallationForPath(string editorPath, bool lookupDiscoveredInstallations, out IVisualStudioInstallation installation)
		{
			installation = null;
			if (string.IsNullOrEmpty(editorPath))
				return false;

			try
			{
				editorPath = Path.GetFullPath(editorPath);
			}
			catch (Exception)
			{
				return false;
			}

			if (lookupDiscoveredInstallations && _discoverInstallations != null && _discoverInstallations.Result.TryGetValue(editorPath, out installation))
				return true;

			return ZedInstallation.TryDiscoverInstallation(editorPath, out installation);
		}

		public virtual bool TryGetInstallationForPath(string editorPath, out CodeEditor.Installation installation)
		{
			var result = TryGetVisualStudioInstallationForPath(editorPath, lookupDiscoveredInstallations: false, out var vsi);
			installation = vsi?.ToCodeEditorInstallation() ?? default;
			return result;
		}

		public void OnGUI()
		{
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();

			if (!TryGetVisualStudioInstallationForPath(CodeEditor.CurrentEditorInstallation, true, out var installation))
			{
				GUILayout.EndHorizontal();
				return;
			}

			var package = UnityEditor.PackageManager.PackageInfo.FindForAssembly(GetType().Assembly);

			var style = new GUIStyle
			{
				richText = true,
				margin = new RectOffset(0, 4, 0, 0)
			};

			if (package != null)
				GUILayout.Label($"<size=10><color=grey>{package.displayName} v{package.version} enabled</color></size>", style);
			GUILayout.EndHorizontal();

			EditorGUILayout.LabelField("Generate .csproj files for:");
			EditorGUI.indentLevel++;
			SettingsButton(ProjectGenerationFlag.Embedded, "Embedded packages", "", installation);
			SettingsButton(ProjectGenerationFlag.Local, "Local packages", "", installation);
			SettingsButton(ProjectGenerationFlag.Registry, "Registry packages", "", installation);
			SettingsButton(ProjectGenerationFlag.Git, "Git packages", "", installation);
			SettingsButton(ProjectGenerationFlag.BuiltIn, "Built-in packages", "", installation);
			SettingsButton(ProjectGenerationFlag.LocalTarBall, "Local tarball", "", installation);
			SettingsButton(ProjectGenerationFlag.Unknown, "Packages from unknown sources", "", installation);
			SettingsButton(ProjectGenerationFlag.PlayerAssemblies, "Player projects", "For each player project generate an additional csproj with the name 'project.Player.csproj'", installation);
			RegenerateProjectFiles(installation);
			EditorGUI.indentLevel--;

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Zed:");
			EditorGUI.indentLevel++;

			var writeSettings = ZedSettings.WriteZedProjectSettings;
			var newWriteSettings = EditorGUILayout.Toggle(new GUIContent("Write .zed/settings.json",
				"Create .zed/settings.json in the project root so csharp-ls loads the generated solution. If the file already exists, only the solutionPathOverride value is updated."), writeSettings);
			if (newWriteSettings != writeSettings)
				ZedSettings.WriteZedProjectSettings = newWriteSettings;

			EditorGUI.indentLevel--;
		}

		private static void RegenerateProjectFiles(IVisualStudioInstallation installation)
		{
			var rect = EditorGUI.IndentedRect(EditorGUILayout.GetControlRect());
			rect.width = 252;
			if (GUI.Button(rect, "Regenerate project files"))
			{
				installation.ProjectGenerator.Sync();
			}
		}

		private static void SettingsButton(ProjectGenerationFlag preference, string guiMessage, string toolTip, IVisualStudioInstallation installation)
		{
			var generator = installation.ProjectGenerator;
			var prevValue = generator.AssemblyNameProvider.ProjectGenerationFlag.HasFlag(preference);

			var newValue = EditorGUILayout.Toggle(new GUIContent(guiMessage, toolTip), prevValue);
			if (newValue != prevValue)
				generator.AssemblyNameProvider.ToggleProjectGeneration(preference);
		}

		public void SyncIfNeeded(string[] addedFiles, string[] deletedFiles, string[] movedFiles, string[] movedFromFiles, string[] importedFiles)
		{
			if (TryGetVisualStudioInstallationForPath(CodeEditor.CurrentEditorInstallation, true, out var installation))
			{
				installation.ProjectGenerator.SyncIfNeeded(addedFiles.Union(deletedFiles).Union(movedFiles).Union(movedFromFiles), importedFiles);
			}
		}

		public void SyncAll()
		{
			if (TryGetVisualStudioInstallationForPath(CodeEditor.CurrentEditorInstallation, true, out var installation))
			{
				installation.ProjectGenerator.Sync();
			}
		}

		public bool OpenProject(string path, int line, int column)
		{
			var editorPath = CodeEditor.CurrentEditorInstallation;

			if (!TryGetVisualStudioInstallationForPath(editorPath, lookupDiscoveredInstallations: true, out var installation))
			{
				Debug.LogWarning($"[Zed] Zed executable {editorPath} is not found. Please change your settings in Edit > Preferences > External Tools.");
				return false;
			}

			var generator = installation.ProjectGenerator;
			if (!string.IsNullOrEmpty(path) && !generator.IsSupportedFile(path))
				return false;

			if (!generator.HasSolutionBeenGenerated())
				generator.Sync();

			return installation.Open(path, line, column, generator.SolutionFile());
		}
	}
}
