using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShipEnergySystem : MonoBehaviour {
    private Ship _ship;

    [SerializeField] private int _initialEnergy;
    private float _maxEnergy;
    private float _energy;
    private List<AccumulatorPart> _accumulators;

    [SerializeField] private Gradient _visualGradient;
    [SerializeField] private SpriteRenderer _visual;

    private void Awake() {
        _ship = GetComponent<Ship>();
        _ship.OnStateChanged += Ship_OnStateChanged;
        _ship.OnPartAttached += Ship_OnPartAttached;
    }

    private void Ship_OnPartAttached(Part obj) {
        _accumulators = _ship.GetParts<AccumulatorPart>();
        _maxEnergy = _initialEnergy;
        foreach (AccumulatorPart part in _accumulators) {
            _maxEnergy += part.Capacity;
        }
    }

    private void Ship_OnStateChanged(Ship.ShipState obj) {
        _energy = _initialEnergy;
    }

    public bool TryConsumeEnergy(float request) {
        float energy = 0;
        int availableAccumulators = 1;
        foreach (var part in _accumulators) {
            availableAccumulators += part.Charge > 0 ? 1 : 0;
        }
        _energy -= energy += request / availableAccumulators;
        foreach (var part in _accumulators) {
            energy += part.GetEnergy(request / availableAccumulators);
        }
        if (energy > 0) {
            return true;
        }
        _energy += energy / availableAccumulators;
        foreach (var part in _accumulators) {
            part.AddEnergy(energy / availableAccumulators);
        }
        return false;
    }

    private void Update() {
        _visual.color = _visualGradient.Evaluate((_energy + _accumulators.Sum((AccumulatorPart a) => a.Charge)) / _maxEnergy);
    }
}
