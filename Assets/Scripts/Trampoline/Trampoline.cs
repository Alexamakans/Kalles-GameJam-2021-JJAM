using UnityEngine;

#if UNITY_EDITOR
[ExecuteAlways]
#endif
public class Trampoline : MonoBehaviour
{
    public float launchForce = 20f;
    public Transform directionMarker;
    
    public Vector3 launchDirection { get; private set; }

    void Awake()
    {
        if (!directionMarker)
        {
            Debug.LogError($"{nameof(directionMarker)} was null. SET IT!! >:)");
            return;
        }

        launchDirection = (directionMarker.position - transform.position).normalized;
    }

#if UNITY_EDITOR
    void Update()
    {
        if (!Application.isPlaying)
        {
            var markerDir = directionMarker.position - transform.position;
            launchDirection = markerDir.normalized;
            directionMarker.position = transform.position
                + launchDirection * Mathf.Max(launchForce * 0.01f, 2f);
        }
    }
#endif

    void OnDrawGizmos()
    {
        Gizmos.DrawRay(transform.position, launchDirection * Mathf.Max(launchForce * 0.01f, 2f));
    }

    void OnTriggerEnter(Collider collider)
    {
        if (collider.TryGetComponent<Rigidbody>(out var otherBody))
        {
            if (collider.CompareTag("Player"))
            {
                var fpsController = collider.GetComponent<FpsController>();
                var clip = fpsController.jumpClips.GetClip();
                if (clip)
                {
                    fpsController.aodiuSource.PlayOneShot(clip);
                }
            }

            otherBody.velocity = Vector3.zero;
            otherBody.AddForce(launchDirection * launchForce, ForceMode.VelocityChange);
        }
    }

    void OnCollisionEnter(Collision other)
    {
        if (other.rigidbody)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                var fpsController = other.gameObject.GetComponent<FpsController>();
                var clip = fpsController.jumpClips.GetClip();
                if (clip)
                {
                    fpsController.aodiuSource.PlayOneShot(clip);
                }
            }

            other.rigidbody.velocity = Vector3.zero;
            other.rigidbody.AddForce(launchDirection * launchForce, ForceMode.VelocityChange);
        }
    }
}