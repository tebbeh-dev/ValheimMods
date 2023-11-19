using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System.IO;
using System.Reflection;

namespace TebArrowsQoL
{
    [BepInDependency(Jotunn.Main.ModGuid)]
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class TebArrowsQoL : BaseUnityPlugin
    {

        private const string ModName = "TebArrowQoL";
        private const string ModVersion = "0.1.0";
        private const string Author = "com.tebbeh.mod";
        private const string ModGUID = Author + "." + ModName;
        private static string ConfigFileName = ModGUID + ".cfg";
        private static string ConfigFileFullPath = BepInEx.Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;

        private readonly Harmony HarmonyInstance = new(ModGUID);

        public static readonly ManualLogSource TebLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);

        #region ConfigurationEntries

        internal static ConfigEntry<bool> AdminBypass = null!;
        internal static ConfigEntry<bool> ChanceToSaveArrowsOn = null!;
        internal static ConfigEntry<float> ChanceToSaveArrowsValue = null!;

        public static bool GetAdminBypass() => AdminBypass.Value;
        public static bool GetChanceToSaveArrowsOn() => ChanceToSaveArrowsOn.Value;
        public static float GetChanceToSaveArrowsValue() => ChanceToSaveArrowsValue.Value;

        private readonly ConfigurationManagerAttributes AdminConfig = new ConfigurationManagerAttributes { IsAdminOnly = true };
        private readonly ConfigurationManagerAttributes ClientConfig = new ConfigurationManagerAttributes { IsAdminOnly = false };

        private void AddConfig<T>(string key, string section, string description, bool synced, T value, ref ConfigEntry<T> configEntry)
        {
            string extendedDescription = GetExtendedDescription(description, synced);
            configEntry = Config.Bind(section, key, value,
                new ConfigDescription(extendedDescription, null, synced ? AdminConfig : ClientConfig));
        }
        public string GetExtendedDescription(string description, bool synchronizedSetting)
        {
            return description + (synchronizedSetting ? " [Synced with Server]" : " [Not Synced with Server]");
        }

        #endregion

        void Awake()
        {
            #region Configuration

            const string general = "General";
            AddConfig("AdminBypass", general, "True to allow admins to bypass some setting restrictions (boolean).", true, true, ref AdminBypass);

            const string misc = "Misc";
            AddConfig("ChanceToSaveArrowsOn", misc, "Turn on chance to recover arrows (bool).", true, true, ref ChanceToSaveArrowsOn);
            AddConfig("ChanceToSaveArrowsValue", misc, "Chance to save arrows. 0 - 100 would represent procentage of chance to get arrows back (float).", true, 50, ref ChanceToSaveArrowsValue);

            #endregion

            Assembly assembly = Assembly.GetExecutingAssembly();
            HarmonyInstance.PatchAll(assembly);

            SetupWatcher();
        }

        private void OnDestroy()
        {
            Config.Save();
        }
        private void SetupWatcher()
        {
            FileSystemWatcher watcher = new(BepInEx.Paths.ConfigPath, ConfigFileName);
            watcher.Changed += ReadConfigValues;
            watcher.Created += ReadConfigValues;
            watcher.Renamed += ReadConfigValues;
            watcher.IncludeSubdirectories = true;
            watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
            watcher.EnableRaisingEvents = true;
        }

        private void ReadConfigValues(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(ConfigFileFullPath)) return;
            try
            {
                TebLogger.LogDebug("Attempting to reload configuration...");
                Config.Reload();
            }
            catch
            {
                TebLogger.LogError($"There was an issue loading {ConfigFileName}");
            }
        }
    }
}