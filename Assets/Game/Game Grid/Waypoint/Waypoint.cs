using info.jacobingalls.jamkit;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

#nullable enable

public enum WaypointDirection
{
    Unspecified,
    North,
    East,
    South,
    West
}

public class Waypoint : MonoBehaviour
{
    [System.Serializable]
    public class NextWaypoint
    {
        public Waypoint Waypoint;
        public WaypointDirection Direction;
        [Range(0, 100)]
        public int Weight = 50;
    }

    public GridManager GridManager;

    public List<NextWaypoint> Next;

    void Awake()
    {
        if (GridManager == null)
        {
            // PANIK, try to find one
            GridManager = GameObject.FindObjectOfType<GridManager>();
        }

        GridManager.RegisterWaypoint(this, new Vector2Int(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y)));

        var visualization = GetComponentInChildren<SpriteRenderer>();
        visualization.enabled = false;
    }

    private void OnDestroy()
    {
        GridManager.UnregisterWaypoint(this);
    }

    public override string ToString()
    {
        return string.Format("<Waypoint {0}>", GridManager.PositionForWaypoint(this));
    }
}
#nullable disable