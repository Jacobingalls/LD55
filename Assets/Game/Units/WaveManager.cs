using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static WaveManager.Wave.Subwave;
using static WaveManager.Wave;
using info.jacobingalls.jamkit;

[RequireComponent(typeof(PubSubSender))]
[RequireComponent(typeof(PubSubListener))]
public class WaveManager : MonoBehaviour
{
    [System.Serializable]
    public class Wave
    {
        public delegate void SpawnCompletionHandler(Wave wave);

        [System.Serializable]
        public class WavePath
        {
            public List<Vector2Int> Path;
            public float Probability;
        }

        [System.Serializable]
        public class Subwave
        {
            public delegate void SpawnCompletionHandler(Subwave subwave);

            [System.Serializable]
            public class UnitSpawnConfig
            {
                public GameObject Prefab;

                [Range(0, 100)]
                public int SpawnCount = 10;

                [HideInInspector]
                public int UnitsLeftToSpawn;
            }
            public List<UnitSpawnConfig> UnitConfigs;

            [SerializeField]
            [Range(0.0f, 2.0f)]
            public float TimeBetweenUnits;

            [SerializeField]
            public float TimeToNextSubwave;

            /// <summary>
            /// IntermixUnitTypes - If true, will cycle through unit prefabs, spawning each one at a time.
            /// If false, will spawn all of the first unit type, then all of the second unit type, etc...
            /// </summary>
            public bool IntermixUnitTypes;

            [HideInInspector]
            public List<WavePath> WavePaths = new();

            public Subwave()
            {
                TimeBetweenUnits = 0.35f;
                TimeToNextSubwave = 1.0f;
                IntermixUnitTypes = false;

                UnitConfigs = new List<UnitSpawnConfig> { new() };
            }

            private void PlaySpawnAudio(Vector3 position)
            {
                AudioManager.Instance.Play("Units/Spawn",
                    pitchMin: 0.7f, pitchMax: 1.2f,
                    volumeMin: 0.45f, volumeMax: 0.55f,
                    position: position,
                    minDistance: 10, maxDistance: 20);
            }

            private void SpawnUnit(UnitSpawnConfig unitSpawnConfig, List<Vector2Int> path, Transform parentTransform)
            {
                var go = Instantiate(unitSpawnConfig.Prefab, parentTransform);
                var spawnPoint = new Vector3(path.First().x, path.First().y, 0.0f);
                go.transform.position = spawnPoint;
                var unit = go.GetComponent<Unit>();
                unit.Move(path);

                unitSpawnConfig.UnitsLeftToSpawn -= 1;

                PlaySpawnAudio(spawnPoint);
            }

            private bool UnitsLeftToSpawn()
            {
                foreach (var unitConfig in UnitConfigs)
                {
                    if (unitConfig.UnitsLeftToSpawn > 0)
                    {
                        return true;
                    }
                }
                return false;
            }

            private int _activeUnitConfigIndex = 0;
            private UnitSpawnConfig SelectNextActiveUnitConfig()
            {
                var currentActiveUnitConfig = UnitConfigs[_activeUnitConfigIndex];
                if (!IntermixUnitTypes && currentActiveUnitConfig.UnitsLeftToSpawn > 0)
                {
                    return currentActiveUnitConfig;
                }

                // Find next available unit type
                var startingIndex = _activeUnitConfigIndex;
                var currentIndex = (_activeUnitConfigIndex + 1) % UnitConfigs.Count;
                while(true)
                {
                    var candidateUnitConfig = UnitConfigs[currentIndex];
                    if (candidateUnitConfig.UnitsLeftToSpawn > 0 )
                    {
                        _activeUnitConfigIndex = currentIndex;
                        return candidateUnitConfig;
                    }

                    if (currentIndex == startingIndex)
                    {
                        break;
                    }
                    else
                    {
                        currentIndex = (currentIndex + 1) % UnitConfigs.Count;
                    }
                }

                return null;
            }

            public IEnumerator Spawn(Transform unitParentTransform, SpawnCompletionHandler spawnCompletionHandler)
            {
                var t = 0.0f;
                var i = 0;

                foreach (var unitConfig in UnitConfigs)
                {
                    unitConfig.UnitsLeftToSpawn = unitConfig.SpawnCount;
                }

                UnitSpawnConfig activeUnitConfig = UnitConfigs.First();
                while (activeUnitConfig != null)
                {
                    t += Time.deltaTime;

                    if (t > TimeBetweenUnits)
                    {
                        SpawnUnit(activeUnitConfig, WavePaths[i].Path, unitParentTransform);
                        t -= TimeBetweenUnits;
                        i = (i + 1) % WavePaths.Count;

                        activeUnitConfig = SelectNextActiveUnitConfig();
                    }

                    yield return null;
                }

                if (spawnCompletionHandler != null)
                {
                    spawnCompletionHandler(this);
                }
            }
        }

        public List<Subwave> Subwaves;

        public Wave()
        {
            Subwaves = new List<Subwave> { new() };
        }

        private bool _spawning = false;
        private int _spawnedSubwaves = 0;

        public IEnumerator Spawn(List<WavePath> wavePaths, Transform unitParentTransform, SpawnCompletionHandler spawnCompletionHandler = null)
        {
            if (_spawning) { yield break; }
            _spawning = true;
            _spawnedSubwaves = 0;
            foreach (var subwave in Subwaves)
            {
                subwave.WavePaths = wavePaths;
                yield return subwave.Spawn(unitParentTransform, (Subwave subwave) => {
                    _spawnedSubwaves += 1;

                    if (spawnCompletionHandler != null && _spawnedSubwaves == Subwaves.Count)
                    {
                        spawnCompletionHandler(this);
                    }
                });

                var t = 0.0f;
                while (t < subwave.TimeToNextSubwave)
                {
                    t += Time.deltaTime;
                    yield return null;
                }
            }
            _spawning = false;
        }

        public bool HasFinishedSpawningSubwaves()
        {
            return _spawnedSubwaves == Subwaves.Count;
        }
    }

    public List<Wave> Waves = new List<Wave> { new() };

    private GameLevel _gameLevel;

    private List<WavePath> _wavePaths = new();

    // Start is called before the first frame update
    void Start()
    {
        _gameLevel = GetComponentInParent<GameLevel>();
        _gameLevel.RegisterWaveManager(this);

        var config = new GridRangeIndicator.Configuration();
        config.range = 999;

        var startingWaypoints = _gameLevel.GridManager.GetStartingWaypoints();
        foreach (var startingWaypoint in startingWaypoints)
        {
            var unitPaths = _gameLevel.GridManager.CalculateWaypointPaths(startingWaypoint);
            Debug.Log(startingWaypoint);
            foreach (var unitPath in unitPaths)
            {
                var wavePath = new WavePath
                {
                    Path = unitPath,
                    Probability = 1.0f
                };
                _wavePaths.Add(wavePath);
            }
        }
    }

    private int _currentWaveIndex = -1;
    private bool _waveIsActive = false;
    private GameObject _currentWaveUnits = null;

    public int CurrentWave
    {
        get
        {
            return _currentWaveIndex;
        }
    }

    public void StartNextWave()
    {
        if (_currentWaveIndex == Waves.Count - 1 || _waveIsActive)
        {
            return;
        }

        _waveIsActive = true;
        _currentWaveIndex += 1;

        var wave = Waves[_currentWaveIndex];
        _currentWaveUnits = new GameObject($"Wave {_currentWaveIndex} Units");
        _currentWaveUnits.transform.parent = transform;

        Debug.Log($"Wave {_currentWaveIndex} has started.");
        StartCoroutine(wave.Spawn(_wavePaths, unitParentTransform: _currentWaveUnits.transform));

        GetComponent<PubSubSender>().Publish("wave.started", this);
    }

    public bool CurrentWaveCompleted()
    {
        return !_waveIsActive;
    }

    private void SetWaveCompleted()
    {
        Debug.Log($"Wave {_currentWaveIndex} has ended.");
        _waveIsActive = false;

        GetComponent<PubSubSender>().Publish("wave.completed", this);
    }

    public bool AllWavesCompleted()
    {
        return _currentWaveIndex == Waves.Count - 1 && _waveIsActive == false;
    }

    private void LateUpdate()
    {
        if (_currentWaveIndex == -1)
        {
            return;
        }

        var currentWave = Waves[_currentWaveIndex];

        if (currentWave.HasFinishedSpawningSubwaves() && _waveIsActive && _currentWaveUnits.transform.childCount == 0)
        {
            SetWaveCompleted();
        }
    }


}
