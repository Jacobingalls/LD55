using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static WaveManager.Wave.Subwave;
using static WaveManager.Wave;

public class WaveManager : MonoBehaviour
{
    [System.Serializable]
    public class Wave
    {
        [System.Serializable]
        public class WavePath
        {
            public List<Vector2Int> Path;
            public float Probability;
        }

        [System.Serializable]
        public class Subwave
        {
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

            private void SpawnUnit(UnitSpawnConfig unitSpawnConfig, List<Vector2Int> path)
            {
                var go = Instantiate(unitSpawnConfig.Prefab);
                var unit = go.GetComponent<Unit>();
                unit.Move(path);

                unitSpawnConfig.UnitsLeftToSpawn -= 1;
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
                        Debug.Log("Returning for index " + _activeUnitConfigIndex);
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

                Debug.Log("Returning null");
                return null;
            }

            public IEnumerator Spawn()
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
                        SpawnUnit(activeUnitConfig, WavePaths[i].Path);
                        t -= TimeBetweenUnits;
                        i = (i + 1) % WavePaths.Count;

                        activeUnitConfig = SelectNextActiveUnitConfig();
                    }

                    yield return null;
                }
            }
        }

        public List<Subwave> Subwaves;

        public Wave()
        {
            Subwaves = new List<Subwave> { new() };
        }

        public IEnumerator Spawn(List<WavePath> wavePaths)
        {
            foreach (var subwave in Subwaves)
            {
                subwave.WavePaths = wavePaths;
                yield return subwave.Spawn();

                var t = 0.0f;
                while (t < subwave.TimeToNextSubwave)
                {
                    t += Time.deltaTime;
                    yield return null;
                }
            }
        }
    }

    public List<Wave> Waves = new List<Wave> { new() };

    private GridManager _gridManager;

    private List<WavePath> _wavePaths = new();

    // Start is called before the first frame update
    void Start()
    {
        _gridManager = FindObjectOfType<GridManager>();

        var config = new GridRangeIndicator.Configuration();
        config.range = 999;

        var startingWaypoints = _gridManager.GetStartingWaypoints();
        foreach (var startingWaypoint in startingWaypoints)
        {
            var unitPaths = _gridManager.CalculateWaypointPaths(startingWaypoint);
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

    private const float secondsBetweenUnitWaves = 2.5f;
    private float _t = secondsBetweenUnitWaves;

    private bool __temp_spawnedWave = false;
    private void Update()
    {
        _t += Time.deltaTime;


        if (_t > secondsBetweenUnitWaves && !__temp_spawnedWave)
        {
            var firstWave = Waves.First();
            StartCoroutine(firstWave.Spawn(_wavePaths));
            _t -= secondsBetweenUnitWaves;
            __temp_spawnedWave = true;
        }
    }


}
