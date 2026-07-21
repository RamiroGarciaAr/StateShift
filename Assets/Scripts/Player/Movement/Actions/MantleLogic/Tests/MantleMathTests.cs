using NUnit.Framework;
using UnityEngine;

namespace StateShift.Player.MantleLogic.Tests
{
    public sealed class MantleMathTests
    {
        private const float MinLedgeHeight = 0.8f;
        private const float MaxLedgeHeight = 1.6f;
        private const float Tolerance = 0.0001f;

        [Test]
        public void IsLedgeHeightValid_WithinWindow_ReturnsTrue()
        {
            Assert.IsTrue(MantleMath.IsLedgeHeightValid(1.2f, MinLedgeHeight, MaxLedgeHeight));
        }

        [Test]
        public void IsLedgeHeightValid_AtBoundaries_ReturnsTrue()
        {
            Assert.IsTrue(MantleMath.IsLedgeHeightValid(MinLedgeHeight, MinLedgeHeight, MaxLedgeHeight));
            Assert.IsTrue(MantleMath.IsLedgeHeightValid(MaxLedgeHeight, MinLedgeHeight, MaxLedgeHeight));
        }

        [Test]
        public void IsLedgeHeightValid_BelowMin_ReturnsFalse()
        {
            Assert.IsFalse(MantleMath.IsLedgeHeightValid(MinLedgeHeight - 0.01f, MinLedgeHeight, MaxLedgeHeight));
        }

        [Test]
        public void IsLedgeHeightValid_AboveMax_ReturnsFalse()
        {
            Assert.IsFalse(MantleMath.IsLedgeHeightValid(MaxLedgeHeight + 0.01f, MinLedgeHeight, MaxLedgeHeight));
        }

        [Test]
        public void ComputeLandingPosition_PushesForwardAlongFlattenedFacing()
        {
            Vector3 ledgeTop = new Vector3(0f, 2f, 0f);
            Vector3 facing = new Vector3(0f, 0.5f, 1f); // has vertical component that must be ignored
            const float forwardOffset = 0.5f;

            Vector3 landing = MantleMath.ComputeLandingPosition(ledgeTop, facing, forwardOffset);

            Assert.AreEqual(0f, landing.x, Tolerance);
            Assert.AreEqual(2f, landing.y, Tolerance);
            Assert.AreEqual(forwardOffset, landing.z, Tolerance);
        }

        [Test]
        public void ComputeLandingPosition_WithZeroHorizontalFacing_ReturnsLedgeTop()
        {
            Vector3 ledgeTop = new Vector3(1f, 3f, 2f);
            Vector3 facing = Vector3.up;

            Vector3 landing = MantleMath.ComputeLandingPosition(ledgeTop, facing, 0.5f);

            Assert.AreEqual(ledgeTop.x, landing.x, Tolerance);
            Assert.AreEqual(ledgeTop.y, landing.y, Tolerance);
            Assert.AreEqual(ledgeTop.z, landing.z, Tolerance);
        }

        [Test]
        public void SampleMantlePosition_AtStart_ReturnsStart()
        {
            Vector3 start = new Vector3(0f, 0f, 0f);
            Vector3 end = new Vector3(2f, 2f, 0f);
            AnimationCurve linear = AnimationCurve.Linear(0f, 0f, 1f, 1f);

            Vector3 pos = MantleMath.SampleMantlePosition(start, end, linear, linear, 0f);

            Assert.AreEqual(start.x, pos.x, Tolerance);
            Assert.AreEqual(start.y, pos.y, Tolerance);
            Assert.AreEqual(start.z, pos.z, Tolerance);
        }

        [Test]
        public void SampleMantlePosition_AtMidpoint_WithLinearCurves_IsHalfway()
        {
            Vector3 start = new Vector3(0f, 0f, 0f);
            Vector3 end = new Vector3(2f, 4f, 0f);
            AnimationCurve linear = AnimationCurve.Linear(0f, 0f, 1f, 1f);

            Vector3 pos = MantleMath.SampleMantlePosition(start, end, linear, linear, 0.5f);

            Assert.AreEqual(1f, pos.x, Tolerance);
            Assert.AreEqual(2f, pos.y, Tolerance);
            Assert.AreEqual(0f, pos.z, Tolerance);
        }

        [Test]
        public void SampleMantlePosition_AtEnd_ReturnsEnd()
        {
            Vector3 start = new Vector3(0f, 0f, 0f);
            Vector3 end = new Vector3(2f, 2f, 3f);
            AnimationCurve linear = AnimationCurve.Linear(0f, 0f, 1f, 1f);

            Vector3 pos = MantleMath.SampleMantlePosition(start, end, linear, linear, 1f);

            Assert.AreEqual(end.x, pos.x, Tolerance);
            Assert.AreEqual(end.y, pos.y, Tolerance);
            Assert.AreEqual(end.z, pos.z, Tolerance);
        }

        [Test]
        public void SampleMantlePosition_ClampsTimeBeyondOne()
        {
            Vector3 start = new Vector3(0f, 0f, 0f);
            Vector3 end = new Vector3(2f, 2f, 0f);
            AnimationCurve linear = AnimationCurve.Linear(0f, 0f, 1f, 1f);

            Vector3 pos = MantleMath.SampleMantlePosition(start, end, linear, linear, 1.5f);

            Assert.AreEqual(end.x, pos.x, Tolerance);
            Assert.AreEqual(end.y, pos.y, Tolerance);
        }

        [Test]
        public void SampleMantlePosition_WithNullCurves_FallsBackToLinear()
        {
            Vector3 start = new Vector3(0f, 0f, 0f);
            Vector3 end = new Vector3(4f, 2f, 0f);

            Vector3 pos = MantleMath.SampleMantlePosition(start, end, null, null, 0.5f);

            Assert.AreEqual(2f, pos.x, Tolerance);
            Assert.AreEqual(1f, pos.y, Tolerance);
        }
    }
}
