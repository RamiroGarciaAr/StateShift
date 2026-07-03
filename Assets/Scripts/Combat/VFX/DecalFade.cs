using UnityEngine;

namespace Combat.VFX
{
    /// <summary>
    /// Pure alpha-falloff logic for scorch decals, isolated for testability.
    /// The alpha holds at full strength for an initial portion of the lifetime,
    /// then eases smoothly to zero by the end of the lifetime.
    /// </summary>
    public static class DecalFade
    {
        private const float HoldFraction = 0.4f;

        /// <summary>
        /// Evaluates the decal alpha at a given elapsed time.
        /// </summary>
        /// <param name="elapsedSeconds">Time elapsed since the decal started.</param>
        /// <param name="lifetimeSeconds">Total lifetime of the decal.</param>
        /// <param name="startAlpha">Alpha value held during the initial hold phase.</param>
        /// <returns>The alpha in the range [0, startAlpha], monotonically non-increasing over time.</returns>
        public static float EvaluateAlpha(float elapsedSeconds, float lifetimeSeconds, float startAlpha)
        {
            if (lifetimeSeconds <= 0f)
                return 0f;

            if (elapsedSeconds <= 0f)
                return startAlpha;

            if (elapsedSeconds >= lifetimeSeconds)
                return 0f;

            float holdSeconds = lifetimeSeconds * HoldFraction;
            if (elapsedSeconds <= holdSeconds)
                return startAlpha;

            float fadeT = Mathf.InverseLerp(holdSeconds, lifetimeSeconds, elapsedSeconds);
            return startAlpha * (1f - Mathf.SmoothStep(0f, 1f, fadeT));
        }
    }
}
