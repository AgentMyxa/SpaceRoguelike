using System.Collections;
using UnityEngine;

public class WeaponPart : Part {
    [SerializeField] private float _arcAngle;
    [SerializeField] private Vector2 _direction;
    [SerializeField] private Vector2 _shootPosition;
    [SerializeField] private float _gunRotationSpeed;
    [SerializeField] private Projectile _projectile;
    [SerializeField] private float _reloadTime;
    private Vector2 _targetDirection;
    private Vector2 _currentDirection;
    private Quaternion _worldRot;
    private WeaponState _weaponState;

    public enum WeaponState {
        Ready,
        Reload
    }

    private void Start() {
        _weaponState = WeaponState.Ready;
    }

    private void Update() {
        if (State != PartState.Attached || Ship.State != Ship.ShipState.Flying) {
            return;
        }
        HandleRotation();
    }

    private void HandleRotation() {
        _worldRot = Rotation * transform.rotation;
        Vector2 rotatedDirection = _worldRot * _direction;
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 dirToMouse = (Vector3)mousePos - transform.TransformPoint((Vector3)Center + _worldRot * _shootPosition);
        float dirAngle = Vector3.Angle(dirToMouse, rotatedDirection) * Mathf.Sign(Vector3.Cross(rotatedDirection, dirToMouse).z);
        dirAngle = Mathf.Clamp(dirAngle, -_arcAngle, _arcAngle);
        _targetDirection = Quaternion.Euler(0, 0, dirAngle) * rotatedDirection;
        _currentDirection = (Vector2)Vector3.RotateTowards(_currentDirection, _targetDirection, Mathf.Deg2Rad * _gunRotationSpeed * Time.deltaTime, 1);
        float curAngle = Vector3.Angle(_currentDirection, rotatedDirection);
        if (curAngle > _arcAngle) {
            _currentDirection = Quaternion.Euler(0, 0, Mathf.Sign(Vector3.Cross(_currentDirection, _targetDirection).z) * (curAngle - _arcAngle)) * _currentDirection;
        }
    }

    public void ResetRotation() {
        _worldRot = Rotation * transform.rotation;
        _targetDirection = _worldRot * _direction;
        _currentDirection = _worldRot * _direction;
    }

    public void Shoot() {
        if (_weaponState != WeaponState.Ready) {
            return;
        }
        if (Vector3.Angle(_currentDirection, _targetDirection) < 5f) {
            var instance = Instantiate(_projectile, transform.TransformPoint((Vector3)Center + Rotation * _shootPosition), Quaternion.identity);
            instance.transform.up = _currentDirection;
            instance.Ignore(transform);
            StartCoroutine(Reload());
        }
    }

    private IEnumerator Reload() {
        _weaponState = WeaponState.Reload;
        yield return new WaitForSeconds(_reloadTime);
        _weaponState = WeaponState.Ready;
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Vector3 rayPos = transform.TransformPoint((Vector3)Center + Rotation * _shootPosition);
        Gizmos.DrawRay(rayPos, _worldRot * Quaternion.Euler(0, 0, _arcAngle) * _direction);
        Gizmos.DrawRay(rayPos, _worldRot * Quaternion.Euler(0, 0, -_arcAngle) * _direction);
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(rayPos, _worldRot * _direction);
        Gizmos.color = Color.green;
        Gizmos.DrawRay(rayPos, _currentDirection);
    }
}
