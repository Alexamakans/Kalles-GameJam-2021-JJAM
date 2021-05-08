using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class Trampolean : MonoBehaviour
{
    [SerializeField] float _launchForce = 20f;
    [SerializeField] Vector3 _direction = Vector3.up;
    [SerializeField] Transform _directionMarker;

    Vector3 _launchDirection => _direction;

    void Update()
    {
        if (!Application.isPlaying)
        {
            var markerDir = _directionMarker.position - transform.position;
            _directionMarker.position = transform.position + markerDir.normalized;
        }
    }

    void OnDrawGizmosSelected()
    {
        Debug.DrawRay(transform.position, _launchDirection);
    }

    void OnCollisionEnter(Collision other)
    {
        other.rigidbody.velocity = Vector3.zero;
        other.rigidbody.AddForce((_directionMarker.position - transform.position) * _launchForce, ForceMode.VelocityChange);
    }

}
