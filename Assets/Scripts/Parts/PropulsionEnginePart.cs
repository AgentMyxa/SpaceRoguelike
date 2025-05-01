using UnityEngine;

public class PropulsionEnginePart : Part {
    [SerializeField] private float _force;
    public Vector2 Force => transform.up * _force;
}
