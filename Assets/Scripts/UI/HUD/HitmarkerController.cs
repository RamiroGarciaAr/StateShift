using Health;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class HitmarkerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private DamageDealtChannelSO channel;

    [SerializeField]
    private AudioPool audioPool;

    [Header("Hit Marker Behaviour")]
    [SerializeField, Range(0.01f, 0.5f)]
    private float hitTimer = 0.4f;

    [Tooltip("How much the Gap/Length grows")]
    [SerializeField]
    private float killSplayAmount = 0.2f;

    [SerializeField]
    private float maxAngle = 15f;

    [Header("Hit Marker Appearance")]
    [SerializeField]
    private Color neutralColor = Color.white;

    [SerializeField]
    private Color resistedColor = Color.blue;

    [SerializeField]
    private Color effectiveColor = Color.yellow;

    [SerializeField]
    private Color killColor = Color.red;

    [Header("Hit Marker Sounds")]
    [SerializeField]
    private Sound normalHitMarkerSound;

    [SerializeField]
    private Sound resistHitMarkerSound;

    [SerializeField]
    private Sound effectiveHitMarkerSound;

    [SerializeField]
    private Sound killHitMarkerSound;

    [SerializeField]
    private Sound weakSpotAccentSound;

    private RawImage _hitMarkerImg;
    private float _hideHitMarkerIn;
    private Material _hitMarkerMaterial;

    private float _initialGap;
    private float _initialLength;
    private bool _hasToPlayKillAnim = false;
    private static readonly int GapID = Shader.PropertyToID("_Gap");
    private static readonly int LengthID = Shader.PropertyToID("_Length");
    private static readonly int WeakSpotID = Shader.PropertyToID("_WeakSpot");

    void Awake()
    {
        _hitMarkerImg = GetComponent<RawImage>();
        if (_hitMarkerImg == null)
        {
            Debug.LogError(
                "[HitmarkerController] No hitmarker Image found! Check that the image is set up correctly"
            );
        }
        else
        {
            _hitMarkerMaterial = new Material(_hitMarkerImg.material);
            _hitMarkerImg.material = _hitMarkerMaterial; // we allocate it once
        }
        if (channel != null)
            channel.OnRaised += HandleHit;
        if (audioPool == null)
            Debug.LogError("[HitmarkerController] No AudioPool");
    }

    void Start()
    {
        //set up hitmarker
        _hitMarkerImg.enabled = false;
        _initialGap = _hitMarkerMaterial.GetFloat(GapID);
        _initialLength = _hitMarkerMaterial.GetFloat(LengthID);
    }

    private void OnDestroy()
    {
        if (channel != null)
            channel.OnRaised -= HandleHit;
    }

    // Update is called once per frame
    void Update()
    {
        if (_hideHitMarkerIn > 0f)
        {
            _hideHitMarkerIn -= Time.deltaTime;
            if (_hasToPlayKillAnim)
                PlayKillAnim();
        }
        if (_hideHitMarkerIn <= 0f)
        {
            _hitMarkerImg.enabled = false;
            _hasToPlayKillAnim = false;
        }
    }

    private void HandleHit(DamageDealtEvent evt)
    {
        //reset our values just in case
        _hitMarkerMaterial.SetFloat(GapID, _initialGap);
        _hitMarkerMaterial.SetFloat(LengthID, _initialLength);

        float ws = (evt.bodyPart == BodyPart.WeakSpot) ? 1f : 0f;
        _hitMarkerMaterial.SetFloat(WeakSpotID, ws);

        _hideHitMarkerIn = hitTimer;
        //then we set up our shader
        _hitMarkerMaterial.color = GetColor(evt);
        if (evt.isKillShot)
        {
            transform.localEulerAngles = new Vector3(0, 0, 0);
            _hasToPlayKillAnim = true;
        }
        else
        {
            transform.localEulerAngles = new Vector3(0, 0, Random.Range(-maxAngle, maxAngle));
        }
        Sound _hitSound = GetHitmarkerSound(evt);
        audioPool.PlayAt(_hitSound, is3D: false);
        if (evt.bodyPart == BodyPart.WeakSpot)
            audioPool.PlayAt(weakSpotAccentSound, is3D: false);

        _hitMarkerImg.enabled = true;
    }

    private void PlayKillAnim()
    {
        float t = 1f - (_hideHitMarkerIn / hitTimer);
        float current_length = Mathf.Lerp(_initialLength, _initialLength + killSplayAmount, t);
        _hitMarkerMaterial.SetFloat(LengthID, current_length);

        float current_gap = Mathf.Lerp(_initialGap, _initialGap + killSplayAmount, t);
        _hitMarkerMaterial.SetFloat(GapID, current_gap);
    }

    private Color GetColor(DamageDealtEvent evt)
    {
        if (evt.isKillShot)
            return killColor;
        else if (evt.dmgEffectiveness > 1f)
            return effectiveColor;
        else if (evt.dmgEffectiveness < 1f)
            return resistedColor;

        return neutralColor;
    }

    private Sound GetHitmarkerSound(DamageDealtEvent evt)
    {
        if (evt.isKillShot)
            return killHitMarkerSound;
        else if (evt.dmgEffectiveness > 1f)
            return effectiveHitMarkerSound;
        else if (evt.dmgEffectiveness < 1f)
            return resistHitMarkerSound;

        return normalHitMarkerSound;
    }
}
