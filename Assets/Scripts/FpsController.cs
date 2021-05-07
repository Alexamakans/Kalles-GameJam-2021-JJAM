using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FpsController : MonoBehaviour
{
    public Rigidbody body;
    
    [Header("Movement Settings")]
    public float moveSpeed = 6f;
    public float jumpForce = 380f;
    public float jumpBufferTime = 0.2f;
    public float groundedCheckRange = 1.1f;
    public LayerMask groundLayerMask;

    private float _bufferedJumpTimer;
    private bool isJumpQueued => _bufferedJumpTimer > 0f;
    [SerializeField]
    private bool _isGrounded = false;

    private Vector3 _moveInput;

    void Reset()
    {
        body = GetComponentInChildren<Rigidbody>();
#if false
        // Default settings for rigidbody
        body.mass = 75f;
        body.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
#endif
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
    }

    void FixedUpdate()
    {
        UpdateGroundedState();

        if (_isGrounded && isJumpQueued)
        {
            Jump();
            _bufferedJumpTimer = 0f;
        }

        if (_isGrounded)
        {
            // Ground control
            SetVelocity(_moveInput * moveSpeed);
        }
        else
        {
            // Air control
        }
    }

    void Jump()
    {
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

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(body.transform.position, Vector3.down * groundedCheckRange);
    }

    void SetVelocity(Vector3 velocity)
    {
        body.AddForce(velocity - body.velocity, ForceMode.VelocityChange);
    }
}
