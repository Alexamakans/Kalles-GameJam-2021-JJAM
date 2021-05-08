using System.Linq;
using UnityEngine;

public class Hook : MonoBehaviour
{
    public float range = 10f;
    public float autoTargetRadius = 3f;

    public Transform raycastFrom;

    public LayerMask hitLayerMask;
    public LayerMask hookableLayerMask;

    private GameObject _target;
    private Vector3 _raycastSphereStop;

    void FixedUpdate()
    {
        var ray = new Ray(raycastFrom.position, raycastFrom.forward);
        if (Physics.Raycast(ray, out var info, range, hitLayerMask))
        {
            _raycastSphereStop = info.point;
        }
        else
        {
            _raycastSphereStop = ray.origin + ray.direction * range;
        }

        var playerToSphereStop = _raycastSphereStop - raycastFrom.position;
        var hits = Physics.SphereCastAll(
            ray,
            autoTargetRadius,
            playerToSphereStop.magnitude,
            hookableLayerMask).AsEnumerable();
        hits = from hit in hits
               where IsVisibleForPlayer(hit)
               orderby DistanceToPlayer(hit) // Ascending, shortest first
               select hit;

        if (hits.Count() > 0)
        {
            _target = hits.First().collider.gameObject;
        }
        else
        {
            _target = null;
        }
    }

    private bool IsVisibleForPlayer(RaycastHit hit)
    {
        var pos = hit.collider.transform.position;
        var toPlayer = raycastFrom.position - pos;
        var distanceToPlayer = toPlayer.magnitude;
        var toPlayerRay = new Ray(pos, toPlayer.normalized);

        if (Physics.Raycast(toPlayerRay, distanceToPlayer, hitLayerMask))
        {
            return false;
        }

        return true;
    }

    private float DistanceToPlayer(RaycastHit hit)
    {
        var pos = hit.collider.transform.position;
        var toPlayer = raycastFrom.position - pos;
        return toPlayer.magnitude;
    }

    void OnDrawGizmos()
    {
        if (_target)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(_target.transform.position, 1.5f);
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_raycastSphereStop, autoTargetRadius);
    }
}
