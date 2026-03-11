using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WaypointController : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private Camera _cam;
    [SerializeField] private NavMeshAgent agent;


    [Header("Gizmos")]
    [SerializeField] private Color gizmoColor = Color.cyan;
    [SerializeField] private float gizmoRadius = 0.3f;
    [SerializeField] private float gizmoHeight = 1.5f;
    private Vector3 _lastDestination;
    private bool _hasDestination=false;
    private int _groundLayerMask;

    void Awake()
    {

        if (_cam == null)
        {
            _cam = Camera.main;
        }
        _groundLayerMask = LayerMask.GetMask("Ground");

        Debug.Assert(_cam != null, $"[WaypointController] Camera reference missing on {gameObject.name}");
        Debug.Assert(agent != null, $"[WaypointController] NavMeshAgent reference missing on {gameObject.name}");
    }

    void Update()
    {
        HandleDestinationInput();
    }

    private void SetDestination(Vector3 destination)
    {
        agent.SetDestination(destination);
        _lastDestination = destination;
        _hasDestination = true;
    }

    private void HandleDestinationInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
            
            if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _groundLayerMask)) return;

            SetDestination(hit.point);
        }
    }

    private void OnDrawGizmos()
    {
        if (!_hasDestination) return;

        Gizmos.color = gizmoColor;

        Gizmos.DrawSphere(_lastDestination,gizmoRadius);

        Vector3 top = _lastDestination + Vector3.up * gizmoHeight;

        Gizmos.DrawLine(_lastDestination,top);
        Gizmos.DrawWireSphere(top,gizmoRadius*0.4f);
    }
}
