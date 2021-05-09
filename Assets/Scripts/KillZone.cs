using UnityEngine;

[RequireComponent(typeof(Collider))]
public class KillZone : MonoBehaviour
{
    void OnTriggerEnter(Collision other)
    {
        if (other.gameObject.GetComponent<FpsController>())
        {
            GameLoop.onPlayerDeath?.Invoke();
        }
    }
}
