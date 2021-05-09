using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBob : MonoBehaviour
{
    public FpsController fpsController;
    public Transform cam;
    public float verticalBobFrequency = 1f;
    public float verticalBobStrength = 1f;
    public float horizontalBobFrequency = 1f;
    public float horizontalBobStrength = 1f;
    [Range(0, 1)]
    public float resetSpeed = 0.1f;

    private Vector3 _originalLocalPosition;
    private float _bobTimer = 0f;

    void Reset()
    {
        fpsController = GetComponent<FpsController>();
    }

    void Awake()
    {
        _originalLocalPosition = cam.localPosition;
    }

    void Update()
    {
        var planeVelocity = fpsController.GetPlaneVelocity();
        var planeSqrMagnitude = planeVelocity.sqrMagnitude;

        var newPosition = cam.transform.localPosition;
        if (planeSqrMagnitude < 1e-2f)
        {
            _bobTimer = 0f;
             newPosition = Vector3.Lerp(newPosition, _originalLocalPosition, resetSpeed);
        }
        else
        {
            _bobTimer += Time.deltaTime * Mathf.PI * 2f * Mathf.Sqrt(planeSqrMagnitude * 0.2f);
            newPosition.x = _originalLocalPosition.x + Mathf.Sin(_bobTimer * horizontalBobFrequency) * horizontalBobStrength;
            newPosition.y = _originalLocalPosition.y + Mathf.Sin(_bobTimer * verticalBobFrequency) * verticalBobStrength;
        }

        cam.transform.localPosition = newPosition;
    }
}
