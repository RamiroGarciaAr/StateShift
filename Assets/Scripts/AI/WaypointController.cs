using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WaypointController : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private Camera _cam;

    [Header("Gizmos")]
    [SerializeField] private Color gizmoColor = Color.cyan;
    [SerializeField] private float gizmoRadius = 0.3f;
    [SerializeField] private float gizmoHeight = 1.5f;

    public static Vector3 CurrentDestination {get; private set;}
    public static bool HasDestination {get; private set;}
    private LayerMask _groundLayerMask;

    void Awake()
    {
        if (_cam == null)
        {
            _cam = Camera.main;
        }
        _groundLayerMask = LayerMask.GetMask("Ground");

        Debug.Assert(_cam != null, $"[WaypointController] Camera reference missing on {gameObject.name}");
    }

    void Update()
    {
        HandleDestinationInput();
    }

    private void SetDestination(Vector3 destination)
    {
        CurrentDestination = destination;
        HasDestination = true;
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
        if (!HasDestination) return;

        Gizmos.color = gizmoColor;

        Gizmos.DrawSphere(CurrentDestination,gizmoRadius);

        Vector3 top = CurrentDestination + Vector3.up * gizmoHeight;

        Gizmos.DrawLine(CurrentDestination,top);
        Gizmos.DrawWireSphere(top,gizmoRadius*0.4f);
    }
}
