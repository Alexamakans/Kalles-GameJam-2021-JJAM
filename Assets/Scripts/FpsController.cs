using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FpsController : MonoBehaviour
{
    public Rigidbody body;
    public GameObject cameraHandle;
    
    [Header("Movement Settings")]
    public float walkAcceleration = 6f;
    public float sprintAcceleration = 8f;
    public float maxWalkSpeed = 5f;
    public float maxSprintSpeed = 8f;
    [Range(0, 1)]
    public float airTurnSpeed = 0.02f;
    public float groundTurnSpeed = 0.25f;
    public float airDrag = 0.6f;
    public float groundDrag = 0.6f;
    [Range(0, 1)]
    public float groundControl = 1f;
    [Range(0, 1)]
    public float airControl = 0f;

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

    private Vector3 _moveInput;
    private bool isSprinting => _wasSprinting || (_isGrounded && Input.GetButton("Sprint"));

    private float _yaw = 0f;
    private float _pitch = 0f;

    private bool _isGrounded = false;
    [SerializeField]
    private bool _wasSprinting = false;

    private float moveAcceleration => isSprinting ? sprintAcceleration : walkAcceleration;
    private float moveControl => _isGrounded ? groundControl : airControl;
    private float maxSpeed => isSprinting ? maxSprintSpeed : maxWalkSpeed;
    private bool isJumpQueued => _bufferedJumpTimer > 0f;
    private float drag => _isGrounded ? groundDrag : airDrag;
    private float turnSpeed => _isGrounded ? groundTurnSpeed : airTurnSpeed;

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
            _wasSprinting = isSprinting;
            Jump();
            _bufferedJumpTimer = 0f;
        }

        var newVelocity = GetVelocity();
        newVelocity.x *= 1f - drag;
        newVelocity.z *= 1f - drag;
        SetVelocity(newVelocity);

        var moveVector = transform.TransformVector(_moveInput.normalized);

        var planeVelocity = GetPlaneVelocity();
        var VoM = Vector3.Dot(planeVelocity.normalized, moveVector);
     
        if (VoM < 0.0f || (planeVelocity.magnitude <= maxSpeed))
        {
            moveVector *= moveAcceleration * moveControl;
            body.AddForce(moveVector, ForceMode.Acceleration);
        }
        else
        {
            if (Mathf.Approximately(moveVector.sqrMagnitude, 0f))
            {
                moveVector = planeVelocity.normalized;
            }

            var rotatedVelocity = Vector3.Slerp(planeVelocity.normalized, moveVector, turnSpeed) * planeVelocity.magnitude;
            rotatedVelocity.y = body.velocity.y;
            SetVelocity(rotatedVelocity);
        }
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
            _wasSprinting = false;
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

    public Vector3 GetVelocity()
    {
        return body.velocity;
    }

    public Vector3 GetPlaneVelocity()
    {
        return Vector3.Scale(body.velocity, Vector3.forward + Vector3.right);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(body.transform.position, Vector3.down * groundedCheckRange);
    }
}
