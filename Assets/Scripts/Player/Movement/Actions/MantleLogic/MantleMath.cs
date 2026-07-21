using UnityEngine;

namespace StateShift.Player.MantleLogic
{
    /// <summary>
    /// Pure, side-effect-free math helpers for the mantle system. Kept free of any
    /// scene/component state so the ledge gating, landing-point, and trajectory
    /// sampling logic can be unit-tested in Edit Mode.
    /// </summary>
    public static class MantleMath
    {
        /// <summary>
        /// Returns true when the measured ledge height falls inside the inclusive
        /// waist-to-chest window that the player is allowed to mantle.
        /// </summary>
        /// <param name="ledgeHeight">Vertical distance from the player's feet to the ledge top.</param>
        /// <param name="minLedgeHeight">Lowest mantleable height (inclusive).</param>
        /// <param name="maxLedgeHeight">Highest mantleable height (inclusive).</param>
        public static bool IsLedgeHeightValid(float ledgeHeight, float minLedgeHeight, float maxLedgeHeight)
        {
            return ledgeHeight >= minLedgeHeight && ledgeHeight <= maxLedgeHeight;
        }

        /// <summary>
        /// Computes the world position the player should be planted at once the mantle
        /// completes: the ledge top pushed forward along the flattened facing direction.
        /// </summary>
        /// <param name="ledgeTop">World point on the top surface of the ledge (at the lip).</param>
        /// <param name="facing">Player facing direction; only the horizontal component is used.</param>
        /// <param name="forwardOffset">How far past the lip to plant the player.</param>
        public static Vector3 ComputeLandingPosition(Vector3 ledgeTop, Vector3 facing, float forwardOffset)
        {
            Vector3 flatFacing = new Vector3(facing.x, 0f, facing.z);
            if (flatFacing.sqrMagnitude > 0.0001f)
            {
                flatFacing.Normalize();
            }

            return ledgeTop + flatFacing * forwardOffset;
        }

        /// <summary>
        /// Samples the scripted mantle trajectory at normalized time <paramref name="normalizedTime"/>.
        /// Horizontal (XZ) progress is driven by <paramref name="horizontalCurve"/> and vertical (Y)
        /// progress by <paramref name="verticalCurve"/>, producing the characteristic "up then over" arc.
        /// </summary>
        /// <param name="start">Trajectory start position.</param>
        /// <param name="end">Trajectory end (landing) position.</param>
        /// <param name="horizontalCurve">Curve mapping [0,1] progress for the XZ plane.</param>
        /// <param name="verticalCurve">Curve mapping [0,1] progress for the Y axis.</param>
        /// <param name="normalizedTime">Progress through the mantle in [0,1].</param>
        public static Vector3 SampleMantlePosition(
            Vector3 start,
            Vector3 end,
            AnimationCurve horizontalCurve,
            AnimationCurve verticalCurve,
            float normalizedTime)
        {
            float t = Mathf.Clamp01(normalizedTime);
            float horizontalProgress = horizontalCurve != null ? horizontalCurve.Evaluate(t) : t;
            float verticalProgress = verticalCurve != null ? verticalCurve.Evaluate(t) : t;

            float x = Mathf.Lerp(start.x, end.x, horizontalProgress);
            float z = Mathf.Lerp(start.z, end.z, horizontalProgress);
            float y = Mathf.Lerp(start.y, end.y, verticalProgress);

            return new Vector3(x, y, z);
        }
    }
}
