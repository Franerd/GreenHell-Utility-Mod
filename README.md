# Utility

Utility is a command-based utility mod for Green Hell. It provides commands for inspecting and gathering nearby materials, clearing compatible resources and plants, completing the nearest unfinished construction, repairing the equipped tool or weapon, and slowing solo sleep.

## Requirements

- Green Hell Mod Loader
- Green Hell 2.9.5

## Installation

1. Download the latest `.ghmod` file from Releases.
2. Install it using the Green Hell Mod Loader.
3. Enable the mod.
4. Open the in-game console and use the `to` command prefix.

## Commands

### Help

```text
to Help
to Help [command]
```

Shows the general command list or detailed help for one command.

### Nearby

```text
to Nearby [distance]
```

Lists and counts supported loose resource ItemIDs within the selected radius without moving or removing them. The default distance is 10 meters.

### FinishNearest

```text
to FinishNearest [distance]
```

Requests completion of the nearest unfinished construction within the selected radius. It affects only one construction at a time.

### ClearArea

```text
to ClearArea [distance]
```

Permanently removes compatible loose resource items within the selected radius.

### ClearPlants

```text
to ClearPlants [distance]
```

Permanently removes compatible cut plant objects within the selected radius.

### Cluster

```text
to Cluster [ItemID] [distance] [pile|line]
```

Moves existing loose materials of one supported ItemID to a safe position near the player. The default distance is 10 meters and the default mode is `pile`.

### Repair

```text
to Repair
```

Restores the currently equipped weapon or tool to 100% durability. It affects only the item held by the player and does not repair other backpack items.

### SlowSleep

```text
to SlowSleep [on|off]
```

Prevents the clock from fast-forwarding while sleeping without another connected player. The selected state is remembered.

## Safety

- Commands are case-insensitive.
- The `all` option is intentionally unavailable.
- Cluster accepts only one supported ItemID at a time.
- Cluster moves existing objects and does not create new items.
- Nearby is read-only and checks the same ItemIDs accepted by Cluster.
- ClearArea and ClearPlants permanently remove compatible objects.
- Very large quantities may affect performance or cause physics instability.

## Support

For support, questions, bug reports, or suggestions, join the Discord server:

[Discord Support](https://discord.gg/NzDcYpYjFx)

## License

GNU AGPLv3
