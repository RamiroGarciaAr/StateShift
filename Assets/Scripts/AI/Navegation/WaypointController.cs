using UnityEngine;

public class WaypointController : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField]
    private Camera _cam;

    [SerializeField]
    private AINavMove _testBot; // drag the Spiderbot here

    [SerializeField]
    private LayerMask _groundLayerMask; // serialized, not GetMask("Ground")

    [Header("Gizmos")]
    [SerializeField]
    private Color _gizmoColor = Color.cyan;

    [SerializeField]
    private float _gizmoRadius = 0.3f;

    [SerializeField]
    private float _gizmoHeight = 1.5f;

    private Vector3 _currentDestination;
    private bool _hasDestination;

    void Awake()
    {
        if (_cam == null)
            _cam = Camera.main;
        if (_cam == null)
        {
            Debug.LogError($"[WaypointController] No camera on {name}", this);
            enabled = false;
            return;
        }
    }

    void Update()
    {
        HandleDestinationInput();
        if (_testBot != null)
            _testBot.Tick(); // drive movement (temporary — brain does this later)
    }

    private void HandleDestinationInput()
    {
        if (!Input.GetMouseButtonDown(0))
            return;

        Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _groundLayerMask))
            return;

        _currentDestination = hit.point;
        _hasDestination = true;
        if (_testBot != null)
            _testBot.SetDestination(hit.point);
    }

    private void OnDrawGizmos()
    {
        if (!_hasDestination)
            return;

        Gizmos.color = _gizmoColor;
        Gizmos.DrawSphere(_currentDestination, _gizmoRadius);

        Vector3 top = _currentDestination + Vector3.up * _gizmoHeight;
        Gizmos.DrawLine(_currentDestination, top);
        Gizmos.DrawWireSphere(top, _gizmoRadius * 0.4f);
    }
}
