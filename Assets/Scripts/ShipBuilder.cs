using System.Collections.Generic;
using UnityEngine;

public class ShipBuilder : MonoBehaviour {
    [SerializeField] private Ship _ship;
    [SerializeField] private List<Part> _prefabs;
    [Header("Building area corners")]
    [SerializeField] private Vector2Int _BottomLeft;
    [SerializeField] private Vector2Int _UpRight;
    private Part _instancedPart;
    private Part _selectedPart;
    private int _placedPartAngle;

    public enum BuildState {
        None,
        PartPlacing
    }

    public void DockShip(Ship ship) {
        ship.State = Ship.ShipState.Building;
        ship.transform.SetPositionAndRotation(transform.position, Quaternion.identity);
        _ship = ship;
        AsteroidSpawner.Instance.DespawnAll();
    }

    public void UndockShip() {
        if (_ship != null) {
            if (_instancedPart != null) {
                Destroy(_instancedPart.gameObject);
                _instancedPart = null;
            }
            _ship.State = Ship.ShipState.Flying;
            _ship = null;
        }
        AsteroidSpawner.Instance.Spawn();
    }

    private void Update() {
        if (_ship == null) {
            return;
        }

        Vector2 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2Int mousePosShipCoords = new(Mathf.RoundToInt(mouse.x), Mathf.RoundToInt(mouse.y));

        for (int i = 0; i < _prefabs.Count; i++) {
            if (Input.GetKeyDown((i + 1).ToString())) {
                if (_instancedPart != null) {
                    Destroy(_instancedPart.gameObject);
                }

                _selectedPart = _prefabs[i];
                _instancedPart = Instantiate(_selectedPart);
                _instancedPart.State = Part.PartState.Placing;
            }
        }

        if (_instancedPart == null) {
            if (Input.GetMouseButtonDown(0)) {
                var res = _ship.FindPartByPoint(mousePosShipCoords - _ship.WorldPosition);
                if (res != null) {
                    if (_ship.DetachPart(res)) {
                        _selectedPart = null;
                        _instancedPart = res;
                        _instancedPart.State = Part.PartState.Placing;
                        _ship.ReattachPartsFromCore();
                    }
                }
            }

            if(Input.GetMouseButtonDown(1)) {
                var res = _ship.FindPartByPoint(mousePosShipCoords - _ship.WorldPosition);
                if (res != null) {
                    _ship.DeletePart(res);
                }
            }
            return;
        }

        if (_instancedPart != null) {
            foreach (int i in new int[] { 0, 1, 2, 3 }) {
                if (Input.GetKeyDown((i + 6).ToString())) {
                    _instancedPart.SetRotation(i);
                }
            }

            if (Input.GetMouseButtonDown(1)) {
                Destroy(_instancedPart.gameObject);
                _instancedPart = null;
                return;
            }

            if (Input.GetMouseButtonDown(2)) {
                if (Input.GetKey(KeyCode.LeftShift)) {
                    _instancedPart.RotateCounterclockwise90();
                }
                else {
                    _instancedPart.RotateClockwise90();
                }
            }

            int mousePosX = Mathf.RoundToInt(mouse.x);
            int mousePosY = Mathf.RoundToInt(mouse.y);
            mousePosX = Mathf.Clamp(mousePosX, _BottomLeft.x, _UpRight.x - _instancedPart.Size.x);
            mousePosY = Mathf.Clamp(mousePosY, _BottomLeft.y, _UpRight.y - _instancedPart.Size.y);

            _instancedPart.SetPosition(new(mousePosX, mousePosY), _ship.WorldPosition);

            if (Input.GetMouseButtonDown(0)) {
                if (CheckIntersections(_instancedPart)) {
                    _ship.RegisterPart(_instancedPart);
                    _instancedPart.State = Part.PartState.Detached;
                    _placedPartAngle = Mathf.RoundToInt(_instancedPart.Rotation.eulerAngles.z);
                    if (_selectedPart != null) {
                        _instancedPart = Instantiate(_selectedPart);
                        _instancedPart.State = Part.PartState.Placing;
                        _instancedPart.SetRotation(_placedPartAngle / 90);
                    }
                    else {
                        _instancedPart = null;
                    }
                    _ship.ReattachPartsFromCore();
                }
            }
        }
    }

    private bool CheckIntersections(Part part) {
        for (int x = 0; x < part.Size.x; x++) {
            for (int y = 0; y < part.Size.y; y++) {
                if (_ship.FindPartByPoint(new(part.Position.x + x, part.Position.y + y)) != null) {
                    return false;
                }
            }
        }
        return true;
    }
}
