/*---------------------------------------------------------------------------------------------
 *  Zed Editor integration for Unity.
 *  Licensed under the MIT License. See LICENSE.md in the package root for license information.
 *--------------------------------------------------------------------------------------------*/
using UnityEditor;

namespace Zed.Unity.Editor
{
	internal static class ZedSettings
	{
		internal const string WriteZedProjectSettingsKey = "zed_write_project_settings_json";

		/// <summary>Write .zed/settings.json in the project root pointing csharp-ls at the generated solution. Default: on.</summary>
		public static bool WriteZedProjectSettings
		{
			get => EditorPrefs.GetBool(WriteZedProjectSettingsKey, true);
			set => EditorPrefs.SetBool(WriteZedProjectSettingsKey, value);
		}
	}
}
