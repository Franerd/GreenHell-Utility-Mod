using System;
using System.Collections.Generic;

using Enums;
using UnityEngine;

using static UtilityMod.UtilityExtensions;

namespace UtilityMod {
    public static class UtilityCommands {
        private static readonly HashSet<ItemID> ClearAreaIds = new HashSet<ItemID> {
            ItemID.Long_Stick,
            ItemID.Stick,
            ItemID.Small_Stick,
            ItemID.Log,
            ItemID.Stone,
            ItemID.Big_Stone,
            ItemID.Palm_Leaf,
            ItemID.Banana_Leaf,
            ItemID.Bamboo_Long_Stick,
            ItemID.Bamboo_Stick,
            ItemID.Bamboo_Log,
            ItemID.Coconut_Green,
            ItemID.Dry_leaf,
            ItemID.Small_leaf_pile,
            ItemID.Liane,
            ItemID.Dryed_Liane
        };

        // Closed allow-list for CollectNearby. Even valid Green Hell ItemIDs are rejected
        // unless explicitly included here, preventing unsafe movement of inventory,
        // quest, equipped or construction-bound objects.
        private static readonly HashSet<string> CollectNearbyAllowedIds =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase) {
                "Long_Stick",
                "Stick",
                "Small_Stick",
                "Log",
                "Stone",
                "Big_Stone",
                "Palm_Leaf",
                "Banana_Leaf",
                "Bamboo_Long_Stick",
                "Bamboo_Stick",
                "Bamboo_Log",
                "Coconut_Green",
                "Dry_leaf",
                "Small_leaf_pile",
                "Liane",
                "Dryed_Liane",
                "mud_to_build",
                "mud_from_water",
                "Rope",
                "Fiber",
                "Wood_Resin",
                "Charcoal",
                "Bone"
            };

        private static bool TryReadDistance(ArraySegment<string> args, float defaultDistance, out float distance) {
            distance = defaultDistance;
            if (args.Count < 1) {
                return true;
            }

            if (!float.TryParse(args[0], out distance) || distance <= 0f) {
                LogMessage($"Invalid distance: `{args[0]}`. Distance must be greater than zero.");
                return false;
            }
            return true;
        }

        private static void DestroyItem(Item item) {
            item.m_Info.m_CanBeRemovedFromInventory = true;
            item.m_Info.m_DestroyByItemsManager = true;
            item.m_Info.m_CantDestroy = false;
            ItemsManager.Get().AddItemToDestroy(item);
        }

        // FinishNearest [maxDistance(Default=10)]
        public static void FinishNearest(ArraySegment<string> args) {
            if (!TryReadDistance(args, 10f, out float maxDistance)) {
                return;
            }

            var manager = ConstructionGhostManager.Get();
            var player = Player.Get();
            if (manager == null || player == null) {
                LogMessage("Player or ConstructionGhostManager is not available.");
                return;
            }

            var playerPosition = player.GetWorldPosition();
            ConstructionGhost nearest = null;
            float nearestDistance = float.MaxValue;

            foreach (var ghost in manager.GetAll()) {
                if (ghost == null) {
                    continue;
                }

                float distance = Vector3.Distance(playerPosition, ghost.transform.position);
                if (distance <= maxDistance && distance < nearestDistance) {
                    nearest = ghost;
                    nearestDistance = distance;
                }
            }

            if (nearest == null) {
                LogMessage($"No unfinished construction found within {maxDistance} meters.");
                return;
            }

            nearest.m_CurrentStep = nearest.m_Steps.Count;
            LogMessage($"Nearest construction completed at distance {nearestDistance:0.00} meters.");
        }

        // ClearArea [maxDistance(Default=10)]
        public static void ClearArea(ArraySegment<string> args) {
            if (!TryReadDistance(args, 10f, out float maxDistance)) {
                return;
            }

            var player = Player.Get();
            if (player == null) {
                LogMessage("Player is not available.");
                return;
            }

            var playerPosition = player.GetWorldPosition();
            var found = new List<Item>();

            foreach (var item in Item.s_AllItems) {
                if (item == null || item.m_Info == null || !ClearAreaIds.Contains(item.m_Info.m_ID)) {
                    continue;
                }

                if (Vector3.Distance(playerPosition, item.transform.position) <= maxDistance) {
                    found.Add(item);
                }
            }

            foreach (var item in found) {
                DestroyItem(item);
            }

            LogMessage($"ClearArea removed {found.Count} loose resource items within {maxDistance} meters.");
        }

        // ClearPlants [maxDistance(Default=10)]
        public static void ClearPlants(ArraySegment<string> args) {
            if (!TryReadDistance(args, 10f, out float maxDistance)) {
                return;
            }

            var player = Player.Get();
            if (player == null) {
                LogMessage("Player is not available.");
                return;
            }

            var playerPosition = player.GetWorldPosition();
            var found = new List<Item>();

            foreach (var item in Item.s_AllItems) {
                if (item == null || item.m_Info == null) {
                    continue;
                }

                string id = item.m_Info.m_ID.ToString();
                bool isPlant =
                    (id.StartsWith("small_plant_", StringComparison.OrdinalIgnoreCase) ||
                     id.StartsWith("medium_plant_", StringComparison.OrdinalIgnoreCase) ||
                     id.Equals("branch_dead_a_cut", StringComparison.OrdinalIgnoreCase)) &&
                    id.EndsWith("_cut", StringComparison.OrdinalIgnoreCase);

                if (!isPlant) {
                    continue;
                }

                if (Vector3.Distance(playerPosition, item.transform.position) <= maxDistance) {
                    found.Add(item);
                }
            }

            foreach (var item in found) {
                DestroyItem(item);
            }

            LogMessage($"ClearPlants removed {found.Count} removable plant objects within {maxDistance} meters.");
        }

        private static void MoveLooseItem(Item item, Vector3 targetPosition) {
            // Loose/heavy resources can be controlled by a parent transform and Rigidbody.
            // Detach the item and update both the Transform and every Rigidbody in its hierarchy.
            item.transform.SetParent(null, true);
            item.transform.position = targetPosition;

            var rigidbodies = item.GetComponentsInChildren<Rigidbody>(true);
            foreach (var rigidbody in rigidbodies) {
                if (rigidbody == null) {
                    continue;
                }
                rigidbody.position = targetPosition;
                rigidbody.velocity = Vector3.zero;
                rigidbody.angularVelocity = Vector3.zero;
                rigidbody.WakeUp();
            }

            var parentRigidbodies = item.GetComponentsInParent<Rigidbody>(true);
            foreach (var rigidbody in parentRigidbodies) {
                if (rigidbody == null) {
                    continue;
                }
                rigidbody.position = targetPosition;
                rigidbody.velocity = Vector3.zero;
                rigidbody.angularVelocity = Vector3.zero;
                rigidbody.WakeUp();
            }
        }

        private static void GetPileSettings(ItemID itemId, out float spacing, out int itemsPerLayer, out float layerHeight) {
            string id = itemId.ToString();

            spacing = 0.32f;
            itemsPerLayer = 12;
            layerHeight = 0.18f;

            if (id.Equals("Small_Stick", StringComparison.OrdinalIgnoreCase)) {
                spacing = 0.20f;
                itemsPerLayer = 16;
                layerHeight = 0.10f;
            } else if (id.Equals("Stick", StringComparison.OrdinalIgnoreCase)) {
                spacing = 0.34f;
                itemsPerLayer = 12;
                layerHeight = 0.18f;
            } else if (id.Equals("Long_Stick", StringComparison.OrdinalIgnoreCase) ||
                       id.Equals("Bamboo_Long_Stick", StringComparison.OrdinalIgnoreCase)) {
                spacing = 0.55f;
                itemsPerLayer = 10;
                layerHeight = 0.24f;
            } else if (id.Equals("Log", StringComparison.OrdinalIgnoreCase) ||
                       id.Equals("Bamboo_Log", StringComparison.OrdinalIgnoreCase)) {
                spacing = 0.85f;
                itemsPerLayer = 8;
                layerHeight = 0.38f;
            } else if (id.Equals("Palm_Leaf", StringComparison.OrdinalIgnoreCase) ||
                       id.Equals("Banana_Leaf", StringComparison.OrdinalIgnoreCase)) {
                spacing = 0.48f;
                itemsPerLayer = 10;
                layerHeight = 0.16f;
            } else if (id.Equals("Big_Stone", StringComparison.OrdinalIgnoreCase) ||
                       id.Equals("Coconut_Green", StringComparison.OrdinalIgnoreCase) ||
                       id.Equals("mud_to_build", StringComparison.OrdinalIgnoreCase)) {
                spacing = 0.42f;
                itemsPerLayer = 10;
                layerHeight = 0.28f;
            }
        }

        private static Vector3 GetSafePileOffset(int index, ItemID itemId, Transform playerTransform) {
            GetPileSettings(itemId, out float spacing, out int itemsPerLayer, out float layerHeight);

            int layer = index / itemsPerLayer;
            int slot = index % itemsPerLayer;

            // Concentric rings: no two items receive the exact same spawn point.
            int ring = slot == 0 ? 0 : 1 + ((slot - 1) / 6);
            int slotInRing = slot == 0 ? 0 : (slot - 1) % 6;
            float angle = slotInRing * 60f * Mathf.Deg2Rad + layer * 0.27f;
            float radius = ring * spacing;

            Vector3 horizontal = playerTransform.right * (Mathf.Cos(angle) * radius) +
                                 playerTransform.forward * (Mathf.Sin(angle) * radius);
            return horizontal + Vector3.up * (0.35f + layer * layerHeight);
        }

        // CollectNearby [ItemID] [maxDistance(Default=10)] [pile/line]
        // Only IDs in CollectNearbyAllowedIds are accepted.
        // Examples:
        // CollectNearby Stick 20              -> safe compact pile (default)
        // CollectNearby Stick 20 pile         -> safe compact pile
        // CollectNearby Stick 20 line         -> previous grid/line behavior
        public static void CollectNearby(ArraySegment<string> args) {
            const float defaultDistance = 10f;

            if (args.Count < 1 || string.IsNullOrWhiteSpace(args[0])) {
                LogMessage("ItemID is required. Example: utility CollectNearby Stick 20");
                return;
            }

            if (args[0].Equals("all", StringComparison.OrdinalIgnoreCase)) {
                LogMessage("The `all` option was removed for safety. Specify one supported ItemID.");
                return;
            }

            if (!args[0].ParseEnum(out ItemID requestedItemId)) {
                LogMessage($"ItemID `{args[0]}` does not exist. Example: utility CollectNearby Stick 20");
                return;
            }

            string requestedName = requestedItemId.ToString();
            if (!CollectNearbyAllowedIds.Contains(requestedName)) {
                LogMessage($"ItemID `{requestedName}` is not allowed by CollectNearby for safety.");
                return;
            }

            float maxDistance = defaultDistance;
            if (args.Count > 1 && (!float.TryParse(args[1], out maxDistance) || maxDistance <= 0f)) {
                LogMessage($"Invalid distance: `{args[1]}`. Distance must be greater than zero.");
                return;
            }

            string mode = args.Count > 2 ? args[2].ToLowerInvariant() : "pile";
            if (mode != "pile" && mode != "line") {
                LogMessage($"Invalid mode: `{mode}`. Use `pile` or `line`.");
                return;
            }

            if (args.Count > 3) {
                LogMessage("Too many parameters. Use: utility CollectNearby [ItemID] [distance] [pile/line]");
                return;
            }

            var player = Player.Get();
            if (player == null) {
                LogMessage("Player is not available.");
                return;
            }

            var playerTransform = player.transform;
            var playerPosition = player.GetWorldPosition();
            var found = new List<Item>();

            foreach (var item in Item.s_AllItems) {
                if (item == null || item.m_Info == null || !item.m_Info.m_ID.Equals(requestedItemId)) {
                    continue;
                }

                if (Vector3.Distance(playerPosition, item.transform.position) <= maxDistance) {
                    found.Add(item);
                }
            }

            Vector3 pileCenter = playerPosition + playerTransform.forward * 3f;

            for (int i = 0; i < found.Count; i++) {
                var item = found[i];
                Vector3 target;

                if (mode == "line") {
                    int column = i % 6;
                    int row = i / 6;
                    var offset = playerTransform.forward * (3f + row * 0.8f) +
                                 playerTransform.right * ((column - 2.5f) * 0.8f);
                    target = playerPosition + offset + Vector3.up * 0.6f;
                } else {
                    target = pileCenter + GetSafePileOffset(i, requestedItemId, playerTransform);
                }

                MoveLooseItem(item, target);
            }

            LogMessage($"CollectNearby moved {found.Count} x {requestedName} in `{mode}` mode from within {maxDistance} meters.");
        }
    }
}
