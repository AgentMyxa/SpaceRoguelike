using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Ship))]
public class ShipWeaponsController : MonoBehaviour {
    private Ship _ship;

    private List<WeaponPart> _weaponParts = new();

    private void Awake() {
        _ship = GetComponent<Ship>();
        _ship.OnStateChanged += Ship_OnStateChanged;
        _ship.OnPartAttached += Ship_OnPartAttached;
    }

    private void Ship_OnPartAttached(Part part) {

    }

    private void Ship_OnStateChanged(Ship.ShipState state) {
        if (state == Ship.ShipState.Building) {
            _weaponParts.Clear();
        }
        else if (state == Ship.ShipState.Flying) {
            _weaponParts = _ship.GetParts<WeaponPart>();
        }
    }

    public void Attack() {
        if (_ship.State != Ship.ShipState.Flying) {
            return;
        }
        foreach (var weapon in _weaponParts) {
            weapon.Shoot();
        }
    }
}
