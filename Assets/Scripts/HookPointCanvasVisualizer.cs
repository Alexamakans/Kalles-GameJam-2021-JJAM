using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HookPointCanvasVisualizer : MonoBehaviour
{
    //jag copypastade koden från den andra hookpoint 
    [Header("Anim")]
    [SerializeField] Image _circle;
    [SerializeField] Image _mouseImage;
    [SerializeField] AnimationCurve _sizeCurve;
    [SerializeField] float _curveTime;

    [Header("Handle")]
    [SerializeField] LayerMask _visibilityBlockers;
    [SerializeField] GameObject _imageHandle;
    [Range(1f, 90f)]
    [Tooltip(
        "Max angle between player forward and hook position in degrees.\n" +
        "Higher requires more precision.")]
    public float maxTargetAngle = 30f;

    private Transform _hookPoint;
    float _lerpValue;
    bool _active;
    float _inverseCurveTime;

    Vector3 mainCameraDir => (_mainCamera.transform.position - _hookPoint.position).normalized;

    Camera _mainCamera;


    public void Start()
    {
        Hook.hookPointFound += Activate;
        _inverseCurveTime = 1 / _curveTime;
        _mainCamera = Camera.main;
    }

    void UpdateVisible()
    {
        if (_hookPoint == null)
        {
            _imageHandle.SetActive(false);
            return;
        }

        var Blocked = Physics.Linecast(
            _mainCamera.transform.position,
            _hookPoint.position,
            _visibilityBlockers);
        _imageHandle.SetActive(!Blocked);
    }

    void Activate(Transform transform)
    {
        _active = transform != null;
        _hookPoint = transform;
    }

    void Update()
    {
        if (!_mainCamera)
        {
            // stuff with disabling things and stuff ooops
            _mainCamera = Camera.main;
        }

        UpdateVisible();
        UpdatePosition();
        UpdateSize();
    }

    void UpdateSize()
    {
        var lerpIncrementFactor = _active ? 1 : -1;
        var newLerpValue = _lerpValue + (Time.deltaTime * lerpIncrementFactor * _inverseCurveTime);

        _lerpValue = Mathf.Clamp01(newLerpValue);

        _circle.transform.localScale = Vector2.one * _sizeCurve.Evaluate(_lerpValue);
        _mouseImage.gameObject.SetActive(_lerpValue == 1);
    }
    void UpdatePosition()
    {
        if (_active)
        {
            _imageHandle.GetComponent<RectTransform>().anchoredPosition = _mainCamera.WorldToScreenPoint(_hookPoint.transform.position);
        }
    }
}
