using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    [SerializeField]
    GameObject GridRangeIndicatorPrefab;

    bool VisualizeUnitPaths = false;

    GridManager _gridManager;

    // Start is called before the first frame update
    void Start()
    {
        _gridManager = FindObjectOfType<GridManager>();

        if (VisualizeUnitPaths)
        {
            var config = new GridRangeIndicator.Configuration();
            config.range = 999;

            var startingWaypoints = _gridManager.GetStartingWaypoints();
            foreach (var startingWaypoint in startingWaypoints)
            {
                var waypointPaths = _gridManager.CalculateWaypointPaths(startingWaypoint);
                foreach (var path in waypointPaths)
                {
                    var go = Instantiate(GridRangeIndicatorPrefab);
                    var gri = go.GetComponent<GridRangeIndicator>();
                    gri.VisualizePath(path, config, _gridManager);
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Debug.Log(_gridManager.ToWorldPositionTileCoordinate(mousePos));
        }
    }
}
