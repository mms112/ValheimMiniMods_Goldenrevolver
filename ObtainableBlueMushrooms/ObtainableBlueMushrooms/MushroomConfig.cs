using BepInEx.Configuration;
using ServerSync;
using System.Xml.Linq;
using System;
using static ObtainableBlueMushrooms.ConfigurationManagerAttributes;
using static ObtainableBlueMushrooms.ObtainableBlueMushroomsPlugin;

namespace ObtainableBlueMushrooms
{
    internal class MushroomConfig
    {
        internal static readonly ConfigSync configSync = new ConfigSync(GUID) { DisplayName = NAME, CurrentVersion = VERSION, MinimumRequiredVersion = VERSION };

        private static ConfigEntry<bool> configLocked;
        public static ConfigEntry<bool> EnablePlantingInCaves;
        public static ConfigEntry<PlantingTool> MushroomPlantingTool;

        internal static void LoadConfig(ConfigFile configF)
        {
            configLocked = config(configF, "General", "LockConfiguration", true, "Configuration is locked and can be changed by server admins only.");
            configSync.AddLockingConfigEntry(configLocked);

            var sectionName = "Mushroom Planting";

            EnablePlantingInCaves = config(configF, sectionName, nameof(EnablePlantingInCaves), true, CustomSeeOnlyDisplay());
            MushroomPlantingTool = config(configF, sectionName, nameof(MushroomPlantingTool), PlantingTool.Cultivator, CustomHiddenDisplay("Whether you plant blue mushrooms with the hammer or the cultivator."));
        }

        public enum LootChange
        {
            Disabled,
            AddToExistingDrops,
            ReplaceLeatherScraps
        }

        public enum PlantingTool
        {
            Hammer,
            Cultivator
        }

        static ConfigEntry<T> config<T>(ConfigFile configF, string group, string name, T defaultValue, ConfigDescription description, bool synchronizedSetting = true)
        {
            ConfigDescription extendedDescription = new ConfigDescription(
                description.Description +
                (synchronizedSetting ? " [Synced with Server]" : " [Not Synced with Server]"),
                description.AcceptableValues, description.Tags);

            ConfigEntry<T> configEntry = configF.Bind(group, name, defaultValue, extendedDescription);

            SyncedConfigEntry<T> syncedConfigEntry = configSync.AddConfigEntry(configEntry);
            syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

            return configEntry;
        }

        static ConfigEntry<T> config<T>(ConfigFile configF, string group, string name, T defaultValue, string description, bool synchronizedSetting = true) => config(configF, group, name, defaultValue, new ConfigDescription(description), synchronizedSetting);
    }
}