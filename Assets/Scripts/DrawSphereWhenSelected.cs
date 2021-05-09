using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawSphereWhenSelected : MonoBehaviour
{
    public float sphereSize = 1f;
    public bool wireFrame = true;
    public Color color = Color.blue;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = color;
        if (wireFrame)
        {
            Gizmos.DrawWireSphere(transform.position, sphereSize * 0.5f);
        }
        else
        {
            Gizmos.DrawSphere(transform.position, sphereSize * 0.5f);
        }
    }
}
