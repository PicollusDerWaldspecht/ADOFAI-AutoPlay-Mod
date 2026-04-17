using HarmonyLib;

namespace AdoFai_AutoPlay_Mod
{
    // The per-frame mirror in InputHost.Update already keeps RDC.auto in sync
    // with our logical state, but these Harmony patches run inside the game's
    // own call chain. That guarantees the flag is correct even on frames where
    // the game reads RDC.auto *before* our MonoBehaviour tick, and during
    // level initialisation where scrController.Awake would otherwise reset it.

    /// <summary>
    /// Re-assert autoplay at the very top of <c>scrController.Update</c>,
    /// before the game reads <c>RDC.auto</c> later in the same frame.
    /// </summary>
    [HarmonyPatch(typeof(scrController), "Update")]
    internal static class ScrControllerUpdatePatch
    {
        private static void Prefix()
        {
            if (AutoPlayState.Enabled)
            {
                RDC.auto = true;
                RDC.useOldAuto = false;
            }
        }
    }

    /// <summary>
    /// <c>scrController.Awake</c> sets <c>RDC.auto = false</c> while
    /// initialising a level. Re-enable it in a postfix so the very first
    /// frame of the level already autoplays.
    /// </summary>
    [HarmonyPatch(typeof(scrController), "Awake")]
    internal static class ScrControllerAwakePatch
    {
        private static void Postfix()
        {
            if (AutoPlayState.Enabled)
            {
                RDC.auto = true;
                RDC.useOldAuto = false;
            }
        }
    }

    /// <summary>
    /// The actual autoplay input loop lives in
    /// <c>scrController.Simulated_PlayerControl_Update</c>. Forcing the flag
    /// right before it runs closes the remaining race where something else
    /// in the same frame could have flipped it.
    /// </summary>
    [HarmonyPatch(typeof(scrController), "Simulated_PlayerControl_Update")]
    internal static class ScrControllerSimulatedPatch
    {
        private static void Prefix()
        {
            if (AutoPlayState.Enabled)
            {
                RDC.auto = true;
                RDC.useOldAuto = false;
            }
        }
    }

    /// <summary>
    /// Intercepts any assignment to <c>RDC.auto</c> and rewrites a
    /// <c>false</c> to <c>true</c> while the mod is active, so nothing else
    /// in the game can turn autoplay off behind our back.
    /// </summary>
    [HarmonyPatch(typeof(RDC), nameof(RDC.auto), MethodType.Setter)]
    internal static class RdcAutoSetterPatch
    {
        private static void Prefix(ref bool value)
        {
            if (AutoPlayState.Enabled && !value)
            {
                value = true;
            }
        }
    }
}
