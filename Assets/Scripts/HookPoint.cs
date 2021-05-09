using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HookPoint : MonoBehaviour
{
    [SerializeField] Image _circle;
    [SerializeField] Image _mouseImage;
    [SerializeField] AnimationCurve _sizeCurve;
    [SerializeField] float _curveTime;
    [Header("Handle")]
    [SerializeField] GameObject _canvasHandle;
    float _lerpValue;
    bool _active;
    float _inverseCurveTime;

    Vector3 mainCameraDir => (_mainCamera.transform.position - transform.position).normalized;

    Camera _mainCamera;

    public void Start()
    {
        Hook.hookPointFound += Activate;
        _inverseCurveTime = 1 / _curveTime;
        _mainCamera = Camera.main;
    }

    void UpdateVisible()
    {

    }

    void Activate(HookPoint hp) => _active = hp == this;

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
