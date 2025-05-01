using UnityEngine;

public class AsteroidCollectorPart : Part {
    [SerializeField] private Vector2 _collectorPosition;
    [SerializeField] private int _collectorRadius;
    [SerializeField] private LayerMask _asteroidLayerMask;

    private void FixedUpdate() {
        var hit = Physics2D.OverlapCircle(transform.TransformPoint((Vector3)Center + Rotation * _collectorPosition), _collectorRadius, _asteroidLayerMask);
        if (hit) {
            if (hit.TryGetComponent(out Asteroid asteroid)) {
                Ship.Storage.AddAsteroid(asteroid.AsteroidType, asteroid.Amount);
                asteroid.SelfDestroy();
            }
        }
    }

    private void OnDrawGizmos() {
        Gizmos.DrawWireSphere(transform.TransformPoint((Vector3)Center + Rotation * _collectorPosition), _collectorRadius);
    }
}
