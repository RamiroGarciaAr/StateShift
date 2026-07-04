using NUnit.Framework;
using UnityEngine;

namespace Utilities.Tests
{
    /// <summary>
    /// Edit Mode tests for the pure <see cref="CameraTraumaShakeState"/> trauma + noise model.
    /// </summary>
    public class CameraTraumaShakeStateTests
    {
        private const float Tolerance = 1e-4f;

        [Test]
        public void AddTrauma_ClampsToOne_OnRepeatedAdds()
        {
            CameraTraumaShakeState state = new CameraTraumaShakeState();

            for (int i = 0; i < 10; i++)
            {
                state.AddTrauma(0.25f);
            }

            Assert.AreEqual(1f, state.Trauma, Tolerance);
        }

        [Test]
        public void Tick_DecaysTraumaTowardZero_AndNeverBelowZero()
        {
            CameraTraumaShakeState state = new CameraTraumaShakeState();
            state.AddTrauma(1f);

            state.Tick(1.8f, 28f, 0.1f);
            Assert.AreEqual(1f - 0.18f, state.Trauma, Tolerance);

            // Over-decay: many ticks must floor at exactly 0, never negative.
            for (int i = 0; i < 100; i++)
            {
                state.Tick(1.8f, 28f, 0.1f);
            }

            Assert.AreEqual(0f, state.Trauma, Tolerance);
        }

        [Test]
        public void Shake_EqualsTraumaRaisedToExponent()
        {
            CameraTraumaShakeState state = new CameraTraumaShakeState();
            state.AddTrauma(0.5f);

            Assert.AreEqual(Mathf.Pow(0.5f, 2f), state.Shake(2f), Tolerance);
            Assert.AreEqual(Mathf.Pow(0.5f, 3f), state.Shake(3f), Tolerance);
        }

        [Test]
        public void GetRotationOffset_IsZero_WhenTraumaIsZero()
        {
            CameraTraumaShakeState state = new CameraTraumaShakeState();

            Vector3 offset = state.GetRotationOffset(new Vector3(2f, 2f, 3f), 2f);

            Assert.AreEqual(Vector3.zero, offset);
        }

        [Test]
        public void GetRotationOffset_StaysWithinMaxAngleBounds()
        {
            CameraTraumaShakeState state = new CameraTraumaShakeState();
            state.AddTrauma(1f);
            Vector3 maxAngles = new Vector3(2f, 2f, 3f);

            // Sample across many noise-time steps to exercise the noise range.
            for (int i = 0; i < 500; i++)
            {
                state.Tick(0f, 28f, 0.016f);
                Vector3 offset = state.GetRotationOffset(maxAngles, 2f);

                Assert.LessOrEqual(Mathf.Abs(offset.x), maxAngles.x + Tolerance);
                Assert.LessOrEqual(Mathf.Abs(offset.y), maxAngles.y + Tolerance);
                Assert.LessOrEqual(Mathf.Abs(offset.z), maxAngles.z + Tolerance);
            }
        }

        [Test]
        public void Tick_ResetsNoiseTimeToZero_WhenTraumaReachesRest()
        {
            CameraTraumaShakeState state = new CameraTraumaShakeState();
            state.AddTrauma(0.1f);

            // Advance while active so noise time grows.
            state.Tick(0f, 28f, 0.1f);
            Assert.Greater(state.NoiseTime, 0f);

            // Decay fully; noise time must reset to exactly 0 at rest.
            state.Tick(10f, 28f, 1f);
            Assert.AreEqual(0f, state.Trauma, Tolerance);
            Assert.AreEqual(0f, state.NoiseTime, Tolerance);
        }

        [Test]
        public void IsAtRest_TrueAfterFullDecay_FalseImmediatelyAfterAddTrauma()
        {
            CameraTraumaShakeState state = new CameraTraumaShakeState();
            Assert.IsTrue(state.IsAtRest);

            state.AddTrauma(0.25f);
            Assert.IsFalse(state.IsAtRest);

            state.Tick(10f, 28f, 1f);
            Assert.IsTrue(state.IsAtRest);
        }
    }
}
