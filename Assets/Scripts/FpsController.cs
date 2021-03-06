using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FpsController : MonoBehaviour
{
    public Rigidbody body;
    public GameObject cameraHandle;
    public CapsuleCollider capsuleCollider;

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

    public Vector3 respawnPosition;
    public float respawnYaw = 0f;
    public float respawnPitch = 0f;

    public float yaw = 0f;
    public float pitch = 0f;
    public bool isInputEnabled = true;

    private float _bufferedJumpTimer;

    private Vector3 _moveInput;
    private bool isSprinting => _wasSprinting || (_isGrounded && Input.GetButton("Sprint"));

    private bool _isGrounded = false;
    [SerializeField]
    private bool _wasSprinting = false;
    [Header("Audio")]
    public RandomizingAodiuPhile jumpClips;
    public RandomizingAodiuPhile landingClips;
    public RandomizingAodiuPhile theOnePizzaClips;
    public AudioSource aodiuSource;

    private float moveAcceleration => isSprinting ? sprintAcceleration : walkAcceleration;
    private float moveControl => _isGrounded ? groundControl : airControl;
    private float maxSpeed => isSprinting ? maxSprintSpeed : maxWalkSpeed;
    private bool isJumpQueued => _bufferedJumpTimer > 0f;
    private float drag => _isGrounded ? groundDrag : airDrag;
    private float turnSpeed => _isGrounded ? groundTurnSpeed : airTurnSpeed;

    void Reset()
    {
        body = GetComponentInChildren<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        aodiuSource = GetComponent<AudioSource>();
#if false
        // Default settings for rigidbody
        body.mass = 75f;
        body.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
#endif
    }

    void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        GameLoop.onPlayerDeath += OnDeath;
    }

    void Update()
    {
        if (isInputEnabled)
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

                yaw += deltaYaw * horizontalSensitivity;
                // Constraint: -180 < _yaw <= 180
                yaw = (yaw + 360f) % 360f;

                pitch = Mathf.Clamp(
                    pitch - deltaPitch * verticalSensitivity,
                    minimumPitch, maximumPitch);
            }
        }
        else
        {
            _moveInput = Vector3.zero;
        }

        UpdateCamera();

        // Temporary (?) implementation of toggling cursor lock state
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            Cursor.lockState = Cursor.lockState == CursorLockMode.Locked
                ? CursorLockMode.None
                : CursorLockMode.Locked;
        }
    }

    public void UpdateCamera()
    {
        var newEuler = transform.eulerAngles;
        newEuler.y = yaw;
        transform.eulerAngles = newEuler;

        var newCameraEuler = cameraHandle.transform.eulerAngles;
        newCameraEuler.x = pitch;
        cameraHandle.transform.eulerAngles = newCameraEuler;
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

        var clip = jumpClips.GetClip();
        if (clip)
        {
            aodiuSource.PlayOneShot(clip);
        }
    }

    void UpdateGroundedState()
    {
        var ray = new Ray(body.transform.position, Vector3.down);
        if (Physics.SphereCast(
                ray: ray,
                radius: capsuleCollider.radius,
                maxDistance: groundedCheckRange - capsuleCollider.radius,
                layerMask: groundLayerMask,
                hitInfo: out var hit))
        {
            var VelDotNormal = Vector3.Dot(body.velocity.normalized, hit.normal.normalized);

            if (VelDotNormal < 0.1f)
            {
                if (!_isGrounded && body.velocity.y < -1.5f)
                {
                    var clip = landingClips.GetClip();
                    if (clip)
                    {
                        aodiuSource.PlayOneShot(clip);
                    }
                }

                _isGrounded = true;
                _wasSprinting = false;
                return;
            }
        }

        _isGrounded = false;
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

    void OnDeath()
    {
        StartCoroutine(OnDeath_Coroutine());
    }

    IEnumerator OnDeath_Coroutine()
    {
        var clip = theOnePizzaClips.GetClip();

        if (clip)
        {
            aodiuSource.PlayOneShot(clip);
            yield return new WaitForSeconds(clip.length + 0.5f);
        }

        transform.position = respawnPosition;
        body.velocity = Vector3.zero;
        yaw = respawnYaw;
        pitch = respawnPitch;
        _bufferedJumpTimer = 0f;
    }
}
