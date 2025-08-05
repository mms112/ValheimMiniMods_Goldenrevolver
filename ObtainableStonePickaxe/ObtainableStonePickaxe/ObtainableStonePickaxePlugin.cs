using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using ServerSync;
using UnityEngine;
using System.Reflection;

namespace ObtainableStonePickaxe
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class ObtainableStonePickaxePlugin : BaseUnityPlugin
    {
        public const string NAME = "Obtainable Stone Pickaxe";
        public const string VERSION = "1.0.1";
        public const string GUID = "goldenrevolver.ObtainableStonePickaxePlugin";

        internal static readonly ConfigSync configSync = new ConfigSync(GUID) { DisplayName = NAME, CurrentVersion = VERSION, MinimumRequiredVersion = VERSION };

        private static ConfigEntry<bool> configLocked;
        public static ConfigEntry<bool> UpgradeableStonePickaxe;
        public static ConfigEntry<bool> UpgradeableAntlerPickaxe;
        public static ConfigEntry<bool> BronzePickaxeUpgradeFix;

        public static ConfigEntry<bool> EnableDebugMessages;

        protected void Awake()
        {
            LoadConfig();

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
        }

        private void LoadConfig()
        {
            configLocked = config("0 - General", "LockConfiguration", true, "Configuration is locked and can be changed by server admins only.");
            configSync.AddLockingConfigEntry(configLocked);

            var sectionName = "1 - Upgrading";

            BronzePickaxeUpgradeFix = config(sectionName, nameof(BronzePickaxeUpgradeFix), true, "Whether to fix that the bronze pickaxe gains one less pickaxe damage than pierce damage per level, unlike all other pickaxes.");
            UpgradeableAntlerPickaxe = config(sectionName, nameof(UpgradeableAntlerPickaxe), true, "Enables upgrading of the antler pickaxe with the default upgrade formula.");
            UpgradeableStonePickaxe = config(sectionName, nameof(UpgradeableStonePickaxe), true, "Enables upgrading of the stone pickaxe with the default upgrade formula.");

            sectionName = "9 - Debugging";

            EnableDebugMessages = config(sectionName, nameof(EnableDebugMessages), false, "Enable this if you can't break a rock with the stone pickaxe to see why the mod thinks it's not a rock. Tell this to the mod author if you think it's a bug.");
        }

        internal static void DebugLog(string message)
        {
            UnityEngine.Debug.Log($"{NAME}: {message}");
        }

        ConfigEntry<T> config<T>(string group, string name, T defaultValue, ConfigDescription description, bool synchronizedSetting = true)
        {
            ConfigDescription extendedDescription = new ConfigDescription(
                description.Description +
                (synchronizedSetting ? " [Synced with Server]" : " [Not Synced with Server]"),
                description.AcceptableValues, description.Tags);

            ConfigEntry<T> configEntry = Config.Bind(group, name, defaultValue, extendedDescription);

            SyncedConfigEntry<T> syncedConfigEntry = configSync.AddConfigEntry(configEntry);
            syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

            return configEntry;
        }

        ConfigEntry<T> config<T>(string group, string name, T defaultValue, string description, bool synchronizedSetting = true) => config(group, name, defaultValue, new ConfigDescription(description), synchronizedSetting);
    }

    [HarmonyPatch(typeof(Attack), nameof(Attack.SpawnOnHitTerrain))]
    internal static class PatchNoDiggingGrave
    {
        static bool Prefix(ref GameObject __result, Vector3 hitPoint, ItemDrop.ItemData weapon)
        {
            if (weapon.m_shared.m_name == PatchObjectDB.stonePickaxe)
            {
                foreach (Location s_allLocation in Location.s_allLocations)
                {
                    if (s_allLocation.name.StartsWith("ShipSetting01") && s_allLocation.IsInside(hitPoint, 0f, buildCheck: true))
                    {
                        __result = null;
                        return false;
                    }
                }
            }

            return true;
        }
    }
}