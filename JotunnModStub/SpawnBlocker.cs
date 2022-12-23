// SpawnBlocker
// a Valheim mod using Jötunn to avoid spawn arown specific items called Spawn Blockers
// 
// File:    SpawnBlocker.cs
// Project: SpawnBlocker

using BepInEx;
using HarmonyLib;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace SpawnBlocker
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    //[NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class SpawnBlocker : BaseUnityPlugin
    {
        public const string PluginGUID = "com.bobo.spawnblocker";
        public const string PluginName = "SpawnBlocker";
        public const string PluginVersion = "0.0.4";

        // Use this class to add your own localization to the game
        // https://valheim-modding.github.io/Jotunn/tutorials/localization.html
        public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();

        private readonly Harmony _harmony = new Harmony(PluginGUID);

        private void Awake()
        {
            // Jotunn comes with its own Logger class to provide a consistent Log style for all mods using it
            Jotunn.Logger.LogInfo("SpawnBlocker has landed");

            // To learn more about Jotunn's features, go to
            // https://valheim-modding.github.io/Jotunn/tutorials/overview.html


            PrefabManager.OnVanillaPrefabsAvailable += CreateSpawnBlocker;

            _harmony.PatchAll();
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
            PieceManager.Instance.AddPiece(customPiece);

            Jotunn.Logger.LogInfo("SpawnBlocker item created");

            // You want that to run only once, Jotunn has the piece cached for the game session
            PrefabManager.OnVanillaPrefabsAvailable -= CreateSpawnBlocker;
        }

        private static T GetSpawnBlocker<T>(Transform transform)
        {
            while (transform != null)
            {
                T c = transform.GetComponent<T>();
                if (c != null)
                    return c;
                transform = transform.parent;
            }
            return default;
        }

        public static bool AreThereNearbySpawnBlocker(Vector3 center, float range)
        {
            try
            {
                foreach (Collider collider in Physics.OverlapSphere(center, Mathf.Max(range, 0), LayerMask.GetMask(new string[] { "piece" })))
                {
                    Piece piece = GetSpawnBlocker<Piece>(collider.transform);
                    if (piece != null && piece.GetComponent<ZNetView>()?.IsValid() == true && piece.m_name == "$spawn_blocker_display_name")
                        return true;
                }
                return false;
            }
            catch { return false; }

        }

        [HarmonyPatch(typeof(MonsterAI), nameof(MonsterAI.UpdateAI))]
        static class MonsterAI__Patch
        {
            static bool Prefix(float dt, MonsterAI __instance)
            {
                if (__instance.m_tamable || __instance.IsSleeping() || __instance.IsEventCreature())
                    return true;

                if (AreThereNearbySpawnBlocker(__instance.transform.position, 75))
                {
                    Jotunn.Logger.LogInfo("MonsterAI__Patch: Destroyed mob");
                    __instance.m_nview.Destroy();
                    return false;
                }

                return true;
            }
        }
    }
}