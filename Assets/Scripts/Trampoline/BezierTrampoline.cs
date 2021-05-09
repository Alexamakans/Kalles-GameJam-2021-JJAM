using System.Collections;
using UnityEngine;

public class BezierTrampoline : MonoBehaviour
{
    public Transform peak;
    public Transform end;

    public AnimationCurve speedCurve;
    public float jumpDurationSeconds = 1f;

    public int debugLineSegments = 60;

    void OnTriggerEnter(Collider collider)
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

        StartCoroutine(SendFlying(collider.transform));
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            var fpsController = collision.gameObject.GetComponent<FpsController>();
            var clip = fpsController.jumpClips.GetClip();
            if (clip)
            {
                fpsController.aodiuSource.PlayOneShot(clip);
            }
        }

        StartCoroutine(SendFlying(collision.transform));
    }

    IEnumerator SendFlying(Transform tx)
    {
        var t = 0f;

        //var wasKinematic = false;

        if (tx.TryGetComponent(out Rigidbody body))
        {
            //wasKinematic = body.isKinematic;
            body.isKinematic = true;
        }

        while (t < 1f)
        {
            tx.position = CalculatePosition(speedCurve.Evaluate(t));
            yield return new WaitForFixedUpdate();//new WaitForSeconds(1f / 60f);
            t += Time.fixedDeltaTime / jumpDurationSeconds;
        }

        tx.position = CalculatePosition(1f);

        if (body)
        {
            body.isKinematic = false;
            //body.isKinematic = wasKinematic;
        }
    }

    // 3-point bezier curve
    Vector3 CalculatePosition(float t)
    {
        var p = end.position;
        var q = peak.position;
        var r = transform.position;

        //P(t) = P0*t^2 + P1*2*t*(1-t) + P2*(1-t)^2
        return p * t * t + q * 2 * t * (1 - t) + r * (1 - t) * (1 - t);
    }

    void OnDrawGizmos()
    {
        var t = 0f;
        var from = CalculatePosition(t);

        for (var i = 0; i < debugLineSegments; ++i)
        {
            t = i / (float)debugLineSegments;

            var to = CalculatePosition(t);
            Gizmos.color = new Color(1f - t, Mathf.Max(0.3f, 1f - t * t), Mathf.Max(0.3f, 1f - Mathf.Sqrt(t)));
            Gizmos.DrawLine(from, to);

            from = to;
        }

        Gizmos.color = new Color(1f, 0f, 0f);
        Gizmos.DrawLine(from, CalculatePosition(1f));

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(CalculatePosition(0.5f), 0.2f);
    }
}