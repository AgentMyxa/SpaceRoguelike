using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;

[RequireComponent(typeof(Ship))]
public class ShipEnginesController : MonoBehaviour {
    [SerializeField] private float _maxThrusterEnergyDrain;
    [SerializeField] private float _maxThrust;

    [SerializeField] private Transform _centerOfMassPoint;
    private Ship _ship;
    private Rigidbody2D _rb;
    private List<ShuntingEnginePart> _shuntingEngines = new();

    private Vector2 _desiredLinearAcceleration;
    private float _desiredAngularAcceleration;

    public Vector2 DesiredLinearAcceleration { get => _desiredLinearAcceleration; set => _desiredLinearAcceleration = value.normalized; }
    public float DesiredAngularAcceleration { get => _desiredAngularAcceleration; set => _desiredAngularAcceleration = value; }

    private Vector<float> _calculatedThrusts;
    int _thrustersCount = 0;
    private List<Vector2> _thrustPositions = new();
    private List<Vector2> _thrustDirections = new();

    private void Awake() {
        _ship = GetComponent<Ship>();
        _rb = GetComponent<Rigidbody2D>();
        _ship.OnStateChanged += Ship_OnStateChanged;
        _ship.OnPartAttached += Ship_OnPartAttached;
    }

    private void Ship_OnPartAttached(Part part) {
        _rb.centerOfMass = _ship.GetCenterOfMass();
        _rb.mass = _ship.GetMass();
        _centerOfMassPoint.localPosition = _rb.centerOfMass;
        UpdateThrusters();
    }

    private void Ship_OnStateChanged(Ship.ShipState state) {
        if (state == Ship.ShipState.Building) {
            _rb.angularVelocity = 0;
            _rb.linearVelocity = Vector2.zero;
        }
        else if (state == Ship.ShipState.Flying) {
            UpdateThrusters();
        }
    }

    public void UpdateThrusters() {
        _shuntingEngines = _ship.GetShuntingEngineParts();
        _thrustDirections.Clear();
        _thrustPositions.Clear();
        _thrustersCount = 0;
        foreach (var e in _shuntingEngines) {
            foreach (var t in e.ThrustDirections) {
                _thrustPositions.Add(e.Position);
                _thrustDirections.Add(e.Rotation * (-t));
                _thrustersCount++;
            }
        }
    }

    public void ApplyMovement(Vector2 desiredLinearAcceleration, float desiredAngularAcceleration) {
        if (_shuntingEngines.Count == 0) return;
        Vector2 desiredForce;
        float desiredTorque;
        float forceBoost = 3;

        if(Mathf.Approximately(desiredLinearAcceleration.magnitude, 0)) {
            desiredForce = -transform.InverseTransformDirection(_rb.linearVelocity) * forceBoost;
        }
        else {
            desiredForce = _rb.mass * forceBoost * transform.InverseTransformDirection(desiredLinearAcceleration);
        }
        if (Mathf.Approximately(desiredAngularAcceleration, 0)) {
            desiredTorque = -_rb.angularVelocity * .1f;
        }
        else {
            desiredTorque = _rb.inertia * desiredAngularAcceleration;
        }

        var A = Matrix<float>.Build.Dense(3, _thrustersCount);
        for (int i = 0; i < _thrustersCount; i++) {
            Vector2 t = _thrustDirections[i];
            Vector2 r = _thrustPositions[i] - _rb.centerOfMass;
            A[0, i] = t.x;
            A[1, i] = t.y;
            A[2, i] = r.x * t.y - r.y * t.x;
        }

        var b = Vector<float>.Build.Dense(3);
        b[0] = desiredForce.x;
        b[1] = desiredForce.y;
        b[2] = desiredTorque;
        
        _calculatedThrusts = A.PseudoInverse() * b;

        for (int i = 0; i < _calculatedThrusts.Count; i++) {
            float thrust = Mathf.Clamp(_calculatedThrusts[i], 0, _maxThrust);
            var worldPos = transform.TransformPoint(_thrustPositions[i]);
            var worldDir = transform.TransformDirection(_thrustDirections[i]);
            if (_ship.EnergySystem.TryConsumeEnergy(thrust / _maxThrust * _maxThrusterEnergyDrain * Time.deltaTime)) {
                _rb.AddForceAtPosition(worldDir * thrust, worldPos);
            }
        }
    }

    private void FixedUpdate() {
        if (_ship.State == Ship.ShipState.Flying) {
            ApplyMovement(_desiredLinearAcceleration, _desiredAngularAcceleration);
        }
        else {
            _rb.linearVelocity = Vector2.zero;
            _rb.angularVelocity = 0;
        }
    }

    private void OnDrawGizmos() {
        if (Application.isPlaying) {
            Gizmos.DrawSphere(_rb.worldCenterOfMass, .2f);
            if (_calculatedThrusts != null) {
                for (int i = 0; i < _thrustersCount; i++) {
                    Gizmos.color = Color.magenta;
                    Gizmos.DrawRay(transform.TransformPoint(_thrustPositions[i]), transform.TransformDirection(_thrustDirections[i]) * -Mathf.Clamp(_calculatedThrusts[i], 0, _maxThrust));
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawRay(transform.TransformPoint(_thrustPositions[i]), transform.TransformDirection(-_thrustDirections[i]));
                }
            }
        }
    }
}
