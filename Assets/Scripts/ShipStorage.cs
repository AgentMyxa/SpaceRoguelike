using System.Collections.Generic;
using UnityEngine;

public class ShipStorage : MonoBehaviour {
    Dictionary<AsteroidType, int> _asteroidStorage = new();

    public void AddAsteroid(AsteroidType asteroid, int amount) {
        _asteroidStorage[asteroid] = _asteroidStorage.GetValueOrDefault(asteroid, 0) + amount;
    }
    
    public int GetAsteroidCount(AsteroidType asteroid) {
        if (_asteroidStorage.TryGetValue(asteroid, out int value)){
            return value;
        }
        return 0;
    }

    public bool TryRemoveAsteroid(AsteroidType asteroid, int amount) {
        if (_asteroidStorage.TryGetValue(asteroid, out int value)){
            if(value <= amount) {
                _asteroidStorage[asteroid] -= amount;
            return true;
            }
        }
        return false;
    }
}
