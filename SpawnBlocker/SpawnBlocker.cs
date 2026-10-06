// SpawnBlocker
// a Valheim mod using Jötunn to stop monsters from spawning around Spawn Blocker pieces
//
// File:    SpawnBlocker.cs
// Project: SpawnBlocker

using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using UnityEngine;

namespace SpawnBlocker
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    // The piece only exists with the mod installed, so everyone on a server needs it
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class SpawnBlocker : BaseUnityPlugin
    {
        public const string PluginGUID = "com.bobo.spawnblocker";
        public const string PluginName = "SpawnBlocker";
        public const string PluginVersion = "0.1.0";

        internal static ConfigEntry<float> BlockRadius;

        private readonly Harmony _harmony = new Harmony(PluginGUID);

        private void Awake()
        {
            BlockRadius = Config.Bind("General", "BlockRadius", 75f,
                new ConfigDescription("Radius in meters around a Spawn Blocker where monsters can't spawn or stay. Synced from the server.",
                    new AcceptableValueRange<float>(5f, 150f),
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

            AddLocalization();
            PrefabManager.OnVanillaPrefabsAvailable += CreateSpawnBlocker;

            _harmony.PatchAll();

            Jotunn.Logger.LogInfo("SpawnBlocker has landed");
        }

        private static void AddLocalization()
        {
            LocalizationManager.Instance.GetLocalization().AddTranslation("English", new Dictionary<string, string>
            {
                { "spawn_blocker_display_name", "Spawn Blocker" },
                { "spawn_blocker_display_description", "Keeps monsters from spawning or roaming nearby." }
            });
        }

        private void CreateSpawnBlocker()
        {
            PieceConfig spawn_blocker = new PieceConfig();
            spawn_blocker.Name = "$spawn_blocker_display_name";
            spawn_blocker.Description = "$spawn_blocker_display_description";
            spawn_blocker.PieceTable = "Hammer";
            spawn_blocker.Category = "Misc";
            spawn_blocker.AddRequirement(new RequirementConfig("Wood", 2, 0, true));

            var customPiece = new CustomPiece("spawn_blocker", "guard_stone", spawn_blocker);
            customPiece.PiecePrefab.AddComponent<SpawnBlockerArea>();
            PieceManager.Instance.AddPiece(customPiece);

            Jotunn.Logger.LogInfo("SpawnBlocker item created");

            // You want that to run only once, Jotunn has the piece cached for the game session
            PrefabManager.OnVanillaPrefabsAvailable -= CreateSpawnBlocker;
        }

        private static bool ShouldRemove(MonsterAI ai, Character character)
        {
            return character != null &&
                   !character.IsTamed() &&
                   !character.IsBoss() &&
                   !ai.IsSleeping() &&
                   !ai.IsEventCreature() &&
                   SpawnBlockerArea.IsBlocked(ai.transform.position);
        }

        // Removes monsters that walk into a blocked area or come from spawners the patches below don't cover.
        // UpdateAI only runs on the owner of the monster, so it is allowed to destroy it.
        [HarmonyPatch(typeof(MonsterAI), nameof(MonsterAI.UpdateAI))]
        private static class MonsterAI_UpdateAI_Patch
        {
            private static bool Prefix(MonsterAI __instance, Character ___m_character, ZNetView ___m_nview, ref bool __result)
            {
                if (!ShouldRemove(__instance, ___m_character))
                    return true;

                Jotunn.Logger.LogDebug($"Removing {___m_character.m_name} near a Spawn Blocker");
                ___m_nview.Destroy();
                __result = false;
                return false;
            }
        }

        // Natural spawns: skip them inside a blocked area instead of spawning and removing them.
        // Raids (eventSpawner) are left alone, like event creatures above.
        [HarmonyPatch(typeof(SpawnSystem), "Spawn")]
        private static class SpawnSystem_Spawn_Patch
        {
            private static bool Prefix(Vector3 spawnPoint, bool eventSpawner)
            {
                return eventSpawner || !SpawnBlockerArea.IsBlocked(spawnPoint);
            }
        }

        // Spawner nests (greydwarf nests, skeleton piles, ...) inside a blocked area stay quiet
        [HarmonyPatch(typeof(SpawnArea), "SpawnOne")]
        private static class SpawnArea_SpawnOne_Patch
        {
            private static bool Prefix(SpawnArea __instance, ref bool __result)
            {
                if (!SpawnBlockerArea.IsBlocked(__instance.transform.position))
                    return true;

                __result = false;
                return false;
            }
        }
    }
}
