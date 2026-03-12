using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlatformMover))]
public class PlatformLoopController : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;
    public float speed = 2f;
    public float waitAtPoint = 0.5f;

    PlatformMover mover;
    Vector3 currentTargetPos;
    bool targetIsB = true;

    void Awake()
    {
        mover = GetComponent<PlatformMover>();
    }

    void Start()
    {
        if (pointA == null || pointB == null)
        {
            Debug.LogError("PlatformLoopController: asigná pointA y pointB en el inspector.");
            enabled = false;
            return;
        }

        // posicion inicial en pointA (opcional)
        transform.position = pointA.position;

        StartCoroutine(LoopRoutine());
    }

    private IEnumerator LoopRoutine()
    {
        while (true)
        {
            currentTargetPos = targetIsB ? pointB.position : pointA.position;
            Vector3 moveDir = currentTargetPos - transform.position;


            MoveCommand cmd = new MoveCommand(mover)
            {
                moveDir = moveDir,   
                maxSpeed = speed
            };

            bool arrived = false;
            void OnArrived() => arrived = true;

            mover.OnArrived += OnArrived;

            CommandInvoker.ExecuteCommand(cmd);
            while (!arrived)
                yield return null;

            mover.OnArrived -= OnArrived;

            yield return new WaitForSeconds(waitAtPoint);

            targetIsB = !targetIsB;
        }
    }
}
