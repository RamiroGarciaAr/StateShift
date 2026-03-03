using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WaypointController : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private Camera cam;
    [SerializeField] private NavMeshAgent agent;


    [Header("Gizmos")]
    [SerializeField] private Color gizmoColor = Color.cyan;
    [SerializeField] private float gizmoRadius = 0.3f;
    [SerializeField] private float gizmoHeight = 1.5f;
    private Vector3 _lastDestination;
    private bool _hasDestination=false;
    

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground")) //EW
                {
                     //agent.SetDestination(hit.point);
                    _lastDestination = hit.point;
                    _hasDestination=true;
                }

            }
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
