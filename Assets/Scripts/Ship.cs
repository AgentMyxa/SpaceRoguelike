using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Ship : MonoBehaviour {
    [SerializeField] private List<Part> _parts;

    private Rigidbody2D _rb;
    public Rigidbody2D Rigidbody => _rb;

    private ShipStorage _storage;
    public ShipStorage Storage => _storage;
    private ShipEnergySystem _energySystem;
    public ShipEnergySystem EnergySystem => _energySystem;
    private ShipWeaponsController _weaponsController;
    public ShipWeaponsController WeaponsController => _weaponsController;

    public CorePart CorePart {
        get {
            foreach (Part part in _parts) {
                if (part is CorePart corePart) {
                    return corePart;
                }
            }
            return null;
        }
    }

    public enum ShipState {
        Building,
        Flying
    }

    private ShipState _state;
    public ShipState State {
        get {
            return _state;
        }
        set {
            _state = value;
            OnStateChanged?.Invoke(_state);
        }
    }

    public event Action<ShipState> OnStateChanged;
    public event Action<Part> OnPartAttached;

    public Vector2Int WorldPosition => new(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));

    private void Awake() {
        _rb = GetComponent<Rigidbody2D>();
        _storage = GetComponent<ShipStorage>();
        _energySystem = GetComponent<ShipEnergySystem>();
        _weaponsController = GetComponent<ShipWeaponsController>();
    }

    private void Start() {
        OnPartAttached?.Invoke(CorePart);
    }

    public void RegisterPart(Part part) {
        if (!_parts.Contains(part)) {
            part.transform.SetParent(transform);
            _parts.Add(part);
        }
    }

    public bool TryAttach(Part part) {
        if (part == null) {
            return false;
        }
        if (part.State == Part.PartState.Attached) {
            return false;
        }

        List<Connector> availableConnectors = new();
        foreach (var p in _parts) {
            if (p.State == Part.PartState.Attached) {
                foreach (var c in p.Connectors) {
                    if (c.Other == null) {
                        availableConnectors.Add(c);
                    }
                }
            }
        }
        if (availableConnectors.Count == 1) {
            return false;
        }
        bool connected = false;
        foreach (var other in part.Connectors) {
            foreach (var connector in availableConnectors) {
                var otherPos = other.ShipRelativePosition;
                var connectorPos = connector.ShipRelativePosition;
                if (otherPos + other.Direction == connectorPos && connector.Direction == -other.Direction) {
                    connector.Connect(other);
                    availableConnectors.Remove(connector);
                    connected = true;
                    break;
                }
            }
        }
        if (connected) {
            part.State = Part.PartState.Attached;
            part.Ship = this;
        }
        else {
            part.State = Part.PartState.Detached;
        }
        return connected;
    }

    private Part TraversingAttach(Part origin, Part lastAttachedPart) {
        foreach (var connector in origin.Connectors) {
            var nextPart = FindPartByPoint(connector.ShipRelativePosition + connector.Direction);
            if (nextPart != null) {
                if (nextPart.State == Part.PartState.Placing) {
                    continue;
                }
                if (TryAttach(nextPart)) {
                    lastAttachedPart = TraversingAttach(nextPart, lastAttachedPart);
                }
            }
        }
        return lastAttachedPart;
    }

    public void ReattachPartsFromCore() {
        foreach (var part in _parts) {
            if (part.State == Part.PartState.Attached) {
                DetachPart(part);
            }
        }
        OnPartAttached?.Invoke(TraversingAttach(CorePart, CorePart));
        if (_state == ShipState.Flying) {
            List<Part> removed = new();
            foreach (var part in _parts) {
                if (part.State == Part.PartState.Detached) {
                    removed.Add(part);
                }
            }
            foreach (var part in removed) {
                _parts.Remove(part);
                Destroy(part.gameObject);
            }
        }
    }

    public bool DetachPart(Part part) {
        if (part is CorePart || part.State == Part.PartState.Placing) {
            return false;
        }
        part.ClearConnections();
        part.State = Part.PartState.Detached;
        return true;
    }

    public void DeletePart(Part part) {
        DetachPart(part);
        _parts.Remove(part);
        Destroy(part.gameObject);
        ReattachPartsFromCore();
    }

    public Part FindPartByPoint(Vector2Int point) {
        foreach (var part in _parts) {
            if (part.State == Part.PartState.Placing) {
                continue;
            }
            if (part.Position.x <= point.x && point.x < part.Position.x + part.Size.x) {
                if (part.Position.y <= point.y && point.y < part.Position.y + part.Size.y) {
                    return part;
                }
            }
        }
        return null;
    }

    public Vector2 GetCenterOfMass() {
        float x = 0;
        float y = 0;
        int c = 0;
        foreach (var part in _parts) {
            if (part.State != Part.PartState.Attached) {
                continue;
            }
            c += part.Size.x * part.Size.y;
            for (int i = 0; i < part.Size.x; i++) {
                for (int j = 0; j < part.Size.y; j++) {
                    x += part.Position.x + i;
                    y += part.Position.y + j;
                }
            }
        }
        return new Vector2(x, y) / c;
    }

    public float GetMass() {
        float mass = 0;
        foreach (var part in _parts) {
            mass += part.Size.x * part.Size.y;
        }
        return mass;
    }

    public Vector2Int GetHalfExtents() {
        int xLeft = 0;
        int xRight = 0;
        int yUp = 0;
        int yDown = 0;
        foreach (var part in _parts) {
            if (part.State != Part.PartState.Attached) {
                continue;
            }
            xLeft = Mathf.Min(xLeft, part.Position.x);
            xRight = Mathf.Max(xRight, part.Position.x + part.Size.x);
            yDown = Mathf.Min(yDown, part.Position.y);
            yUp = Mathf.Max(yUp, part.Position.y + part.Size.y);
        }
        return new(Mathf.CeilToInt((xRight - xLeft) / 2f), Mathf.CeilToInt((yUp - yDown) / 2f));
    }

    public List<ShuntingEnginePart> GetShuntingEngineParts() {
        List<ShuntingEnginePart> parts = new();
        foreach (Part part in _parts) {
            if (part is ShuntingEnginePart enginePart) {
                if (part.State == Part.PartState.Attached) {
                    parts.Add(enginePart);
                }
            }
        }
        return parts;
    }

    public List<T> GetParts<T>() where T : Part {
        List<T> parts = new();
        foreach (Part part in _parts) {
            if (part is T t) {
                parts.Add(t);
            }
        }
        return parts;
    }

    public List<Part> GetParts() {
        return GetParts<Part>();
    }
}
