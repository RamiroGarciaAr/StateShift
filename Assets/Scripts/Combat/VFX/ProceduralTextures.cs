using UnityEngine;

namespace Combat.VFX
{
    /// <summary>
    /// Generates the runtime textures used by the impact VFX so no imported art is required.
    /// Textures are allocated once at initialisation and shared across effect instances.
    /// </summary>
    public static class ProceduralTextures
    {
        private const float HexagonEdgeSoftness = 0.04f;
        private const float ScorchCoreRadius = 0.15f;
        private const float ScorchNoiseStrength = 0.12f;

        /// <summary>
        /// Creates a crisp point-up hexagon with a hard edge, giving particles a flat low-poly
        /// (Synty-style) silhouette instead of a soft round glow. Used for sparks and the flash.
        /// </summary>
        /// <param name="size">Square texture resolution in pixels.</param>
        public static Texture2D CreateHexagon(int size)
        {
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };

            const float hexRadius = 0.9f;
            const float cos30 = 0.8660254f;
            float center = (size - 1) * 0.5f;
            float invHalf = 1f / center;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float px = (x - center) * invHalf;
                    float py = (y - center) * invHalf;
                    float qx = Mathf.Abs(px);
                    float qy = Mathf.Abs(py);

                    // Signed distance to a point-up regular hexagon.
                    float hexDistance = Mathf.Max(qx * cos30 + qy * 0.5f, qy);
                    float alpha = 1f - Mathf.SmoothStep(hexRadius - HexagonEdgeSoftness, hexRadius, hexDistance);

                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            texture.Apply();
            return texture;
        }

        /// <summary>
        /// Creates a dark radial gradient with slight noise for a burn/scorch mark.
        /// </summary>
        /// <param name="size">Square texture resolution in pixels.</param>
        public static Texture2D CreateScorch(int size)
        {
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };

            float center = (size - 1) * 0.5f;
            float maxDistance = center;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Mathf.Sqrt((x - center) * (x - center) + (y - center) * (y - center));
                    float normalized = Mathf.Clamp01(distance / maxDistance);

                    float alpha;
                    if (normalized <= ScorchCoreRadius)
                    {
                        alpha = 1f;
                    }
                    else
                    {
                        float edgeT = Mathf.InverseLerp(ScorchCoreRadius, 1f, normalized);
                        alpha = 1f - Mathf.SmoothStep(0f, 1f, edgeT);
                    }

                    float noise = (Mathf.PerlinNoise(x * 0.25f, y * 0.25f) - 0.5f) * ScorchNoiseStrength;
                    alpha = Mathf.Clamp01(alpha + noise);

                    texture.SetPixel(x, y, new Color(0.05f, 0.04f, 0.03f, alpha));
                }
            }

            texture.Apply();
            return texture;
        }
    }
}
