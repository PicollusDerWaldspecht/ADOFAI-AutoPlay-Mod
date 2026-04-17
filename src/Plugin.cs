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
        public const string PluginVersion = "0.3.0";

        internal static ConfigEntry<KeyCode> ToggleKeyConfig = null!;
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
                KeyCode.F8,
                "Key that toggles AutoPlay on/off. See https://docs.unity3d.com/ScriptReference/KeyCode.html for valid names.");

            EnabledAtStartConfig = Config.Bind(
                "General",
                "EnabledAtStart",
                false,
                "Startup behaviour when RememberState is false.\n" +
                "  false = AutoPlay starts OFF every time you launch the game.\n" +
                "  true  = AutoPlay starts ON every time you launch the game.\n" +
                "Ignored when RememberState is true.");

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
                "You normally don't need to edit this by hand.");

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
