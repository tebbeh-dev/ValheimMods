using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using System.Net;

namespace Tebbeh.TebConnectInfo
{
    [BepInDependency(Jotunn.Main.ModGuid)]
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class TebConnectInfo : BaseUnityPlugin
    {
        private const string ModName = "TebConnectInfo";
        private const string ModVersion = "0.1.0";
        private const string Author = "com.tebbeh.mod";
        private const string ModGUID = Author + "." + ModName;
        private static string ConfigFileName = ModGUID + ".cfg";
        private static string ConfigFileFullPath = BepInEx.Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;

        private readonly Harmony HarmonyInstance = new(ModGUID);

        public static readonly ManualLogSource TebLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);

        #region ConfigurationEntries

        internal static ConfigEntry<string> WebhookURL = null!;
        internal static ConfigEntry<string> WebhookName = null!;
        internal static ConfigEntry<string> joinMessage = null!;
        internal static ConfigEntry<string> leaveMessage = null!;

        public static string GetWebhookURL() => WebhookURL.Value;
        public static string GetWebhookName() => WebhookName.Value;
        public static string GetjoinMessage() => joinMessage.Value;
        public static string GetleaveMessage() => leaveMessage.Value;

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
            const string messages = "Messages";

            AddConfig("WebhookURL", general, "Webhook url (string).", true, "", ref WebhookURL);
            AddConfig("WebhookName", general, "Webhook send as username (string).", true, "", ref WebhookName);
            AddConfig("JoinMessage", messages, "Join message to post with webhook, manipulate after your needs. Standard is for Discord! (string).", true, ":green_circle: **{player}** has come online!", ref joinMessage);
            AddConfig("LeaveMessage", messages, "Leave message to post with webhook, manipulate after your needs. Standard is for Discord! (string).", true, ":red_circle: **{player}** gone offline!", ref leaveMessage);

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


        private static void PostToDiscord(string content)
        {
            string webhookURL = WebhookURL.Value;
            string webhookUsername = WebhookName.Value;

            if (content == "" || webhookURL == "")
            {
                return;
            }

            WebRequest discordAPI = WebRequest.Create(webhookURL);
            discordAPI.Method = "POST";
            discordAPI.ContentType = "application/json";

            discordAPI.GetRequestStreamAsync().ContinueWith(t =>
            {
                static string escape(string s) => s.Replace("\\", "\\\\").Replace("\"", "\\\"");

                using StreamWriter writer = new(t.Result);
                string json = @"{""content"":""" + escape(content) + @"""" + (webhookUsername == "" ? "" : @", ""username"":""" + escape(webhookUsername) + @"""") + "}";
                writer.WriteAsync(json).ContinueWith(_ => discordAPI.GetResponseAsync());
            });
        }

        internal class GetConnectInfo
        {

            [HarmonyPatch(typeof(Chat), nameof(Chat.OnNewChatMessage))]
            private class OnConnect
            {
                private static void Prefix(ref UserInfo user)
                {
                    string playername = user.Name;
                    if (string.IsNullOrEmpty(playername))
                    {
                        return;
                    }

                    string postdata = TebConnectInfo.GetjoinMessage();
                    postdata = postdata.Replace("{player}", playername);

                    PostToDiscord(postdata);
                    
                }
            }

            [HarmonyPatch(typeof(ZNet), nameof(ZNet.RPC_Disconnect))]
            internal class RPC_Disconnect
            {
                private static void Prefix(ZRpc rpc)
                {
                    ZNetPeer peer = ZNet.instance.GetPeer(rpc);

                    if (peer == null) { return; }

                    string playername = peer.m_playerName;

                    if (playername.Length > 0)
                    {
                        string postdata = TebConnectInfo.GetleaveMessage();
                        postdata = postdata.Replace("{player}", playername);

                        PostToDiscord(postdata);
                    }
                }
            }
        }
    }
}
