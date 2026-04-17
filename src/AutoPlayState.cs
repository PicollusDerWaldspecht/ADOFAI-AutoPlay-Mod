namespace AdoFai_AutoPlay_Mod
{
    /// <summary>
    /// Shared, process-wide autoplay toggle. Written by <see cref="InputHost"/>
    /// when the user presses the hotkey, and read every frame by both
    /// <see cref="InputHost"/> and the Harmony patches that enforce
    /// <c>RDC.auto</c>.
    /// </summary>
    internal static class AutoPlayState
    {
        public static bool Enabled;
    }
}
