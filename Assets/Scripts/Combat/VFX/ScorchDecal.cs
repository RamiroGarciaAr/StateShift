using System;
using UnityEngine;

namespace Combat.VFX
{
    /// <summary>
    /// Fades a scorch decal quad's tint alpha over its lifetime using a cached
    /// <see cref="MaterialPropertyBlock"/> so no material instances are allocated at runtime.
    /// Raises <see cref="Finished"/> when the lifetime ends or when force-released.
    /// </summary>
    public sealed class ScorchDecal : MonoBehaviour
    {
        private static readonly int ColorPropertyId = Shader.PropertyToID("_Color");

        [Header("Fade")]
        [Tooltip("Renderer whose material tint alpha is faded over the decal lifetime.")]
        [SerializeField] private MeshRenderer _meshRenderer;

        [Tooltip("Alpha the decal holds at before fading to zero.")]
        [SerializeField] private float _startAlpha = 0.85f;

        private MaterialPropertyBlock _propertyBlock;
        private Color _baseColor = Color.black;
        private float _elapsed;
        private float _lifetime;
        private bool _isPlaying;

        /// <summary>
        /// Raised when the decal has finished fading or has been force-released.
        /// </summary>
        public event Action<ScorchDecal> Finished;

        private void Awake()
        {
            _propertyBlock = new MaterialPropertyBlock();
            if (_meshRenderer != null && _meshRenderer.sharedMaterial != null &&
                _meshRenderer.sharedMaterial.HasProperty(ColorPropertyId))
            {
                _baseColor = _meshRenderer.sharedMaterial.GetColor(ColorPropertyId);
            }
        }

        /// <summary>
        /// Starts the decal's timed fade with the given lifetime.
        /// </summary>
        /// <param name="lifetimeSeconds">Total seconds before the decal is released.</param>
        public void Play(float lifetimeSeconds)
        {
            gameObject.SetActive(true);
            _elapsed = 0f;
            _lifetime = lifetimeSeconds;
            _isPlaying = true;
            ApplyAlpha(_startAlpha);
        }

        /// <summary>
        /// Immediately ends the decal, used by the spawner's oldest-decal recycling.
        /// </summary>
        public void ForceRelease()
        {
            if (!_isPlaying)
                return;

            _isPlaying = false;
            Finished?.Invoke(this);
            gameObject.SetActive(false);
        }

        private void Update()
        {
            if (!_isPlaying)
                return;

            _elapsed += Time.deltaTime;
            float alpha = DecalFade.EvaluateAlpha(_elapsed, _lifetime, _startAlpha);
            ApplyAlpha(alpha);

            if (_elapsed >= _lifetime)
            {
                _isPlaying = false;
                Finished?.Invoke(this);
                gameObject.SetActive(false);
            }
        }

        private void ApplyAlpha(float alpha)
        {
            if (_meshRenderer == null)
                return;

            _meshRenderer.GetPropertyBlock(_propertyBlock);
            _baseColor.a = alpha;
            _propertyBlock.SetColor(ColorPropertyId, _baseColor);
            _meshRenderer.SetPropertyBlock(_propertyBlock);
        }
    }
}
