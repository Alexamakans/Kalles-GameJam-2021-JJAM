using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HookPoint : MonoBehaviour
{
    [Header("Anim")]
    [SerializeField] Image _circle;
    [SerializeField] Image _mouseImage;
    [SerializeField] AnimationCurve _sizeCurve;
    [SerializeField] float _curveTime;

    [Header("Handle")]
    [SerializeField] GameObject _canvasHandle;
    [SerializeField] LayerMask _visibilityBlockers;
    [Range(1f, 90f)]
    [Tooltip(
        "Max angle between player forward and hook position in degrees.\n" +
        "Higher requires more precision.")]
    public float maxTargetAngle = 60f;

    float _lerpValue;
    bool _active;
    float _inverseCurveTime;

    Vector3 mainCameraDir => (_mainCamera.transform.position - transform.position).normalized;

    Camera _mainCamera;


    public void Start()
    {
        _inverseCurveTime = 1 / _curveTime;
        _mainCamera = Camera.main;
        Hook.hookPointFound += Activate;
    }

    void UpdateVisible()
    {
        var Blocked = Physics.Linecast(
            transform.position,
            _mainCamera.transform.position,
            _visibilityBlockers);
        _canvasHandle.SetActive(!Blocked);
    }

    void Activate(Transform t) => _active = t == transform;

    void Update()
    {
        UpdateVisible();
        UpdateSize();
        _canvasHandle.transform.rotation = Quaternion.LookRotation(mainCameraDir);
    }

    void UpdateSize()
    {
        var lerpIncrementFactor = _active ? 1 : -1;
        var newLerpValue = _lerpValue + (Time.deltaTime * lerpIncrementFactor * _inverseCurveTime);

        _lerpValue = Mathf.Clamp01(newLerpValue);

        _circle.transform.localScale = Vector2.one * _sizeCurve.Evaluate(_lerpValue);
        _mouseImage.gameObject.SetActive(_lerpValue == 1);
    }
}

// Pizza Jump Man Copyright
// Deep Dish Pizza Dash