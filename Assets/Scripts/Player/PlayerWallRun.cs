using UnityEngine;
[RequireComponent(typeof(Rigidbody))]
public class PlayerWallRun : MonoBehaviour
{
    [Header("WallRunning")]
    [SerializeField] private LayerMask wallLayerMask;
    [SerializeField] private LayerMask groundLayerMask;
    [SerializeField] private float wallRunForce;
    [SerializeField] private float maxWallRunTime;

    [Header("Detection")]
    [SerializeField] private float wallCheckDistance;
    [SerializeField] private float minJumpHeight;


    public bool HasWall => _isWallRight || _isWallLeft;

    private float _wallRunTimer;
    private RaycastHit _leftWallHit;
    private RaycastHit _rightWallHit;
    private bool _isWallLeft;
    private bool _isWallRight;
    private bool _rb;
    private Transform _dir;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _dir = Camera.main.transform;
    }
    private void Update()
    {
        CheckForWall();
    }

    private void CheckForWall()
    {
        _isWallRight = Physics.Raycast(transform.position, _dir.right, out _rightWallHit, wallCheckDistance, wallLayerMask);
        _isWallLeft = Physics.Raycast(transform.position, -_dir.right, out _leftWallHit, wallCheckDistance, wallLayerMask);
    }

}
