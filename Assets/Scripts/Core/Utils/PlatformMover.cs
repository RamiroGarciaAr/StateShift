using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlatformMover : MonoBehaviour, IMovable
{
    public bool IsMoving { get; private set; } = false;
    public event Action OnArrived; 

    private Coroutine currentCoroutine;


    public void Move(Vector3 dir, float maxSpeed)
    {
        Vector3 target = transform.position + dir;

        if (currentCoroutine != null)
            StopCoroutine(currentCoroutine);

        currentCoroutine = StartCoroutine(MoveToTargetCoroutine(target, maxSpeed));
    }

    private IEnumerator MoveToTargetCoroutine(Vector3 target, float speed)
    {
        IsMoving = true;

        // tolerancia para considerar "llegado"
        float threshold = 0.01f;

        // mover usando MovePosition para compatibilidad con Rigidbody
        Rigidbody rb = GetComponent<Rigidbody>();

        // Si tenés cuerpo kinemático, usá rb.MovePosition; si no, transform.position.
        bool hasRb = rb != null && !rb.isKinematic;

        while ((transform.position - target).sqrMagnitude > threshold * threshold)
        {
            Vector3 newPos = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

            if (hasRb)
                rb.MovePosition(newPos);
            else
                transform.position = newPos;

            yield return null;
        }

        // Asegurar precisión final
        if (hasRb)
            rb.MovePosition(target);
        else
            transform.position = target;

        IsMoving = false;
        OnArrived?.Invoke();
        currentCoroutine = null;
    }

    // opcional: detener movimiento
    public void StopMoving()
    {
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
            currentCoroutine = null;
        }
        IsMoving = false;
    }
}
