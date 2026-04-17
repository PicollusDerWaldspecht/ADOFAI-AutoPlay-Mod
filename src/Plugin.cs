using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace AdoFai_AutoPlay_Mod
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "adofai.autoplay";
        public const string PluginName = "ADOFAI AutoPlay";
        public const string PluginVersion = "0.4.0";

        internal static ConfigEntry<KeyboardShortcut> ToggleKeyConfig = null!;
        internal static ConfigEntry<bool> EnabledAtStartConfig = null!;
        internal static ConfigEntry<bool> RememberStateConfig = null!;
        internal static ConfigEntry<bool> PersistedEnabledConfig = null!;
        internal static ConfigEntry<bool> HideAutoplayTextConfig = null!;
        internal static ManualLogSource? StaticLogger;

        private Harmony? _harmony;

        private void Awake()
        {
            StaticLogger = Logger;

            BindConfig();

            AutoPlayState.Enabled = RememberStateConfig.Value
                ? PersistedEnabledConfig.Value
                : EnabledAtStartConfig.Value;

            ApplyHarmonyPatches();
            SpawnInputHost();

            Logger.LogInfo(
                $"{PluginName} v{PluginVersion} loaded. " +
                $"Toggle with [{ToggleKeyConfig.Value}]. Initial state: {(AutoPlayState.Enabled ? "ON" : "OFF")}.");
            Logger.LogDebug(
                $"Parsed ToggleKey → main={ToggleKeyConfig.Value.MainKey}, " +
                $"modifiers=[{string.Join("+", ToggleKeyConfig.Value.Modifiers)}]");
        }

        private void OnDestroy()
        {
            _harmony?.UnpatchSelf();
        }

        private void BindConfig()
        {
            ToggleKeyConfig = Config.Bind(
                "General",
                "ToggleKey",
                new KeyboardShortcut(KeyCode.F8),
                "Key (or key combination) that toggles AutoPlay on/off.\n" +
                "Single key example:     ToggleKey = Tab\n" +
                "Combination example:    ToggleKey = Tab + LeftControl\n" +
                "Valid key names are Unity KeyCodes: https://docs.unity3d.com/ScriptReference/KeyCode.html");

            EnabledAtStartConfig = Config.Bind(
                "General",
                "EnabledAtStart",
                false,
                "If true, AutoPlay is already active the moment the game starts. " +
                "Ignored when RememberState is true (the last saved state wins instead).");

            RememberStateConfig = Config.Bind(
                "General",
                "RememberState",
                false,
                "If true, AutoPlay resumes in whatever state it was in when you last quit the game.\n" +
                "The state is saved into the 'Enabled' setting below whenever you toggle AutoPlay.\n" +
                "If false, the startup state is controlled by EnabledAtStart instead.");

            PersistedEnabledConfig = Config.Bind(
                "General",
                "Enabled",
                false,
                "Storage slot for RememberState. The mod overwrites this automatically every time\n" +
                "you press the toggle key, so it reflects AutoPlay's state at your last quit.\n" +
                "Only read on startup when RememberState is true — otherwise this value is ignored.\n" +
                "You don't need to edit this by hand.");

            HideAutoplayTextConfig = Config.Bind(
                "UI",
                "HideAutoplayText",
                true,
                "If true, the built-in \"autoplay\" status text in the top-left is hidden " +
                "while AutoPlay is on. Set to false to let the game display its normal indicator.");
        }

        private void ApplyHarmonyPatches()
        {
            try
            {
                _harmony = new Harmony(PluginGuid);
                _harmony.PatchAll(typeof(ScrControllerUpdatePatch));
                _harmony.PatchAll(typeof(ScrControllerAwakePatch));
                _harmony.PatchAll(typeof(ScrControllerSimulatedPatch));
                _harmony.PatchAll(typeof(RdcAutoSetterPatch));
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to apply Harmony patches: {ex}");
            }
        }

        /// <summary>
        /// The BepInEx-hosted plugin component's <c>Update</c> is not reliably
        /// dispatched in ADOFAI, so we put all per-frame work onto a
        /// manually-created GameObject that we keep alive ourselves.
        /// </summary>
        private void SpawnInputHost()
        {
            try
            {
                var host = new GameObject("ADOFAI_AutoPlay_InputHost");
                DontDestroyOnLoad(host);
                host.hideFlags = HideFlags.HideAndDontSave;
                host.AddComponent<InputHost>();
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to spawn input host: {ex}");
            }
        }
    }
}
