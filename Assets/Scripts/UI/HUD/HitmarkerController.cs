using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class HitmarkerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private DamageDealtChannelSO channel;

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
    private RawImage _hitMarkerImg;
    private float _hideHitMarkerIn;
    private Material _hitMarkerMaterial;

    private float _initialGap;
    private float _initialLength;
    private bool _hasToPlayKillAnim = false;
    private const string prop_gap = "_Gap";
    private const string prop_length = "_Length";

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
    }

    void Start()
    {
        //set up hitmarker
        _hitMarkerImg.enabled = false;
        _initialGap = _hitMarkerMaterial.GetFloat(prop_gap);
        _initialLength = _hitMarkerMaterial.GetFloat(prop_length);
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
        _hitMarkerMaterial.SetFloat(prop_gap, _initialGap);
        _hitMarkerMaterial.SetFloat(prop_length, _initialLength);

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

        _hitMarkerImg.enabled = true;
    }

    private void PlayKillAnim()
    {
        float t = 1f - (_hideHitMarkerIn / hitTimer);
        float current_length = Mathf.Lerp(_initialLength, _initialLength + killSplayAmount, t);
        _hitMarkerMaterial.SetFloat(prop_length, current_length);

        float current_gap = Mathf.Lerp(_initialGap, _initialGap + killSplayAmount, t);
        _hitMarkerMaterial.SetFloat(prop_gap, current_gap);
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
}
