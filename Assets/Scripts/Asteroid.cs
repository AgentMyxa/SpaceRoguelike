using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class Asteroid : MonoBehaviour, IDamageable {
    [SerializeField] private AsteroidType _asteroidType;
    [SerializeField] private int _amount;
    private Rigidbody2D _rb;

    private CircleCollider2D _collider;
    private bool _isAlive = false;

    public AsteroidType AsteroidType => _asteroidType;
    public int Amount => _amount;

    private void Awake() {
        _rb = GetComponent<Rigidbody2D>();
        _collider = GetComponent<CircleCollider2D>();
    }

    public void Revive() {
        _isAlive = true;
        _collider.enabled = true;
    }

    public void SelfDestroy() {
        _isAlive = false;
        _collider.enabled = false;
    }

    private void OnDrawGizmos() {
        if (_isAlive) {
            Gizmos.DrawWireSphere(transform.position, _collider.radius);
        }
    }

    public void TakeDamage(int damage) {

    }

    public void TakeForce(Vector2 postion, float force) {
        _rb.AddForceAtPosition(((Vector2)transform.position - postion).normalized * force, postion);
    }

    public void ImpactDamage(Collision2D collision) {

    }
}
