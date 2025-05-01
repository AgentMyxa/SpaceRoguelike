using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour {
    [SerializeField] private float _speed;
    [SerializeField] private float _maxDistance;
    [SerializeField] private float _explosionRadius;
    [SerializeField] private int _damage;
    [SerializeField] private float _force;
    private float _travelledDistance;
    private Rigidbody2D _rb;
    private List<Transform> _ignoreList;

    private void Awake() {
        _rb = GetComponent<Rigidbody2D>();
        _ignoreList = new();
    }

    private void FixedUpdate() {
        _rb.linearVelocity = transform.up * _speed;
        _travelledDistance += _speed * Time.deltaTime;
        if (_travelledDistance >= _maxDistance) {
            Explode();
        }
    }

    private void OnTriggerEnter2D(Collider2D collider) {
        if (_ignoreList.Contains(collider.transform)) {
            return;
        }
        Explode();
    }

    public void Ignore(Transform transform) {
        _ignoreList.Add(transform);
    }

    public void Explode() {
        var colliders = Physics2D.OverlapCircleAll(transform.position, _explosionRadius);
        foreach (var collider in colliders) {
            if (collider.TryGetComponent(out IDamageable damageable)) {
                float damageMult = Mathf.Clamp(1 - (collider.transform.position - transform.position).magnitude / _maxDistance, .2f, 1);
                damageable.TakeDamage(Mathf.RoundToInt(_damage * damageMult));
                damageable.TakeForce(transform.position, _force * damageMult);
            }
        }
        Destroy(gameObject);
    }
}
