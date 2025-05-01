using UnityEngine;

public class Connector : MonoBehaviour {
    private Part _parrentPart;

    [SerializeField] private Vector2Int _direction;
    public Vector2Int Direction {
        get {
            return _direction;
        }
        set {
            _direction = value;
        }
    }

    private Connector _other;
    public Connector Other => _other;

    public Vector2Int PartRelativePosition {
        set {
            transform.localPosition = new(value.x, value.y);
        }
        get {
            return new(Mathf.RoundToInt(transform.localPosition.x), Mathf.RoundToInt(transform.localPosition.y));
        }
    }

    public Vector2Int ShipRelativePosition {
        set {
            transform.position = new(value.x, value.y);
        }
        get {
            return PartRelativePosition + _parrentPart.Position;
        }
    }

    private void Awake() {
        _parrentPart = GetComponentInParent<Part>();
    }

    private void OnValidate() {
        _direction.x = Mathf.Clamp(_direction.x, -1, 1);
        _direction.y = Mathf.Clamp(_direction.y, -1, 1);
    }

    public void Connect(Connector connector) {
        if (_other == connector) {
            return;
        }
        if (this != connector) {
            _other = connector;
            _other.Connect(this);
        }
    }

    public void Disconnect() {
        if (_other != null) {
            _other._other = null;
            _other = null;
        }
    }

    private void OnDrawGizmos() {
        if (_other == null) {
            Gizmos.DrawLine(transform.position, transform.position + transform.rotation * new Vector3(_direction.x, _direction.y));
        }
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + transform.rotation * new Vector3(_direction.x, _direction.y));
    }
}
