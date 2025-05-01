using UnityEngine;

public class Player : MonoBehaviour {
    [SerializeField] private Ship _ship;
    private ShipEnginesController _shipEnginesController;
    private ShipBuilder _shipBuilder;
    [SerializeField] private LayerMask _shipBuilderLayerMask;

    private void Awake() {
        _shipEnginesController = _ship.GetComponent<ShipEnginesController>();
    }

    private void Start() {
        PerformDocking();
    }

    private void Update() {
        Docking();
        Movement();
        Shooting();
    }

    private void Docking() {
        if (Input.GetKeyDown(KeyCode.B)) {
            PerformDocking();
        }
        if (Input.GetKeyDown(KeyCode.U)) {
            _shipBuilder.UndockShip();
        }
    }

    private void PerformDocking() {
        float additionalDockDistance = 1;
        var hit = Physics2D.OverlapCircle(_ship.transform.TransformPoint(_ship.GetCenterOfMass()), 
            _ship.GetHalfExtents().magnitude + additionalDockDistance, 
            _shipBuilderLayerMask);
        if (hit) {
            if (hit.TryGetComponent(out ShipBuilder shipBuilder)) {
                _shipBuilder = shipBuilder;
                _shipBuilder.DockShip(_ship);
            }
        }
    }

    private void Movement() {
        _shipEnginesController.DesiredLinearAcceleration = Vector2.zero;
        if (Input.GetKey(KeyCode.W)) {
            _shipEnginesController.DesiredLinearAcceleration += Vector2.up;
        }
        if (Input.GetKey(KeyCode.A)) {
            _shipEnginesController.DesiredLinearAcceleration += Vector2.left;
        }
        if (Input.GetKey(KeyCode.S)) {
            _shipEnginesController.DesiredLinearAcceleration += Vector2.down;
        }
        if (Input.GetKey(KeyCode.D)) {
            _shipEnginesController.DesiredLinearAcceleration += Vector2.right;
        }

        if (Input.GetKey(KeyCode.Q)) {
            _shipEnginesController.DesiredAngularAcceleration = 1;
        }
        else if (Input.GetKey(KeyCode.E)) {
            _shipEnginesController.DesiredAngularAcceleration = -1;
        }
        else {
            _shipEnginesController.DesiredAngularAcceleration = 0;
        }
    }

    private void Shooting() {
        if(Input.GetMouseButtonDown(0)) {
            _ship.WeaponsController.Attack();
        }
    }

    private void OnDrawGizmos() {
        if (_ship == null) {
            return;
        }
        Gizmos.DrawWireSphere(_ship.transform.TransformPoint(_ship.GetCenterOfMass()), _ship.GetHalfExtents().magnitude);
    }
}
