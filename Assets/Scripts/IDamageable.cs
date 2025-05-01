using UnityEngine;

public interface IDamageable {
    public void TakeDamage(int damage);
    public void TakeForce(Vector2 postion, float force);
    public void ImpactDamage(Collision2D collision);
}
