using BepInEx;
using HarmonyLib;
using BepInEx.Configuration;
using BepInEx.Logging;
using Jotunn.Utils;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;
using Jotunn.Managers;

namespace TebInfiniteTorches
{
    [BepInDependency(Jotunn.Main.ModGuid)]
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class Plugin : BaseUnityPlugin
    {

        private const string ModName = "tebbehInfiniteTorches";
        private const string ModVersion = "0.1.1";
        private const string Author = "com.tebbeh.mod";
        private const string ModGUID = Author + "." + ModName;
        private static string ConfigFileName = ModGUID + ".cfg";
        private static string ConfigFileFullPath = BepInEx.Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;

        private readonly Harmony HarmonyInstance = new(ModGUID);

        public static readonly ManualLogSource InfiniteTorchesLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);

        #region ConfigurationEntries

        internal static ConfigEntry<bool> AdminBypass = null!;
        internal static ConfigEntry<bool> infiniteFuel = null!;
        internal static ConfigEntry<bool> weatherBlock = null!;

        public static bool GetAdminBypass() => AdminBypass.Value;
        public static bool GetInfiniteFuel() => infiniteFuel.Value;
        public static bool GetWeather() => weatherBlock.Value;

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
            const string misc = "Misc";

            AddConfig("AdminBypass", general, "True to allow admins to bypass some setting restrictions (boolean).",
                true, true, ref AdminBypass);
            AddConfig("InfiniteFuel", misc, "Apply infinite to all types of fireplaces (boolean).",
                true, true, ref infiniteFuel);
            AddConfig("WeatherBlock", misc, "Apply weather block to all types of fireplaces (boolean).",
                true, true, ref weatherBlock);

            #endregion

            Assembly assembly = Assembly.GetExecutingAssembly();
            HarmonyInstance.PatchAll(assembly);

            SetupWatcher();

            /// Print config sync for debug
            /// 
            /// InfiniteTorchesLogger.LogInfo("Thanks for using my InfiniteTorches Mod");
            /// InfiniteTorchesLogger.LogInfo("AdminByPass Config set to:" + " " + Plugin.GetAdminBypass());
            /// InfiniteTorchesLogger.LogInfo("InfiniteFuel Config set to:" + " " + Plugin.GetInfiniteFuel());
            /// InfiniteTorchesLogger.LogInfo("WeatherBlock Config set to:" + " " + Plugin.GetWeather());

            ///SynchronizationManager.OnConfigurationSynchronized += (sender, args) => {
            ///    InfiniteTorchesLogger.LogInfo("AdminBypass: " + AdminBypass.Value);
            ///    InfiniteTorchesLogger.LogInfo("InfiniteFuel: " + infiniteFuel.Value);
            ///    InfiniteTorchesLogger.LogInfo("WeatherBlock: " + weatherBlock.Value);
            ///};
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
                InfiniteTorchesLogger.LogDebug("[TebInfiniteTorches] Attempting to reload configuration...");
                Config.Reload();
            }
            catch
            {
                InfiniteTorchesLogger.LogError($"[TebInfiniteTorches] There was an issue loading {ConfigFileName}");
            }
        }
    }
}