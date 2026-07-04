using NUnit.Framework;

namespace Combat.VFX.Tests
{
    public sealed class DecalFadeTests
    {
        private const float StartAlpha = 0.85f;
        private const float Lifetime = 12f;
        private const float Tolerance = 0.0001f;

        [Test]
        public void EvaluateAlpha_AtStart_ReturnsStartAlpha()
        {
            float alpha = DecalFade.EvaluateAlpha(0f, Lifetime, StartAlpha);
            Assert.AreEqual(StartAlpha, alpha, Tolerance);
        }

        [Test]
        public void EvaluateAlpha_AtOrPastLifetime_ReturnsZero()
        {
            Assert.AreEqual(0f, DecalFade.EvaluateAlpha(Lifetime, Lifetime, StartAlpha), Tolerance);
            Assert.AreEqual(0f, DecalFade.EvaluateAlpha(Lifetime + 5f, Lifetime, StartAlpha), Tolerance);
        }

        [Test]
        public void EvaluateAlpha_DuringHoldPhase_StaysAtStartAlpha()
        {
            float alpha = DecalFade.EvaluateAlpha(Lifetime * 0.2f, Lifetime, StartAlpha);
            Assert.AreEqual(StartAlpha, alpha, Tolerance);
        }

        [Test]
        public void EvaluateAlpha_IsMonotonicNonIncreasing()
        {
            float previous = float.MaxValue;
            const int steps = 50;
            for (int i = 0; i <= steps; i++)
            {
                float elapsed = Lifetime * i / steps;
                float alpha = DecalFade.EvaluateAlpha(elapsed, Lifetime, StartAlpha);
                Assert.LessOrEqual(alpha, previous + Tolerance);
                previous = alpha;
            }
        }

        [Test]
        public void EvaluateAlpha_WithNonPositiveLifetime_ReturnsZero()
        {
            Assert.AreEqual(0f, DecalFade.EvaluateAlpha(1f, 0f, StartAlpha), Tolerance);
            Assert.AreEqual(0f, DecalFade.EvaluateAlpha(1f, -5f, StartAlpha), Tolerance);
        }
    }
}
