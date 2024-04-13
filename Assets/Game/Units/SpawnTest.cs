using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SpawnTest : MonoBehaviour
{
    public GameObject UnitPrefab;
    
    GridManager _gridManager;

    private List<List<Vector2Int>> _unitPaths;

    // Start is called before the first frame update
    void Start()
    {
        _gridManager = FindObjectOfType<GridManager>();

        var config = new GridRangeIndicator.Configuration();
        config.range = 999;

        var startingWaypoints = _gridManager.GetStartingWaypoints();
        foreach (var startingWaypoint in startingWaypoints)
        {
            _unitPaths = _gridManager.CalculateWaypointPaths(startingWaypoint);
        }
    }

    private const float secondsBetweenUnitWaves = 2.5f;
    private float _t = secondsBetweenUnitWaves;

    private void Update()
    {
        _t += Time.deltaTime;

        if (_t > secondsBetweenUnitWaves)
        {
            StartCoroutine(SpawnWave());
            _t -= secondsBetweenUnitWaves;
        }
    }

    IEnumerator SpawnWave()
    {
        var t = 0.0f;
        var i = 0;
        const float secondsBetweenUnitSpawns = 0.35f;

        while (i < _unitPaths.Count)
        {
            t += Time.deltaTime;

            if (t > secondsBetweenUnitSpawns)
            {
                var path = _unitPaths[i];
                var go = Instantiate(UnitPrefab);
                var unit = go.GetComponent<Unit>();
                unit.Move(path);
                t -= secondsBetweenUnitSpawns;
                i += 1;
            }

            yield return null;
        }
    }
}
