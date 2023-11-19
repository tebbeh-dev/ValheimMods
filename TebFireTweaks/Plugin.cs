using BepInEx;
using HarmonyLib;
using BepInEx.Configuration;
using BepInEx.Logging;
using System.IO;
using System.Reflection;

namespace TebFireTweaks
{
    [BepInDependency(Jotunn.Main.ModGuid)]
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class TebFireTweaks : BaseUnityPlugin
    {

        private const string ModName = "TebFireTweaks";
        private const string ModVersion = "0.1.0";
        private const string Author = "com.tebbeh.mod";
        private const string ModGUID = Author + "." + ModName;
        private static string ConfigFileName = ModGUID + ".cfg";
        private static string ConfigFileFullPath = BepInEx.Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;

        private readonly Harmony HarmonyInstance = new(ModGUID);

        public static readonly ManualLogSource TebLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);

        #region ConfigurationEntries

        internal static ConfigEntry<bool> AdminBypass = null!;
        internal static ConfigEntry<bool> FireplaceInfinite = null!;
        internal static ConfigEntry<bool> FireplaceWeatherBlock = null!;
        internal static ConfigEntry<bool> CookingStationInfinite = null!;
        internal static ConfigEntry<float> CookingCookingSpeedInfinite = null!;
        internal static ConfigEntry<bool> SmelterInfinite = null!;

        public static bool GetAdminBypass() => AdminBypass.Value;
        public static bool GetFireplaceInfinite() => FireplaceInfinite.Value;
        public static bool GetFireplaceWeatherBlock() => FireplaceWeatherBlock.Value;
        public static bool GetCookingStationInfinite() => CookingStationInfinite.Value;
        public static float GetCookingCookingSpeedInfinite() => CookingCookingSpeedInfinite.Value;
        public static bool GetSmelterInfinite() => SmelterInfinite.Value;

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
            AddConfig("FireplaceInfinite", misc, "Apply infinite fuel to all types of fireplaces (boolean).", true, true, ref FireplaceInfinite);
            AddConfig("FireplaceWeatherBlock", misc, "Apply weather block to all types of fireplaces (boolean).", true, true, ref FireplaceWeatherBlock);
            AddConfig("CookingStationInfinite", misc, "Apply infinite fuel to all types of cooking stations (boolean).", true, true, ref CookingStationInfinite);
            AddConfig("CookingStationCookingSpeed", misc, "Change cooking speed of cooking stations. -50 will reduce time with 50%. Max is -100, less could cause bugs. (float).", true, 0, ref CookingCookingSpeedInfinite);
            AddConfig("SmelterInfinite", misc, "Apply infinite fuel to all types of smelters (boolean).", true, true, ref SmelterInfinite);

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