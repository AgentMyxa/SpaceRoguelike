using System.Collections.Generic;
using UnityEngine;

public class ShuntingEnginePart : Part {
    [SerializeField] private List<Vector2> _thrustDirections;
    public List<Vector2> ThrustDirections => _thrustDirections;
}
