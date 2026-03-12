using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;

[DisallowMultipleComponent]
public class CrosshairBloomController : MonoBehaviour
{
    [Header("Material y propiedad del shader")]
    [SerializeField] private Material crosshairMaterial;
    [SerializeField] private string innerRadiusProp = "_InnerRadius";

    [Header("Valores base")]
    [SerializeField, Min(0f)] private float baseInnerRadius = 0.20f;

    [Header("Bloom")]
    [Tooltip("Cuánto aumenta el radio por cada click adicional mientras anima.")]
    [SerializeField, Min(0f)] private float perClickAdd = 0.06f;
    [Tooltip("Límite superior del bloom.")]
    [SerializeField, Min(0f)] private float maxBloomRadius = 0.45f;

    [Header("Tiempos")]
    [SerializeField, Min(0.01f)] private float growDuration = 0.06f;   // subida rápida
    [SerializeField, Min(0.01f)] private float decayDuration = 0.25f;  // bajada + lenta
    [Tooltip("Pausa antes de empezar a decaer (opcional).")]
    [SerializeField, Min(0f)] private float decayDelay = 0.0f;

    [Header("Curvas")]
    [SerializeField] private AnimationCurve growCurve = AnimationCurve.EaseInOut(0,0,1,1);
    [SerializeField] private AnimationCurve decayCurve = AnimationCurve.EaseInOut(0,0,1,1);

    [Header("Input System (Nuevo)")]
    [SerializeField] private InputActionReference fireAction;

    [Header("Extras")]
    [Tooltip("Tiempo mínimo entre disparos de bloom (seg). 0 = sin límite.")]
    [SerializeField, Min(0f)] private float minInterval = 0.03f;

    float lastBloomTime = -999f;
    Coroutine animCo;
    int innerRadiusID;

    void Awake()
    {
        innerRadiusID = Shader.PropertyToID(innerRadiusProp);
        if (!crosshairMaterial)
            Debug.LogWarning("[CrosshairBloomController] Asigná el material con _InnerRadius.");
        SetInner(baseInnerRadius);
    }

    void OnEnable()
    {
        if (fireAction != null && fireAction.action != null)
        {
            fireAction.action.performed += OnFire;
            fireAction.action.Enable();
        }
    }

    void OnDisable()
    {
        if (fireAction != null && fireAction.action != null)
        {
            fireAction.action.performed -= OnFire;
            fireAction.action.Disable();
        }
    }

    void OnFire(InputAction.CallbackContext ctx)
    {
        if (!crosshairMaterial) return;
        if (minInterval > 0f && (Time.time - lastBloomTime) < minInterval) return;
        lastBloomTime = Time.time;

        //Acumular bloom.
        float current = GetInner();
        float desired = Mathf.Min(current + perClickAdd, maxBloomRadius);

        if (animCo != null) StopCoroutine(animCo);
        animCo = StartCoroutine(BloomRoutine(desired));
    }

    IEnumerator BloomRoutine(float target)
    {
        // GROW desde el valor actual al nuevo target acumulado.
        float start = GetInner();
        float end = Mathf.Max(target, start); // por la puta
        float t = 0f;

        while (t < growDuration)
        {
            t += Time.unscaledDeltaTime;
            float pct = Mathf.Clamp01(t / growDuration);
            float k = growCurve.Evaluate(pct);
            SetInner(Mathf.Lerp(start, end, k));
            yield return null;
        }
        SetInner(end);

        // Hold antes de arrancar
        if (decayDelay > 0f)
        {
            float hold = 0f;
            while (hold < decayDelay)
            {
                hold += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        // DECAY hacia base
        float dStart = GetInner();
        float dEnd = baseInnerRadius;
        t = 0f;

        while (t < decayDuration)
        {
            t += Time.unscaledDeltaTime;
            float pct = Mathf.Clamp01(t / decayDuration);
            float k = decayCurve.Evaluate(pct);
            SetInner(Mathf.Lerp(dStart, dEnd, k));
            yield return null;
        }

        SetInner(dEnd);
        animCo = null;
    }

    void SetInner(float v)
    {
        if (crosshairMaterial) crosshairMaterial.SetFloat(innerRadiusID, v);
    }

    float GetInner()
    {
        if (!crosshairMaterial) return baseInnerRadius;
        return crosshairMaterial.GetFloat(innerRadiusID);
    }

    // Trigger manual
    public void TriggerBloomOnce() => OnFire(default);
}
