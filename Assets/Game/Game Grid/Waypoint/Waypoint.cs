using info.jacobingalls.jamkit;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEngine;

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

    public GameLevel GameLevel;

    public List<NextWaypoint> Next;

    void Awake()
    {
        if (GameLevel == null)
        {
            // PANIK, try to find one
            GameLevel = transform.GetComponentInParent<GameLevel>();
        }

        GameLevel.GridManager.RegisterWaypoint(this, new Vector2Int(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y)));

        var visualization = GetComponentInChildren<SpriteRenderer>();
        visualization.enabled = false;
    }

    private void OnDestroy()
    {
        GameLevel.GridManager.UnregisterWaypoint(this);
    }

    public override string ToString()
    {
        //return string.Format("<Waypoint {0}>", GameLevel.GridManager.PositionForWaypoint(this));
        return "Waypoint";
    }
}
