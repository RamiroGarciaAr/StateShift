using UnityEngine;

public class Spinner : MonoBehaviour
{
    [SerializeField, Tooltip("Degrees per second")]
    private float _speed = 90f;

    [SerializeField]
    private Vector3 _axis = Vector3.up;

    private void Update()
    {
        transform.Rotate(_axis, _speed * Time.deltaTime, Space.Self);
    }
}
