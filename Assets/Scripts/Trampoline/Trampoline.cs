using UnityEngine;

#if UNITY_EDITOR
[ExecuteAlways]
#endif
public class Trampoline : MonoBehaviour
{
    public float launchForce = 20f;
    public Transform directionMarker;
    
    public Vector3 launchDirection { get; private set; }

    void Update()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            var markerDir = directionMarker.position - transform.position;
            launchDirection = markerDir.normalized;
            directionMarker.position = transform.position
                + launchDirection * Mathf.Max(launchForce * 0.01f, 2f);
        }
#endif
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawRay(transform.position, launchDirection * Mathf.Max(launchForce * 0.01f, 2f));
    }

    void OnCollisionEnter(Collision other)
    {
        if (other.rigidbody)
        {
            other.rigidbody.velocity = Vector3.zero;
            other.rigidbody.AddForce(launchDirection * launchForce, ForceMode.VelocityChange);
        }
    }
}
