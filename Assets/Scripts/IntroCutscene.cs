using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class IntroCutscene : MonoBehaviour
{
    public AudioClip ringTone;
    public AudioClip speakingClip;
    public AudioSource source;
    public float ringDuration;
    public float phoneShakeFrequency = Mathf.PI * 0.5f * 5f;
    public float phoneShakeStrength = 25f;

    public GameObject player;

    public GameObject canvas;
    public GameObject phoneImageParent;
    public GameObject stonerDudeImage;

    public AudioListener listener;
    public Camera cam;

    void Awake()
    {
        Play();
        listener.enabled = true;
    }

    void Play()
    {
        player.SetActive(false);
        cam.enabled = true;
        StartCoroutine(Play_Coroutine());
    }

    IEnumerator Play_Coroutine()
    {
        yield return new WaitForSeconds(2f);

        var t = 0f;

        source.PlayOneShot(ringTone);

        while (t < ringDuration)
        {
            var newEuler = phoneImageParent.transform.eulerAngles;
            newEuler.z = Mathf.Sin(t * phoneShakeFrequency) * phoneShakeStrength;

            phoneImageParent.transform.eulerAngles = newEuler;

            yield return new WaitForSeconds(0.01f);
            t += 0.01f;
        }

        phoneImageParent.transform.eulerAngles = Vector3.zero;
        source.Stop();

        yield return new WaitForSeconds(0.5f);
        stonerDudeImage.SetActive(true);

        yield return new WaitForSeconds(0.8f);

        source.PlayOneShot(speakingClip);


        yield return new WaitForSeconds(1.5f + speakingClip.length);

        canvas.SetActive(false);
        listener.enabled = false;
        cam.enabled = false;
        player.SetActive(true);
    }
}
