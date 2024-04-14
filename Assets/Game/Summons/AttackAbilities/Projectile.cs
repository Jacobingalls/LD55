using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Projectile : MonoBehaviour, IDamageSource
{
    [Header("Damage")]
    [Range(0, 50)]
    [SerializeField] private int _baseDamage = 1;
    [SerializeField] private DamageType _damageType = DamageType.Piercing;
    [SerializeField] private bool _targetedAttack = true; // If true, will only destroy itself on collision with target

    [Header("Splash")]
    [SerializeField] private bool _dealsSplashDamage = false;
    [Range(0.0f, 5.0f)]
    [SerializeField] private float _splashRadius = 0.0f;
    [SerializeField] private AnimationCurve _splashDamageFalloff;

    [Header("References")]
    [SerializeField] private Rigidbody2D _rb;
    [SerializeField] private Targetable _target;
    [SerializeField] private GameObject _explosionPrefab;
    private World _world;

    [Header("Movement")]
    [SerializeField] private float _speed = 15;
    [SerializeField] private float _rotateSpeed = 95;

    [Header("Prediction")]
    [SerializeField] private float _maxDistancePredict = 100;
    [SerializeField] private float _minDistancePredict = 5;
    [SerializeField] private float _maxTimePrediction = 5;
    private Vector3 _standardPrediction, _deviatedPrediction, _previousTargetPosition;

    [Header("Deviation")]
    [SerializeField] private float _deviationAmount = 50;
    [SerializeField] private float _deviationSpeed = 2;

    // Lifespan
    private float _remainingTargetlessLife = 5.0f;

    void FindTarget()
    {
        if (_target != null)
        {
            return;
        }

        if (_world == null)
        {
            _world = FindObjectOfType<World>();
        }

        var units = _world.Units;

        if (units.Count == 0)
        {
            return;
        }

        _target = units.First().GetComponent<Targetable>();
        _previousTargetPosition = _target.transform.position;
    }

    public void SetTarget(Targetable target)
    {
        if (target == null)
        {
            return;
        }
        _target = target;
        _previousTargetPosition = _target.transform.position;
    }

    private void FixedUpdate()
    {
        if (_target == null)
        {
            _remainingTargetlessLife -= Time.fixedDeltaTime;
            if (_remainingTargetlessLife < 0.0f)
            {
                Destroy(gameObject);
            }
            return;
        }

        _rb.velocity = transform.up * _speed;

        var leadTimePercentage = Mathf.InverseLerp(_minDistancePredict, _maxDistancePredict, Vector3.Distance(transform.position, _target.transform.position));

        PredictMovement(leadTimePercentage);

        AddDeviation(leadTimePercentage);

        RotateRocket();
    }

    private void PredictMovement(float leadTimePercentage)
    {
        var predictionTime = Mathf.Lerp(0, _maxTimePrediction, leadTimePercentage);

        var estimatedVelocity = (_target.transform.position - _previousTargetPosition) / Time.deltaTime;
        _previousTargetPosition = _target.transform.position;
        _standardPrediction = _target.transform.position + estimatedVelocity * predictionTime;
        //_standardPrediction = _target.transform.position;
    }

    private void AddDeviation(float leadTimePercentage)
    {
        var deviation = new Vector3(Mathf.Cos(Time.time * _deviationSpeed), 0, 0);

        var predictionOffset = transform.TransformDirection(deviation) * _deviationAmount * leadTimePercentage;

        _deviatedPrediction = _standardPrediction + predictionOffset;
        //_deviatedPrediction = _standardPrediction;
    }

    private void RotateRocket()
    {
        var heading = _deviatedPrediction - transform.position;

        var rotation = Quaternion.LookRotation(Vector3.forward, heading);
        _rb.MoveRotation(Quaternion.RotateTowards(transform.rotation, rotation, _rotateSpeed * Time.deltaTime));
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_targetedAttack)
        {
            if (_target != null && collision.gameObject != _target.gameObject)
            {
                return;
            }
        }

        if (_explosionPrefab) Instantiate(_explosionPrefab, transform.position, Quaternion.identity);
        if (collision.transform.TryGetComponent<IDamageable>(out var d)) d.Damage(damageSource: this);

        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        if (_target == null)
        {
            return;
        }

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, _target.transform.position);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(_target.transform.position, _standardPrediction);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(_standardPrediction, _deviatedPrediction);
    }

    public DamageType GetDamageType()
    {
        return _damageType;
    }

    public int GetDamageAmount()
    {
        return _baseDamage;
    }
}