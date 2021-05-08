using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplineTrampolean : MonoBehaviour
{
    [SerializeField] private List<Vector3> _waypoints = new List<Vector3>();

    public List<Vector3> waypoints => _waypoints;

    public void OnTriggerEnter(Collider other)
    {
        other.transform.position = waypoints[1];
    }
}
