using System;
using System.Collections.Generic;
using System.Reflection;

using HarmonyLib;

using UtilityMod;

using static UtilityMod.UtilityExtensions;

public class Utility : Mod {
    private const string HarmonyId = "com.franerd.greenhell.utility";
    private Harmony _harmony;

    public void Start() {
        RestoreLogToggle();
        UtilitySlowSleep.RestoreSetting();
        _harmony = new Harmony(HarmonyId);
        _harmony.PatchAll(Assembly.GetExecutingAssembly());
        LogMessage("Mod Utility has been loaded!");
    }

    [ConsoleCommand("to", "Area, construction and sleep utility commands")]
    public static void Command(string[] args) {
        if (args == null || args.Length == 0) {
            ShowHelp();
            return;
        }

        if (string.IsNullOrWhiteSpace(args[0])) {
            LogMessage("Empty `to` command is invalid.");
            return;
        }

        if (!Commands.TryGetValue(args[0], out Action<ArraySegment<string>> action)) {
            LogMessage($"Unknown `to` command: `{args[0]}`.");
            ShowHelp();
            return;
        }

        // Always remove the command name before forwarding its parameters.
        // This also produces an empty segment for commands without parameters.
        var segment = new ArraySegment<string>(args, 1, args.Length - 1);

        try {
            action(segment);
        } catch (Exception e) {
            LogMessage($"Error while executing `to` command: `{e.Message}`\nStackTrace: {e.StackTrace}");
        }
    }

    private static void ShowHelp() {
        LogMessage("TO commands:");
        LogMessage("to Help [command]");
        LogMessage("to Nearby [distance]");
        LogMessage("to FinishNearest [distance]");
        LogMessage("to ClearArea [distance]");
        LogMessage("to ClearPlants [distance]");
        LogMessage("to Cluster [ItemID] [distance] [pile/line]");
        LogMessage("to Repair");
        LogMessage("to SlowSleep [on/off]");
        LogMessage("Use `to Help [command]` for detailed help.");
    }

    private static void ShowHelp(ArraySegment<string> args) {
        if (args.Count == 0) {
            ShowHelp();
            return;
        }

        if (args.Count > 1) {
            LogMessage("Too many parameters. Use: to Help [command]");
            return;
        }

        string topic = args[0];
        if (topic.Equals("Help", StringComparison.OrdinalIgnoreCase)) {
            LogMessage("to Help [command]");
            LogMessage("Shows the general command list or detailed help for one command.");
        } else if (topic.Equals("Nearby", StringComparison.OrdinalIgnoreCase)) {
            LogMessage("Nearby lists supported loose resource ItemIDs found within the selected radius without moving or removing them.");
            LogMessage("Only ItemIDs supported by Cluster are counted.");
            LogMessage("to Nearby [distance]");
            LogMessage("Default distance: 10 meters.");
        } else if (topic.Equals("FinishNearest", StringComparison.OrdinalIgnoreCase)) {
            LogMessage("FinishNearest finds the nearest unfinished construction within the selected radius and requests its completion.");
            LogMessage("It affects only one construction at a time.");
            LogMessage("to FinishNearest [distance]");
            LogMessage("Default distance: 10 meters.");
        } else if (topic.Equals("ClearArea", StringComparison.OrdinalIgnoreCase)) {
            LogMessage("ClearArea permanently removes compatible resource items registered within the selected radius.");
            LogMessage("Removed ItemIDs: Long_Stick, Stick, Small_Stick, Log, Stone, Big_Stone, Palm_Leaf, Banana_Leaf, Bamboo_Long_Stick, Bamboo_Stick, Bamboo_Log, Coconut_Green, Dry_leaf, Small_leaf_pile, Bone");
            LogMessage("to ClearArea [distance]");
            LogMessage("Default distance: 10 meters.");
        } else if (topic.Equals("ClearPlants", StringComparison.OrdinalIgnoreCase)) {
            LogMessage("ClearPlants permanently removes compatible cut plant objects within the selected radius.");
            LogMessage("Affected objects: small_plant_*_cut, medium_plant_*_cut and branch_dead_a_cut.");
            LogMessage("to ClearPlants [distance]");
            LogMessage("Default distance: 10 meters.");
        } else if (topic.Equals("Cluster", StringComparison.OrdinalIgnoreCase)) {
            LogMessage("Cluster moves existing loose materials within the selected radius to a safe position near the player.");
            LogMessage("It does not spawn items and accepts only one ItemID at a time.");
            LogMessage("to Cluster [ItemID] [distance] [pile/line]");
            LogMessage("Default distance: 10 meters. Default mode: pile.");
            LogMessage("Main ItemIDs: Long_Stick, Stick, Small_Stick, Log, Stone, Big_Stone, Palm_Leaf, Banana_Leaf, Bamboo_Long_Stick, Bamboo_Stick, Bamboo_Log, Coconut_Green, mud_to_build, mud_from_water, Bone");
            LogMessage("Experimental ItemIDs: Dry_leaf, Small_leaf_pile, Rope, Fiber, Wood_Resin, Charcoal");
        } else if (topic.Equals("Repair", StringComparison.OrdinalIgnoreCase)) {
            LogMessage("Repair restores the currently equipped weapon or tool to 100% durability.");
            LogMessage("It affects only the item held by the player and does not repair the backpack.");
            LogMessage("to Repair");
        } else if (topic.Equals("SlowSleep", StringComparison.OrdinalIgnoreCase)) {
            LogMessage("SlowSleep prevents the clock from fast-forwarding when sleeping without another connected player.");
            LogMessage("Energy and other sleep effects progress at the same slower rate used while another co-op player is awake.");
            LogMessage("to SlowSleep [on/off]");
            LogMessage("Default: on. The selected state is remembered.");
        } else {
            LogMessage($"Unknown help topic: `{topic}`.");
            LogMessage("Use: to Help");
        }
    }

    public void OnModUnload() {
        if (_harmony != null) {
            _harmony.UnpatchAll(HarmonyId);
        }
        LogMessage("Mod Utility has been unloaded!");
    }

    private static readonly Dictionary<string, Action<ArraySegment<string>>> Commands =
        new Dictionary<string, Action<ArraySegment<string>>>(StringComparer.OrdinalIgnoreCase) {
            { "Help", ShowHelp },
            { "Nearby", UtilityCommands.Nearby },
            { "FinishNearest", UtilityCommands.FinishNearest },
            { "ClearArea", UtilityCommands.ClearArea },
            { "ClearPlants", UtilityCommands.ClearPlants },
            { "Cluster", UtilityCommands.Cluster },
            { "Repair", UtilityCommands.Repair },
            { "SlowSleep", UtilitySlowSleep.Command },
        };
}
