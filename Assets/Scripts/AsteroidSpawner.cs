using System.Collections.Generic;
using UnityEngine;

public class AsteroidSpawner : MonoBehaviour {
    [SerializeField] private Asteroid _prefab;
    [SerializeField] int _asteroidMaxCount;
    [SerializeField] private float _minSpawnDistance;
    [SerializeField] private float _maxSpawnDistance;

    private List<Asteroid> _asteroids;

    public static AsteroidSpawner Instance { get; private set; }

    private void Awake() {
        Instance = this;
        _asteroids = new(_asteroidMaxCount);
        for (int i = 0; i < _asteroidMaxCount; i++) {
            _asteroids.Add ( Instantiate(_prefab, Vector3.zero, Quaternion.identity));
            _asteroids[i].transform.SetParent(transform);
            _asteroids[i].SelfDestroy();
        }
    }

    public void Spawn() {
        for (int i = 0; i < _asteroidMaxCount; i++) {
            Vector2 r = Quaternion.Euler(0, 0, Random.Range(0f, 360f)) * Vector2.up * Random.Range(_minSpawnDistance, _maxSpawnDistance);
            _asteroids[i].transform.position = r;
            _asteroids[i].Revive();
        }
    }

    public void DespawnAll() {
        for (int i = 0; i < _asteroidMaxCount; i++) {
            _asteroids[i].SelfDestroy();
        }
    }

    private void OnDisable() {
        if(Instance == this) {
            Instance = null;
        }
    }
}
