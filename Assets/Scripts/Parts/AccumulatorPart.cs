using UnityEngine;

public class AccumulatorPart : Part
{
    [SerializeField] private int _capacity;
    public int Capacity => _capacity;
    private float _charge;
    public float Charge => _charge;

    private void Start() {
        _charge = _capacity;
    }

    public float GetEnergy(float request) {
        _charge -= request;
        if(_charge >= 0) {
            return request;
        }
        return 0;
    }

    public void AddEnergy(float amount) {
        _charge += amount;
    }
}
