using UnityEngine;

[RequireComponent(typeof(Collider))]
public class KillZone : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<FpsController>())
        {
            GameLoop.onPlayerDeath?.Invoke();
        }
    }
}
