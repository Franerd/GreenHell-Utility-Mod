using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

using HarmonyLib;
using UnityEngine;

using static UtilityMod.UtilityExtensions;

namespace UtilityMod {
    public static class UtilitySlowSleep {
        private const string SettingKey = "utility_mod_slow_sleep";
        private static bool _enabled = true;

        public static bool Enabled => _enabled;

        public static void RestoreSetting() {
            _enabled = !PlayerPrefs.HasKey(SettingKey) || PlayerPrefs.GetInt(SettingKey) != 0;
        }

        public static void Command(ArraySegment<string> args) {
            if (args.Count == 0) {
                LogMessage($"SlowSleep is {(_enabled ? "on" : "off")}.");
                return;
            }

            if (args.Count > 1) {
                LogMessage("Too many parameters. Use: to SlowSleep [on/off]");
                return;
            }

            string value = args[0];
            if (value.Equals("on", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("true", StringComparison.OrdinalIgnoreCase)) {
                SetEnabled(true);
            } else if (value.Equals("off", StringComparison.OrdinalIgnoreCase) ||
                       value.Equals("false", StringComparison.OrdinalIgnoreCase)) {
                SetEnabled(false);
            } else {
                LogMessage($"Invalid SlowSleep state: `{value}`. Use `on` or `off`.");
            }
        }

        private static void SetEnabled(bool enabled) {
            _enabled = enabled;
            PlayerPrefs.SetInt(SettingKey, enabled ? 1 : 0);
            PlayerPrefs.Save();
            LogMessage($"SlowSleep is now {(enabled ? "on" : "off")}.");
        }

        // SleepController normally treats a session with no remote peer as solo,
        // completing eight hours of sleep in only a few real-time seconds. Inside
        // the patched sleep methods this replacement selects the slower co-op path.
        public static bool UseVanillaSoloSleep() {
            return ReplTools.IsPlayingAlone() && !_enabled;
        }

        public static IEnumerable<CodeInstruction> ReplaceSoloSleepChecks(
            IEnumerable<CodeInstruction> instructions) {
            MethodInfo vanillaCheck = AccessTools.Method(typeof(ReplTools), "IsPlayingAlone");
            MethodInfo replacement = AccessTools.Method(typeof(UtilitySlowSleep), "UseVanillaSoloSleep");

            foreach (CodeInstruction instruction in instructions) {
                if (instruction.opcode == OpCodes.Call && Equals(instruction.operand, vanillaCheck)) {
                    instruction.operand = replacement;
                }
                yield return instruction;
            }
        }
    }

    [HarmonyPatch(typeof(SleepController), "UpdateSleeping")]
    internal static class SlowSleepUpdatePatch {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            return UtilitySlowSleep.ReplaceSoloSleepChecks(instructions);
        }
    }

    [HarmonyPatch(typeof(SleepController), "WakeUp")]
    internal static class SlowSleepWakeUpPatch {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            return UtilitySlowSleep.ReplaceSoloSleepChecks(instructions);
        }
    }

    [HarmonyPatch(typeof(SleepController), "IsAllPlayersSleeping")]
    internal static class SlowSleepAllPlayersPatch {
        private static void Postfix(ref bool __result) {
            if (UtilitySlowSleep.Enabled && ReplTools.IsPlayingAlone()) {
                __result = false;
            }
        }
    }
}
