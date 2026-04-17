using UnityEngine;
using UnityEngine.UI;

namespace AdoFai_AutoPlay_Mod
{
    /// <summary>
    /// Dedicated MonoBehaviour that sits on a <see cref="GameObject"/> we
    /// create and keep alive ourselves (via <see cref="Object.DontDestroyOnLoad"/>).
    /// The BepInEx plugin host component doesn't reliably receive Unity's
    /// <c>Update</c> callback inside ADOFAI, whereas a plain manually-spawned
    /// GameObject does — so we do all per-frame work from here.
    /// <para>
    /// Responsibilities:
    ///   <list type="bullet">
    ///     <item>Poll the configured toggle hotkey.</item>
    ///     <item>Mirror <see cref="AutoPlayState.Enabled"/> into
    ///       <c>RDC.auto</c> every frame (both directions, so toggling the
    ///       mod off also turns autoplay off immediately).</item>
    ///     <item>In <c>LateUpdate</c>, hide the game's built-in "autoplay"
    ///       status text by disabling the <see cref="Text"/> component on
    ///       the <see cref="scrShowIfDebug"/> HUD element. Running in
    ///       <c>LateUpdate</c> ensures we overwrite the game's own
    ///       <c>Update</c>, which sets the text to visible every frame.</item>
    ///   </list>
    /// </para>
    /// </summary>
    internal class InputHost : MonoBehaviour
    {
        private scrShowIfDebug? _cachedDebugHud;
        private Text? _cachedDebugHudText;

        private void Update()
        {
            if (AutoPlayState.Enabled)
            {
                RDC.auto = true;
                RDC.useOldAuto = false;
            }
            else if (RDC.auto)
            {
                RDC.auto = false;
            }

            if (Input.GetKeyDown(Plugin.ToggleKeyConfig.Value))
            {
                AutoPlayState.Enabled = !AutoPlayState.Enabled;

                if (Plugin.RememberStateConfig.Value)
                {
                    Plugin.PersistedEnabledConfig.Value = AutoPlayState.Enabled;
                }

                RDC.auto = AutoPlayState.Enabled;
                if (AutoPlayState.Enabled)
                {
                    RDC.useOldAuto = false;
                }

                Plugin.StaticLogger?.LogInfo($"AutoPlay {(AutoPlayState.Enabled ? "ENABLED" : "DISABLED")}");
            }
        }

        private void LateUpdate()
        {
            if (!AutoPlayState.Enabled || !Plugin.HideAutoplayTextConfig.Value)
            {
                return;
            }

            if (_cachedDebugHud == null)
            {
                _cachedDebugHud = FindObjectOfType<scrShowIfDebug>();
                if (_cachedDebugHud == null)
                {
                    return;
                }

                _cachedDebugHudText = _cachedDebugHud.GetComponent<Text>();
            }

            if (_cachedDebugHudText == null)
            {
                _cachedDebugHudText = _cachedDebugHud.GetComponent<Text>();
                if (_cachedDebugHudText == null)
                {
                    return;
                }
            }

            _cachedDebugHudText.enabled = false;
            _cachedDebugHudText.text = string.Empty;
        }
    }
}
