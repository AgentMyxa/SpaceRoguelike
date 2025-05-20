using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Part : MonoBehaviour, IDamageable {
    [Header("Part info")]
    [SerializeField] private Vector2Int _size;
    public Vector2Int Size => _size;

    [SerializeField] private Vector2Int _position;
    public Vector2Int Position => _position;
    [SerializeField] private Quaternion _rotation;
    public Quaternion Rotation => _rotation;
    public Vector2 Center => (Vector2)Size / 2 - Vector2.one * .5f;

    private List<Connector> _connectors = new();
    public IReadOnlyList<Connector> Connectors => _connectors;

    [SerializeField] private int _maxHealth;
    private int _health;

    [SerializeField] private SpriteRenderer _spriteRenderer;
    private Color _defaultColor;

    public Ship Ship { get; set; }
    private BoxCollider2D _collider;

    public enum PartState {
        Attached,
        Detached,
        Placing
    }

    private PartState _state;
    public PartState State {
        get {
            return _state;
        }
        set {
            _state = value;
            _spriteRenderer.color = _state switch {
                PartState.Detached => Color.HSVToRGB(.6f, 1, .75f),
                PartState.Placing => Color.HSVToRGB(.3f, 1, .75f),
                _ => _defaultColor
            };
        }
    }

    private void OnValidate() {
        if (_collider == null) {
            _collider = GetComponent<BoxCollider2D>();
        }
        _collider.size = Size;
        _collider.offset = Center;
        _spriteRenderer.transform.localPosition = Center;
        _spriteRenderer.transform.localScale = new(Size.x - .2f, Size.y - .2f, 1);
    }

    private void Awake() {
        _rotation = Quaternion.identity;
        foreach (Connector connector in transform.GetComponentsInChildren<Connector>()) {
            _connectors.Add(connector);
        }
        _spriteRenderer.transform.localPosition = Center;
        _spriteRenderer.transform.localScale = new(Size.x - .2f, Size.y - .2f, 1);
        _defaultColor = _spriteRenderer.color;
        _collider = GetComponent<BoxCollider2D>();
        _collider.size = Size;
        _collider.offset = Center;
        _health = _maxHealth;
    }

    public void TakeDamage(int damage) {
        _health -= damage;
        if (_health <= 0) {
            Ship.DeletePart(this);
        }
    }

    public void TakeForce(Vector2 postion, float force) {
        if (Ship == null) {
            return;
        }
        Ship.Rigidbody.AddForceAtPosition(((Vector2)transform.position - postion).normalized * force, postion);
    }

    public void ImpactDamage(Collision2D collision) {

    }

    public void SetPosition(Vector2Int position, Vector2Int shipPosition) {
        transform.position = new(position.x, position.y);
        _position = position;
        _position -= shipPosition;
    }

    public void SetRotation(int zMult) {
        zMult = Mathf.Clamp(zMult, 0, 3);
        Quaternion newRotation = Quaternion.Euler(0, 0, 90 * zMult);
        Quaternion fromToRotation = Quaternion.Inverse(Rotation) * newRotation;
        var angle = Mathf.DeltaAngle(Rotation.eulerAngles.z, newRotation.eulerAngles.z);
        var prevCenter = Center;
        if (Mathf.Abs(angle) - 90 == 0) {
            _size = new(Size.y, Size.x);
            _collider.size = Size;
            _collider.offset = Center;
            _spriteRenderer.transform.localPosition = Center;
            _spriteRenderer.transform.localScale = new(Size.x - .2f, Size.y - .2f, 1);
        }
        foreach (var connector in _connectors) {
            Vector3 pos = fromToRotation * ((Vector2)connector.PartRelativePosition - prevCenter) + (Vector3)Center;
            var dir = fromToRotation * (Vector2)connector.Direction;
            connector.PartRelativePosition = new(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y));
            connector.Direction = new(Mathf.RoundToInt(dir.x), Mathf.RoundToInt(dir.y));
        }
        _rotation = newRotation;
    }

    public void RotateClockwise90() {
        SetRotation((Mathf.RoundToInt(Rotation.eulerAngles.z / 90) - 1 + 4) % 4);
    }

    public void RotateCounterclockwise90() {
        SetRotation((Mathf.RoundToInt(Rotation.eulerAngles.z / 90) + 1 + 4) % 4);
    }

    public void ClearConnections() {
        foreach (var connector in _connectors) {
            connector.Disconnect();
        }
    }

    public void MirrorX() {
        // TODO:
    }
    public void MirrorY() {
        // TODO:
    }
}
