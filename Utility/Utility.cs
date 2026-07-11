using System;
using System.Collections.Generic;

using UtilityMod;

using static UtilityMod.UtilityExtensions;

public class Utility : Mod {
    public void Start() {
        RestoreLogToggle();
        LogMessage("Mod Utility has been loaded!");
    }

    [ConsoleCommand("utility", "Area and construction utility commands")]
    public static void Command(string[] args) {
        if (args == null || args.Length == 0) {
            ShowHelp();
            return;
        }

        if (string.IsNullOrWhiteSpace(args[0])) {
            LogMessage("Empty utility command is invalid.");
            return;
        }

        if (!Commands.TryGetValue(args[0], out Action<ArraySegment<string>> action)) {
            LogMessage($"Unknown utility command: `{args[0]}`.");
            ShowHelp();
            return;
        }

        var segment = new ArraySegment<string>(args);
        if (segment.Count > 1) {
            segment = segment.Slice(1);
        }

        try {
            action(segment);
        } catch (Exception e) {
            LogMessage($"Error while executing utility command: `{e.Message}`\nStackTrace: {e.StackTrace}");
        }
    }

    private static void ShowHelp() {
        LogMessage("Utility commands:");
        LogMessage("utility FinishNearest [distance]");
        LogMessage("utility ClearArea [distance]");
        LogMessage("utility ClearPlants [distance]");
        LogMessage("utility CollectNearby [ItemID] [distance] [pile/line]");
    }

    public void OnModUnload() {
        LogMessage("Mod Utility has been unloaded!");
    }

    private static readonly Dictionary<string, Action<ArraySegment<string>>> Commands =
        new Dictionary<string, Action<ArraySegment<string>>>(StringComparer.OrdinalIgnoreCase) {
            { "FinishNearest", UtilityCommands.FinishNearest },
            { "ClearArea", UtilityCommands.ClearArea },
            { "ClearPlants", UtilityCommands.ClearPlants },
            { "CollectNearby", UtilityCommands.CollectNearby },
        };
}
