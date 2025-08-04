using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.Reflection;
using ServerSync;

namespace SimpleSetAndCapeBonuses
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class SimpleSetAndCapeBonusesPlugin : BaseUnityPlugin
    {
        public const string GUID = "goldenrevolver.SimpleSetAndCapeBonuses";
        public const string NAME = "Simple New Set and Cape Bonuses";
        public const string VERSION = "1.0.3";

        internal static readonly ConfigSync configSync = new ConfigSync(GUID) { DisplayName = NAME, CurrentVersion = VERSION, MinimumRequiredVersion = VERSION };

        private static ConfigEntry<bool> configLocked;

        public static ConfigEntry<bool> EnableLeatherArmorSetBonus;
        public static ConfigEntry<bool> EnableForagerArmorSetBonus;
        public static ConfigEntry<bool> EnableTrollArmorSetBonusChange;
        public static ConfigEntry<bool> EnableCapeBuffs;

        public static ConfigEntry<bool> EnableSetBonusIcons;
        public static ConfigEntry<bool> AlwaysEnableMidsummerCrownRecipe;

        public static ConfigEntry<float> ForagerSetBonusExtraChance;

        protected void Awake()
        {
            LoadConfig();

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
        }

        private void LoadConfig()
        {
            configLocked = config("General", "LockConfiguration", true, "Configuration is locked and can be changed by server admins only.");
            configSync.AddLockingConfigEntry(configLocked);

            var sectionName = "0 - Requires Restart";

            EnableTrollArmorSetBonusChange = config(sectionName, nameof(EnableTrollArmorSetBonusChange), true, "Removes the troll cape from the Troll Set.");
            EnableLeatherArmorSetBonus = config(sectionName, nameof(EnableLeatherArmorSetBonus), true, "Enables the set bonus for the Leather Set.");
            EnableForagerArmorSetBonus = config(sectionName, nameof(EnableForagerArmorSetBonus), true, "Enables the set bonus for the Rag Set.");
            EnableCapeBuffs = config(sectionName, nameof(EnableCapeBuffs), true, "Enables additional buffs for some capes.");

            EnableSetBonusIcons = config(sectionName, nameof(EnableSetBonusIcons), true, "Enables icons for the new set boni.", false);
            AlwaysEnableMidsummerCrownRecipe = config(sectionName, nameof(AlwaysEnableMidsummerCrownRecipe), true, "Enables the midsummer crown in all seasons.");

            sectionName = "1 - No Restart Required";

            ForagerSetBonusExtraChance = config(sectionName, nameof(ForagerSetBonusExtraChance), 0.5f, "The change of Forager granting an extra item.");
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
}