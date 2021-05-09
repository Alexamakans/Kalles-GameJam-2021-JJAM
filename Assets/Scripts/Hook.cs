using System.Linq;
using UnityEngine;

public class Hook : MonoBehaviour
{
    public float range = 10f;
    public float autoTargetRadius = 3f;
    public float endHookDistance = 1f;
    [Range(0, 10)]
    [Tooltip("In units per second")]
    public float hookMoveSpeed = 6.0f;
    public float hookEndUpForce = 750f;
    public float hookCancelUpForce = 400f;
    public AnimationCurve hookMovementCurve = new AnimationCurve(
        new Keyframe(0, 0),
        new Keyframe(1, 1));
    public bool useCurveForHookSpeedInstead = false;
    [Tooltip("Used only for curve based movement")]
    public float hookBaseDuration = 0.3f;
    [Tooltip("Used only for curve based movement")]
    public float hookDurationPerUnit = 0.06f;
    [Range(0, 5)]
    public float hookCancelPercentageOfVelocity = 1f;
    public Transform raycastFrom;
    public LayerMask hitLayerMask;
    public LayerMask hookableLayerMask;
    public Rigidbody body;

    private GameObject _target;
    private Vector3 _targetPosition;
    private Vector3 _raycastSphereStop;
    private bool _isHooking;
    private Vector3 _hookStartPosition;
    private FpsController _fpsController;
    private float _hookPercentageDone;
    private float _hookLength;

    public static System.Action<HookPoint> hookPointFound;

    void Reset()
    {
        body = GetComponentInChildren<Rigidbody>();
    }

    void Start()
    {
        _fpsController = GetComponent<FpsController>();
    }

    void Update()
    {
        if (_isHooking)
        {
            if (useCurveForHookSpeedInstead)
            {
                transform.position = Vector3.Lerp(_hookStartPosition, _targetPosition, hookMovementCurve.Evaluate(_hookPercentageDone));
                _hookPercentageDone += Time.deltaTime / (hookBaseDuration + hookDurationPerUnit * _hookLength);
            }
            else
            {
                transform.position = Vector3.MoveTowards(transform.position, _targetPosition, Time.deltaTime * hookMoveSpeed);
            }
            var toTarget = _targetPosition - body.position;
            var distanceLeft = toTarget.magnitude;

            if (distanceLeft <= endHookDistance)
            {
                _hookPercentageDone = 0f;
                _isHooking = false;
                body.isKinematic = false;
                body.velocity = Vector3.zero;
                body.AddForce(Vector3.up * hookEndUpForce, ForceMode.Impulse);
            }
            else if (Input.GetButtonDown("Jump"))
            {
                _hookPercentageDone = 0f;
                _isHooking = false;
                body.isKinematic = false;
                body.velocity = Vector3.zero;
                body.AddForce(Vector3.up * hookEndUpForce, ForceMode.Impulse);
                body.AddForce(toTarget.normalized * hookCancelPercentageOfVelocity, ForceMode.VelocityChange);
            }
        }
        else if (_target)
        {
            if (Input.GetButtonDown("Fire1"))
            {
                ShootHook();
            }
        }
    }

    void FixedUpdate()
    {
        if (!_isHooking)
        {
            Targeting();
        }
    }

    private void ShootHook()
    {
        if (!_target)
        {
            Debug.LogWarning($"Can't use hook without a target: {nameof(_target)} was null", this);
            return;
        }

        if (_isHooking)
        {
            Debug.LogWarning($"Can't use hook while already hooking: {nameof(_isHooking)} was true", this);
            return;
        }

        _isHooking = true;
        body.isKinematic = true;
        body.velocity = Vector3.zero;
        _hookStartPosition = body.position;
        _hookLength = (_targetPosition - _hookStartPosition).magnitude;
    }

    private void Targeting()
    {
        var ray = new Ray(raycastFrom.position, raycastFrom.forward);
        if (Physics.Raycast(ray, out var info, range, hitLayerMask))
        {
            _raycastSphereStop = info.point;
        }
        else
        {
            _raycastSphereStop = ray.origin + ray.direction * range;
        }

        var playerToSphereStopDistance = (_raycastSphereStop - raycastFrom.position).magnitude;
        var hits = Physics.SphereCastAll(
            ray,
            autoTargetRadius,
            playerToSphereStopDistance,
            hookableLayerMask).AsEnumerable();

        hits = from hit in hits
               where IsVisibleForPlayer(hit)
               where IsAboveish(hit)
               orderby DistanceToPlayer(hit) // Ascending, shortest first
               select hit;

        if (hits.Count() > 0)
        {
            var hit = hits.First();
            _target = hit.collider.gameObject;

            if (Mathf.Approximately(Vector3.Dot(hit.normal, ray.direction), -1f))
            {
                // Selected collider was inside the start of the spherecast
                // we need to do a standard raycast on it to
                // get a proper hit.point
                if (Physics.Raycast(
                    raycastFrom.position,
                    (_target.transform.position - raycastFrom.position),
                    out var preciseInfo,
                    range,
                    hookableLayerMask))
                {
                    if (hit.collider != preciseInfo.collider)
                    {
                        Debug.LogWarning("Odd result, should probably investigate this", this);
                    }
                    hit = preciseInfo;
                }
            }

            _targetPosition = hit.point;
        }
        else
        {
            _target = null;
        }

        //Oooga boogaa
        var hookpoint = _target ? _target.GetComponent<HookPoint>() : null;
        //Debug.Log(hookPointFound.GetInvocationList().Length > 0);
        hookPointFound.Invoke(hookpoint);
    }

    private bool IsAboveish(RaycastHit hit)
    {
        var hookPoint = hit.collider.GetComponent<HookPoint>();

        if (!hookPoint)
        {
            Debug.LogError("Hookable object without a HookPoint component!", hit.collider.gameObject);
            return false;
        }

        var maxTargetAngle = hookPoint.maxTargetAngle;
        var toPoint = (hit.collider.transform.position - raycastFrom.position);

        return Vector3.Dot(raycastFrom.forward, toPoint.normalized)
            > Mathf.Cos(Mathf.Deg2Rad * (90f - maxTargetAngle));
    }

    private bool IsVisibleForPlayer(RaycastHit hit)
    {
        var pos = hit.collider.transform.position;
        var toPlayer = raycastFrom.position - pos;
        var distanceToPlayer = toPlayer.magnitude;
        var toPlayerRay = new Ray(pos, toPlayer.normalized);

        if (Physics.Raycast(toPlayerRay, distanceToPlayer, hitLayerMask))
        {
            return false;
        }

        return true;
    }

    private float DistanceToPlayer(RaycastHit hit)
    {
        var pos = hit.collider.transform.position;
        var toPlayer = raycastFrom.position - pos;
        return toPlayer.magnitude;
    }

    void OnDrawGizmos()
    {
        if (_target)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(_target.transform.position, 1.5f);
            Gizmos.DrawWireSphere(_targetPosition, 1.5f);
        }
        if (!Application.isPlaying)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, range);
        }
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_raycastSphereStop, autoTargetRadius);
    }
}
