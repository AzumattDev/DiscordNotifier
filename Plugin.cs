using System;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Fusion;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;
using WardIsLove.Util.DiscordMessenger;

namespace DiscordNotifier
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class DiscordNotifierPlugin : BaseUnityPlugin
    {
        internal const string ModName = "DiscordNotifier";
        internal const string ModVersion = "1.0.0";
        internal const string Author = "Azumatt";
        private const string ModGUID = Author + "." + ModName;
        private static string ConfigFileName = ModGUID + ".cfg";
        private static string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;
        private readonly Harmony _harmony = new(ModGUID);
        public static readonly ManualLogSource DiscordNotifierLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);


        public enum Toggle
        {
            On = 1,
            Off = 0
        }

        public void Awake()
        {
            discordWebhook = Config.Bind("1 - General", "Discord Webhook", "", "Place the webhook link here. Must be running on the host machine for it to log information.");
            Assembly assembly = Assembly.GetExecutingAssembly();
            _harmony.PatchAll(assembly);
            SetupWatcher();
        }

        private void OnDestroy()
        {
            Config.Save();
        }

        private void SetupWatcher()
        {
            FileSystemWatcher watcher = new(Paths.ConfigPath, ConfigFileName);
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
                DiscordNotifierLogger.LogDebug("ReadConfigValues called");
                Config.Reload();
            }
            catch
            {
                DiscordNotifierLogger.LogError($"There was an issue loading your {ConfigFileName}");
                DiscordNotifierLogger.LogError("Please check your config entries for spelling and format!");
            }
        }


        #region ConfigOptions

        internal static ConfigEntry<string> discordWebhook = null!;

        private class ConfigurationManagerAttributes
        {
            [UsedImplicitly] public int? Order = null!;
            [UsedImplicitly] public bool? Browsable = null!;
            [UsedImplicitly] public string? Category = null!;
            [UsedImplicitly] public Action<ConfigEntryBase>? CustomDrawer = null!;
        }

        class AcceptableShortcuts : AcceptableValueBase
        {
            public AcceptableShortcuts() : base(typeof(KeyboardShortcut))
            {
            }

            public override object Clamp(object value) => value;
            public override bool IsValid(object value) => true;

            public override string ToDescriptionString() =>
                "# Acceptable values: " + string.Join(", ", UnityInput.Current.SupportedKeyCodes);
        }

        #endregion
    }

    [HarmonyPatch(typeof(PlayerDummy), nameof(PlayerDummy.NoticePlayerJoined_RPC))]
    static class PlayerDummyNoticePlayerJoined_RPCPatch
    {
        static void Postfix(PlayerDummy __instance, ref string characterName)
        {
            try
            {
                SessionProperty sessionProperty;
                if (!__instance.Runner.SessionInfo.Properties.TryGetValue("serverHostPlayerIdentifier", out sessionProperty))
                {
                    return;
                }

                Guid guid;
                if (!Guid.TryParse(sessionProperty, out guid))
                {
                    return;
                }

                bool flag = false;
                int playerCount = 1;
                for (int index = 0; index < WorldScene.code.allPlayerDummies.items.ToList<Transform>().Count; ++index)
                {
                    playerCount++;
                }

                Utils.LogToDiscord("Player Joined", characterName, playerCount);
            }
            catch (Exception e)
            {
                // Ignored for singleplayer
            }
        }
    }
}