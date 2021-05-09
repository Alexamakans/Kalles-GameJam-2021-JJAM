using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Checkpoint : MonoBehaviour
{
    public Transform respawnTransform;
    public float flashDuration = 2.5f;
    public float flashInterval = 0.5f;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var fpsController = other.GetComponent<FpsController>();

            var wasHere = fpsController.respawnPosition == respawnTransform.position;
            
            fpsController.respawnPosition = respawnTransform.position;
            fpsController.respawnYaw = respawnTransform.eulerAngles.y;
            fpsController.respawnPitch = respawnTransform.eulerAngles.x;

            if (!wasHere)
            {
                StartCoroutine(FlashCheckpointReached_Coroutine());
            }
        }
    }

    IEnumerator FlashCheckpointReached_Coroutine()
    {
        var txtGO = GameObject.FindGameObjectWithTag("Checkpoint Text");
        var txt = txtGO.GetComponent<Text>();
        var bgGO = GameObject.FindGameObjectWithTag("Checkpoint Text BG");
        var bg = bgGO.GetComponent<Image>();

        var enable = true;
        var t = 0f;

        while (t < flashDuration)
        {
            txt.enabled = enable;
            bg.enabled = enable;

            yield return new WaitForSeconds(flashInterval);
            t += flashInterval;
            enable = !enable;
        }

        txt.enabled = false;
        bg.enabled = false;
    }
}
