using System.Collections;
using UnityEngine;

public class BezierTrampoline : MonoBehaviour
{
    public Transform peak;
    public Transform end;

    public int debugLineSegments = 60;

    void OnTriggerEnter(Collider collider)
    {
        StartCoroutine(SendFlying(collider.transform));
    }

    void OnCollisionEnter(Collision collision)
    {
        StartCoroutine(SendFlying(collision.transform));
    }

    IEnumerator SendFlying(Transform tx)
    {
        var t = 0.01f;

        while (t < 1f)
        {
            tx.position = CalculatePosition(t);
            yield return new WaitForSeconds(0.01f);
            t += 0.01f;
        }

        tx.position = CalculatePosition(1f);
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
    }
}