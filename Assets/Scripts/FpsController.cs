using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FpsController : MonoBehaviour
{
    public Rigidbody body;
    public GameObject cameraHandle;
    
    [Header("Movement Settings")]
    public float moveSpeed = 6f;
    [Range(0, 1)]
    public float airControl = 0f;
    [Range(0, 1)]
    public float speedChangeRate = 0f;

    public float jumpForce = 380f;
    public float jumpBufferTime = 0.2f;
    public float groundedCheckRange = 1.1f;
    public LayerMask groundLayerMask;

    [Header("Camera Settings")]
    public float horizontalSensitivity = 1f;
    public float verticalSensitivity = 1f;
    public float minimumPitch = -89.9f;
    public float maximumPitch = 89.9f;

    private float _bufferedJumpTimer;
    private bool isJumpQueued => _bufferedJumpTimer > 0f;
    private bool _isGrounded = false;
    private Vector3 _moveInput;

    private float _yaw = 0f;
    private float _pitch = 0f;

    void Reset()
    {
        body = GetComponentInChildren<Rigidbody>();
#if false
        // Default settings for rigidbody
        body.mass = 75f;
        body.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
#endif
    }

    void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        _moveInput.x = Input.GetAxis("Horizontal");
        _moveInput.z = Input.GetAxis("Vertical");

        if (Input.GetButtonDown("Jump"))
        {
            _bufferedJumpTimer = jumpBufferTime;
        }
        else if (isJumpQueued)
        {
            _bufferedJumpTimer -= Time.deltaTime;
        }

        if (Cursor.lockState == CursorLockMode.Locked)
        {
            var deltaYaw = Input.GetAxisRaw("Mouse X");
            var deltaPitch = Input.GetAxisRaw("Mouse Y");

            _yaw += deltaYaw * horizontalSensitivity;
            // Constraint: -180 < _yaw <= 180
            _yaw = (_yaw + 360f) % 360f;

            _pitch = Mathf.Clamp(
                _pitch - deltaPitch * verticalSensitivity,
                minimumPitch, maximumPitch);

            var newEuler = transform.eulerAngles;
            newEuler.y = _yaw;
            transform.eulerAngles = newEuler;

            var newCameraEuler = cameraHandle.transform.eulerAngles;
            newCameraEuler.x = _pitch;
            cameraHandle.transform.eulerAngles = newCameraEuler;
        }

        // Temporary (?) implementation of toggling cursor lock state
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            Cursor.lockState = Cursor.lockState == CursorLockMode.Locked
                ? CursorLockMode.None
                : CursorLockMode.Locked;
        }
    }

    void FixedUpdate()
    {
        UpdateGroundedState();

        if (_isGrounded && isJumpQueued)
        {
            Jump();
            _bufferedJumpTimer = 0f;
        }

        var moveVector = transform.TransformVector(_moveInput.normalized);
        moveVector *= moveSpeed * (_isGrounded ? 1f : airControl);
        moveVector.y = body.velocity.y;
        SetVelocity(moveVector, speedChangeRate);
    }

    void Jump()
    {
        var newVelocity = body.velocity;
        newVelocity.y = 0f;
        SetVelocity(newVelocity);
        body.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    void UpdateGroundedState()
    {
        if (body.velocity.y <= 0.001f
            && Physics.Raycast(
                body.transform.position,
                Vector3.down,
                groundedCheckRange,
                groundLayerMask))
        {
            _isGrounded = true;
        }
        else
        {
            _isGrounded = false;
        }
    }

    void SetVelocity(Vector3 velocity)
    {
        body.AddForce(velocity - body.velocity, ForceMode.VelocityChange);
    }

    void SetVelocity(Vector3 targetVelocity, float lerpRate)
    {
        var actualVelocity = Vector3.Lerp(body.velocity, targetVelocity, lerpRate);
        body.AddForce(actualVelocity - body.velocity, ForceMode.VelocityChange);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(body.transform.position, Vector3.down * groundedCheckRange);
    }
}
