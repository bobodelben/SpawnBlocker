// SpawnBlocker
// a Valheim mod using Jötunn to avoid spawn arown specific items called Spawn Blockers
// 
// File:    SpawnBlocker.cs
// Project: SpawnBlocker

using BepInEx;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;

namespace SpawnBlocker
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    //[NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class SpawnBlocker : BaseUnityPlugin
    {
        public const string PluginGUID = "com.bobo.spawnblocker";
        public const string PluginName = "SpawnBlocker";
        public const string PluginVersion = "0.0.2";
        
        // Use this class to add your own localization to the game
        // https://valheim-modding.github.io/Jotunn/tutorials/localization.html
        public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();

        private void Awake()
        {
            // Jotunn comes with its own Logger class to provide a consistent Log style for all mods using it
            Jotunn.Logger.LogInfo("SpawnBlocker has landed");

            // To learn more about Jotunn's features, go to
            // https://valheim-modding.github.io/Jotunn/tutorials/overview.html


            PrefabManager.OnVanillaPrefabsAvailable += CreateSpawnBlocker;
        }

        private void CreateSpawnBlocker()
        {
            PieceConfig spawn_blocker = new PieceConfig();
            spawn_blocker.Name = "$spawn_blocker_display_name";
            spawn_blocker.PieceTable = "Hammer";
            spawn_blocker.Category = "Misc";
            spawn_blocker.AddRequirement(new RequirementConfig("Wood", 2, 0, true));

            PieceManager.Instance.AddPiece(new CustomPiece("spawn_blocker", "guard_stone", spawn_blocker));

            Jotunn.Logger.LogInfo("SpawnBlocker item created");

            // You want that to run only once, Jotunn has the piece cached for the game session
            PrefabManager.OnVanillaPrefabsAvailable -= CreateSpawnBlocker;
        }
    }
}