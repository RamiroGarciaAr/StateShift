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
        _hitMarkerImg.enabled = false;
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
        }
        if (_hideHitMarkerIn <= 0f)
        {
            _hitMarkerImg.enabled = false;
        }
    }

    private void HandleHit(DamageDealtEvent evt)
    {
        _hideHitMarkerIn = hitTimer;
        //then we set up our shader
        transform.localEulerAngles = new Vector3(0, 0, Random.Range(-maxAngle, maxAngle));

        _hitMarkerMaterial.color = GetColor(evt);

        _hitMarkerImg.enabled = true;
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
