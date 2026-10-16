/*---------------------------------------------------------------------------------------------
 *  Zed Editor integration for Unity.
 *  Licensed under the MIT License. See LICENSE.md in the package root for license information.
 *--------------------------------------------------------------------------------------------*/
using UnityEditor;

namespace Zed.Unity.Editor
{
	internal static class ZedSettings
	{
		internal const string GenerateZedSolutionKey = "zed_generate_zed_sln";
		internal const string WriteZedProjectSettingsKey = "zed_write_project_settings_json";

		/// <summary>Also write &lt;ProjectName&gt;.zed.sln (without *.Player projects) on every project regeneration. Default: on.</summary>
		public static bool GenerateZedSolution
		{
			get => EditorPrefs.GetBool(GenerateZedSolutionKey, true);
			set => EditorPrefs.SetBool(GenerateZedSolutionKey, value);
		}

		/// <summary>Write .zed/settings.json in the project root (only if it does not exist yet). Default: off.</summary>
		public static bool WriteZedProjectSettings
		{
			get => EditorPrefs.GetBool(WriteZedProjectSettingsKey, false);
			set => EditorPrefs.SetBool(WriteZedProjectSettingsKey, value);
		}
	}
}
