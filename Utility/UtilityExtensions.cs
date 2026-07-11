using System;
using System.IO;
using UnityEngine;

namespace UtilityMod
{
	public static class UtilityExtensions
	{
		// parses the item id from the string, disregards case, and verifies result is defined
		public static bool ParseEnum<T>(this string arg, out T @enum) where T : struct, Enum
		{
			return Enum.TryParse(arg, true, out @enum) && Enum.IsDefined(typeof(T), @enum);
		}

		// compares two strings disregarding case
		public static bool EqualsInsensitive(this string a, string b)
		{
			return string.Compare(a, b, true) == 0;
		}

		public static readonly string DesktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

		private static readonly string _logPath = Path.Combine(DesktopPath, "UtilityLog.log");

		private static bool _logToDesktop;

		private static readonly string _dataPath = Path.Combine(Directory.GetParent(Application.dataPath).FullName,
			"mods",
			"ModData");

		public static string CalculatePath(this string fileName)
		{
			return Path.Combine(_dataPath, fileName);
		}

		private const string _logToggleKey = "utility_mod_logToDesktop";

		public static void RestoreLogToggle()
		{
			if (!PlayerPrefs.HasKey(_logToggleKey))
			{
				_logToDesktop = false;
				return;
			}
			_logToDesktop = Convert.ToBoolean(PlayerPrefs.GetInt(_logToggleKey));
		}

		public static void ToggleLogToDesktop(ArraySegment<string> args)
		{
			_logToDesktop = !_logToDesktop;
			PlayerPrefs.SetInt(_logToggleKey, Convert.ToInt32(_logToDesktop));
			LogMessage($"Log to desktop: {_logToDesktop}");
		}

		public static void LogMessage(string message)
		{
			Debug.Log(message);
			if (!_logToDesktop)
			{
				return;
			}
			var time = DateTime.Now.ToString();
			var output = string.Format("[{0}]: {1}\n", time, message);
			File.AppendAllText(_logPath, output);
		}
	}
}