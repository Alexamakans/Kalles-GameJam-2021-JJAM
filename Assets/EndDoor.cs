using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndDoor : MonoBehaviour
{
    public Transform doorParent;
    public Transform player;
    public Transform camHandle;
    public Transform pizzaHandle;
    public Transform pizza;

    private Rigidbody _body;
    private FpsController _fpsController;

    public Transform lerpTo1;
    public float lerpSpeed1 = 0.1f;
    public float targetYaw1 = -20.582f;
    public float lerpYawSpeed1 = 0.08f;
    public float targetPitch1 = -5f;
    public float lerpPitchSpeed1 = 0.1f;
    public float doorParentYawTarget1 = -143.015f;

    private AudioSource _audio;
    private bool _gameEnded = false;

    private void Awake()
    {
        _audio = GetComponent<AudioSource>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!_gameEnded && other.CompareTag("Player"))
        {
            _body = player.GetComponent<Rigidbody>();
            _fpsController = player.GetComponent<FpsController>();
            _gameEnded = true;
            StartCoroutine(EndScene_Coroutine());
        }
    }

    IEnumerator EndScene_Coroutine()
    {
        _body.isKinematic = true;
        _fpsController.isInputEnabled = false;
       // yield return new WaitForSeconds(0.5f);
        _body.isKinematic = true;

        var distance = (player.position - lerpTo1.position).sqrMagnitude;

        // Move player to door and look at it
        while (
            distance > 0.01f ||
            _fpsController.yaw - targetYaw1 > 0.01f ||
            _fpsController.pitch - targetPitch1 > 0.01f)
        {
            player.position = Vector3.Lerp(player.position, lerpTo1.position, lerpSpeed1);
            _fpsController.yaw = Mathf.LerpAngle(_fpsController.yaw, targetYaw1, lerpYawSpeed1);
            _fpsController.pitch = Mathf.LerpAngle(_fpsController.pitch, targetPitch1, lerpPitchSpeed1);

            _fpsController.UpdateCamera();

            distance = (player.position - lerpTo1.position).sqrMagnitude;

            yield return new WaitForSeconds(0.01f);
        }

        // Spawn dude image
        // TODO

        // Open door
        var t = 0f;
        var startYaw = doorParent.eulerAngles.y;
        var openDoorDuration = 1.5f;
        _audio.PlayDelayed(openDoorDuration * 0.25f);
        while (t < 1f)
        {
            var yaw = Mathf.LerpAngle(startYaw, doorParentYawTarget1, t * t);

            doorParent.eulerAngles = new Vector3(
                doorParent.eulerAngles.x,
                yaw,
                doorParent.eulerAngles.z
            );

            yield return new WaitForSeconds(0.01f);
            t += 0.01f / openDoorDuration;
        }

        doorParent.eulerAngles = new Vector3(
            doorParent.eulerAngles.x,
            doorParentYawTarget1,
            doorParent.eulerAngles.z);

        _body.isKinematic = false;
        _fpsController.isInputEnabled = true;
    }
}