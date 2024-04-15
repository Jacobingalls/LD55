using info.jacobingalls.jamkit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
using static UnityEngine.EventSystems.EventTrigger;
using static UnityEngine.RuleTile.TilingRuleOutput;

public struct TileData
{
    public Vector2Int Position;

    public Summon Summon;
    public List<Waypoint> Waypoints;
    public GridManager GridManager;

    public Vector3 WorldPosition
    {
        get
        {
            return GridManager.TileCoordinateToWorldPosition(Position);
        }
    }
}

[RequireComponent(typeof(PubSubSender))]
public class GridManager : MonoBehaviour
{

    [Header("Tilemap")]
    public Tilemap Walls;
    public Tilemap Walkable;

    private const float _tileCenterOffset = 0.5f;

    private Dictionary<Vector2Int, TileData> _tileData = new();
    private Dictionary<object, Vector2Int> _objectTilePositions = new();

    public HashSet<Summon> Summons { get { return _summons; } }
    private HashSet<Summon> _summons = new();

    public HashSet<Waypoint> Waypoints { get { return _waypoints; } }
    private HashSet<Waypoint> _waypoints = new();
    private Dictionary<Waypoint, Waypoint> _waypointToPreviousMapping = new();
    private List<Waypoint> _startingWaypoints = new();
    private bool _dirtyWaypoints = false;

    public PubSubSender PubSubSender;

    [SerializeField]
    private List<TileConfig> tileConfigs;

    private Dictionary<TileBase, TileConfig> _dataFromTiles;


    private void Awake()
    {
        _dataFromTiles = new Dictionary<TileBase, TileConfig>();
        foreach (var config in tileConfigs)
        {
            foreach (var tile in config.tiles)
            {
                _dataFromTiles.Add(tile, config);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
    }

    public TileConfig GetTileConfig(Vector2Int position)
    {
        var tile = Walkable.GetTile((Vector3Int)position);
        if (tile != null)
        {
            return _dataFromTiles[tile];
        }
        else
        {
            return null;
        }
    }

    public TileData GetTileData(Vector2Int position)
    {
        if (_tileData.ContainsKey(position))
        {
            return _tileData[position];
        }
        else
        {
            var tileData = new TileData
            {
                Position = position,
                GridManager = this,
                Waypoints = new List<Waypoint>(),
            };
            return tileData;
        }
    }

    public void UpdateTileData(TileData newTileData)
    {
        if (true /* newTileData.IsEmpty() */) // Was stubbed true before, not sure how to fix.
        {
            if (_tileData.ContainsKey(newTileData.Position))
            {
                _tileData.Remove(newTileData.Position);
            }
        }
        else
        {
            _tileData[newTileData.Position] = newTileData;
        }

        if (PubSubSender != null)
        {
            PubSubSender.Publish("grid.tile.updated");
        }
    }

    public Vector2Int WorldTilePositionToGridTilePosition(Vector2Int worldTilePosition)
    {
        var result = new Vector2Int(worldTilePosition.x - Mathf.FloorToInt(transform.position.x), worldTilePosition.y - Mathf.FloorToInt(transform.position.y));

        Debug.Log("Was " + worldTilePosition);
        Debug.Log("Is now " + result);

        return result;
    }

    public Vector3Int WorldTilePositionToGridTilePosition(Vector3Int worldTilePosition)
    {
        return new Vector3Int(worldTilePosition.x - Mathf.FloorToInt(transform.position.x), worldTilePosition.y - Mathf.FloorToInt(transform.position.y), worldTilePosition.z - Mathf.FloorToInt(transform.position.z));
    }

    ////An ordered list of selectables at a given tile.
    //public List<Selectable> GetSelectables(Vector2Int position)
    //{
    //    var selectables = new List<Selectable>();

    //    var tile = GetTileData(position);

    //    // Get the Summon, if it exists
    //    if (tile.Summon != null && tile.Summon.TryGetComponent<Selectable>(out var selectable)) { selectables.Add(selectable); }

    //    return selectables;
    //}

    //public Vector2Int? PositionForSelectable(Selectable selectable)
    //{
    //    // This whole method is gross.
    //    foreach (var (pos, tile) in _tileData)
    //    {
    //        foreach (var s in tile.Selectables)
    //        {
    //            if (s == selectable) { return pos; }
    //        }
    //    }

    //    return null;
    //}

    public Vector2Int PositionForSummon(Summon Summon)
    {
        return _objectTilePositions[Summon];
    }

    public Vector2Int PositionForWaypoint(Waypoint waypoint)
    {
        return _objectTilePositions[waypoint];
    }

    public List<Waypoint> GetStartingWaypoints()
    {
        if (!_dirtyWaypoints)
        {
            return _startingWaypoints;
        }
        _dirtyWaypoints = false;

        _waypointToPreviousMapping = new Dictionary<Waypoint, Waypoint>();
        // Pass One -> get prev mappings
        foreach (Waypoint waypoint in _waypoints)
        {
            _waypointToPreviousMapping[waypoint] = null;
        }

        foreach (Waypoint waypoint in _waypoints)
        {
            foreach (Waypoint.NextWaypoint next in waypoint.Next) {
                _waypointToPreviousMapping[next.Waypoint] = waypoint;
            }
        }

        _startingWaypoints = new List<Waypoint>();
        foreach (KeyValuePair<Waypoint, Waypoint> entry in _waypointToPreviousMapping)
        {
            if (entry.Value == null)
            {
                _startingWaypoints.Add(entry.Key);
            }
        }
        return _startingWaypoints;
    }

    public void RegisterWaypoint(Waypoint waypoint, Vector2Int position)
    {
        _dirtyWaypoints = true;
        var tileData = GetTileData(position);

        if (!Walkable.HasTile((Vector3Int)WorldTilePositionToGridTilePosition(position)))
        {
            Debug.LogError("Cannot register waypoint " + waypoint + " at " + position + " as a walkable tile does not exist there!");
            return;
        }

        _objectTilePositions[waypoint] = position;
        _waypoints.Add(waypoint);

        SnapWaypointToGrid(waypoint, position);

        Debug.Log("Registered " + waypoint + " at " + position + ".");

        PubSubSender.Publish("grid.Waypoint.registered", waypoint);

    }

    public void UnregisterWaypoint(Waypoint waypoint)
    {
        _dirtyWaypoints = true;
        var position = _objectTilePositions[waypoint];
        _objectTilePositions.Remove(waypoint);
        _waypoints.Remove(waypoint);

        var data = GetTileData(position);
        data.Waypoints.Remove(waypoint);
        UpdateTileData(data);

        PubSubSender.Publish("grid.Waypoint.unregistered", waypoint);
    }

    public void RegisterSummon(Summon Summon, Vector2Int position)
    {
        var tileData = GetTileData(position);
        if (tileData.Summon != null)
        {
            Debug.LogError("Cannot register Summon " + Summon + " at " + position + " as " + tileData.Summon + " is already occupying that space!");
            return;
        }

        if (!Walkable.HasTile((Vector3Int)WorldTilePositionToGridTilePosition(position)))
        {
            Debug.LogError("Cannot register Summon " + Summon + " at " + position + " as a walkable tile does not exist there!");
            return;
        }

        _objectTilePositions[Summon] = position;
        _summons.Add(Summon);

        SnapSummonToGrid(Summon, position);
        _setSummonPositions_unsafe(Summon, position);

        Debug.Log("Registered " + Summon + " at " + position + ".");

        PubSubSender.Publish("grid.Summon.registered", Summon);
    }

    public void UnregisterSummon(Summon Summon)
    {
        var position = _objectTilePositions[Summon];
        _objectTilePositions.Remove(Summon);
        _summons.Remove(Summon);

        var data = GetTileData(position);
        data.Summon = null;
        UpdateTileData(data);

        PubSubSender.Publish("grid.Summon.position.changed", Summon);
        PubSubSender.Publish("grid.Summon.unregistered", Summon);
    }

    public TileData? SetSummonPosition(Summon Summon, Vector2Int position)
    {
        if (!_objectTilePositions.ContainsKey(Summon))
        {
            Debug.Log("Unable to set position for Summon that has not been registered.");
            return null;
        }

        return _setSummonPositions_unsafe(Summon, position);
    }

    private TileData? _setSummonPositions_unsafe(Summon Summon, Vector2Int position)
    {
        var oldPosition = _objectTilePositions[Summon];
        _objectTilePositions[Summon] = position;

        var oldTileData = GetTileData(oldPosition);
        oldTileData.Summon = null;
        UpdateTileData(oldTileData);

        var newTileData = GetTileData(position);
        newTileData.Summon = Summon;
        UpdateTileData(newTileData);

        PubSubSender.Publish("grid.Summon.position.changed", Summon);

        return newTileData;
    }

    public Summon GetSummon(Vector2Int position)
    {
        return GetTileData(position).Summon;
    }

    public bool HasSummon(Vector2Int position)
    {
        return GetTileData(position).Summon != null;
    }

    void SnapSummonToGrid(Summon Summon, Vector2Int position)
    {
        Summon.transform.position = TileCoordinateToWorldPosition(position);
    }

    void SnapWaypointToGrid(Waypoint waypoint, Vector2Int position)
    {
        waypoint.transform.position = TileCoordinateToWorldPosition(position);
    }


    public List<Vector2Int> CalculatePath(Vector2Int startPosition, Vector2Int endPosition, int range = 0, int maxRange = 0, bool alwaysIncludeTarget = false, bool debugVisuals = false, bool ignoringObstacles = false) 
    {
        return CalculatePath((Vector3Int)startPosition, (Vector3Int)endPosition, range:range, maxRange: maxRange, debugVisuals: debugVisuals, alwaysIncludeTarget: alwaysIncludeTarget, ignoringObstacles: ignoringObstacles);
    }


    public List<Vector2Int> CalculatePath(Vector3Int startPosition, Vector3Int endPosition, int range = 0, int maxRange = 0, bool alwaysIncludeTarget = false, bool debugVisuals = false, bool ignoringObstacles = false)
    {
        if (startPosition == endPosition)
        {
            var quickPath = new List<Vector2Int>();
            quickPath.Add((Vector2Int)startPosition);
            return quickPath;
        }

        Dictionary<Vector3Int, Vector3Int> visited = new Dictionary<Vector3Int, Vector3Int>();
        Queue<Vector3Int> queue = new Queue<Vector3Int>();

        foreach (var neighbor in NeighborsForTileAtPosition(startPosition, target: endPosition, alwaysIncludeTarget: alwaysIncludeTarget, ignoringObstacles: ignoringObstacles))
        {
            visited[neighbor] = startPosition;
            queue.Enqueue(neighbor);
        }

        bool pathFound = false;
        while (queue.Count > 0)
        {
            var currentTile = queue.Dequeue();
            if (currentTile == endPosition) { 
                pathFound = true;
                break;
            }
            else
            {
                var neighbors = NeighborsForTileAtPosition(currentTile, target: endPosition, alwaysIncludeTarget: alwaysIncludeTarget, ignoringObstacles: ignoringObstacles);
                foreach (var neighbor in neighbors)
                {
                    if (visited.ContainsKey(neighbor)) { continue; }
                    visited[neighbor] = currentTile;
                    queue.Enqueue(neighbor);
                }
            }
        }

        if (!pathFound)
        {
            return new List<Vector2Int>();
        }

        var path = BuildPath(startPosition, endPosition, visited);

        if (range > 0)
        {
            path = path.GetRange(0, Mathf.Min(range + 1, path.Count));
        }

        if (maxRange > 0)
        {
            if (path.Count > maxRange)
            {
                return new List<Vector2Int>();
            }
        }

        return path;
    }

    public List<List<Vector2Int>> CalculateWaypointPaths(Waypoint startingWaypoint, bool alwaysIncludeTarget = false, bool debugVisuals = false, bool ignoringObstacles = false)
    {
        if (startingWaypoint.Next.Count == 0)
        {
            var quickPath = new List<List<Vector2Int>>();
            quickPath.Add(new List<Vector2Int>());
            return quickPath;
        }

        List<List<Vector2Int>> paths = new List<List<Vector2Int>>();
        foreach (var nextWaypoint in startingWaypoint.Next)
        {
            // Find the path from the current waypoint to one of the next waypoints, starting from the specified direction
            Vector3Int originalStartPosition = (Vector3Int)PositionForWaypoint(startingWaypoint);
            Vector3Int startPosition = originalStartPosition;

            switch (nextWaypoint.Direction)
            {
                case WaypointDirection.North:
                    startPosition += Vector3Int.up;
                    break;
                case WaypointDirection.East:
                    startPosition += Vector3Int.right;
                    break;
                case WaypointDirection.South:
                    startPosition += Vector3Int.down;
                    break;
                case WaypointDirection.West:
                    startPosition += Vector3Int.left;
                    break;
                default:
                    break;
            }

            Vector3Int endPosition = (Vector3Int)PositionForWaypoint(nextWaypoint.Waypoint);

            if (startPosition == endPosition)
            {
                var quickPath = new List<Vector2Int>();
                paths.Add(quickPath);
                continue;
            }

            Dictionary<Vector3Int, Vector3Int> visited = new Dictionary<Vector3Int, Vector3Int>();
            Queue<Vector3Int> queue = new Queue<Vector3Int>();

            foreach (var neighbor in NeighborsForTileAtPosition(startPosition, target: endPosition, alwaysIncludeTarget: alwaysIncludeTarget, ignoringObstacles: ignoringObstacles))
            {
                visited[neighbor] = startPosition;
                if (neighbor != originalStartPosition)
                {
                    queue.Enqueue(neighbor);
                }
            }

            bool pathFound = false;
            while (queue.Count > 0)
            {
                var currentTile = queue.Dequeue();
                if (currentTile == endPosition)
                {
                    pathFound = true;
                    break;
                }
                else
                {
                    var neighbors = NeighborsForTileAtPosition(currentTile, target: endPosition, alwaysIncludeTarget: alwaysIncludeTarget, ignoringObstacles: ignoringObstacles);
                    foreach (var neighbor in neighbors)
                    {
                        if (visited.ContainsKey(neighbor)) { continue; }
                        visited[neighbor] = currentTile;
                        queue.Enqueue(neighbor);
                    }
                }
            }

            if (!pathFound)
            {
                var quickPath = new List<Vector2Int>();
                paths.Add(quickPath);
                continue;
            }

            var pathToCurrentWaypoint = BuildPath(startPosition, endPosition, visited);

            var pathToNextWaypoints = CalculateWaypointPaths(nextWaypoint.Waypoint, alwaysIncludeTarget: alwaysIncludeTarget, debugVisuals: debugVisuals, ignoringObstacles: ignoringObstacles);
            foreach (var pathToNextWaypoint in pathToNextWaypoints)
            {
                var truePath = pathToCurrentWaypoint.Concat(pathToNextWaypoint).ToList();
                paths.Add(truePath);
            }
        }

        return paths;
    }

    public List<Vector2Int> CalculateAllPaths(Vector2Int startPosition, Vector2Int endPosition, int range = 0, int maxRange = 0, bool alwaysIncludeTarget = false, bool debugVisuals = false, bool ignoringObstacles = false)
    {
        return CalculateAllPaths((Vector3Int)startPosition, (Vector3Int)endPosition, range: range, maxRange: maxRange, debugVisuals: debugVisuals, alwaysIncludeTarget: alwaysIncludeTarget, ignoringObstacles: ignoringObstacles);
    }

    public List<Vector2Int> CalculateAllPaths(Vector3Int startPosition, Vector3Int endPosition, int range = 0, int maxRange = 0, bool alwaysIncludeTarget = false, bool debugVisuals = false, bool ignoringObstacles = false)
    {
        if (startPosition == endPosition)
        {
            var quickPath = new List<Vector2Int>();
            quickPath.Add((Vector2Int)startPosition);
            return quickPath;
        }

        Dictionary<Vector3Int, Vector3Int> visited = new Dictionary<Vector3Int, Vector3Int>();
        Queue<Vector3Int> queue = new Queue<Vector3Int>();

        foreach (var neighbor in NeighborsForTileAtPosition(startPosition, target: endPosition, alwaysIncludeTarget: alwaysIncludeTarget, ignoringObstacles: ignoringObstacles))
        {
            visited[neighbor] = startPosition;
            queue.Enqueue(neighbor);
        }

        bool pathFound = false;
        while (queue.Count > 0)
        {
            var currentTile = queue.Dequeue();
            var neighbors = NeighborsForTileAtPosition(currentTile, target: endPosition, alwaysIncludeTarget: alwaysIncludeTarget, ignoringObstacles: ignoringObstacles);
            foreach (var neighbor in neighbors)
            {
                if (visited.ContainsKey(neighbor)) { continue; }
                visited[neighbor] = currentTile;
                queue.Enqueue(neighbor);
            }
        }

        if (!pathFound)
        {
            return new List<Vector2Int>();
        }

        var path = BuildPath(startPosition, endPosition, visited);

        if (range > 0)
        {
            path = path.GetRange(0, Mathf.Min(range + 1, path.Count));
        }

        if (maxRange > 0)
        {
            if (path.Count > maxRange)
            {
                return new List<Vector2Int>();
            }
        }

        return path;
    }

    public HashSet<Vector2Int> BFS(Vector3Int startPosition, int range, bool ignoringObstacles = false)
    {
        if (range < 0)
        {
            Debug.LogError("Bruh.");
            return new HashSet<Vector2Int>();
        }

        if (range == 0)
        {
            var quickPath = new HashSet<Vector2Int>();
            quickPath.Add((Vector2Int)startPosition);
            return quickPath;
        }

        Dictionary<Vector3Int, int> visited = new Dictionary<Vector3Int, int>();
        Queue<Vector3Int> queue = new Queue<Vector3Int>();

        foreach (var neighbor in NeighborsForTileAtPosition(startPosition, ignoringObstacles: ignoringObstacles))
        {
            visited[neighbor] = 1;
            queue.Enqueue(neighbor);
        }

        while (queue.Count > 0)
        {
            var currentTile = queue.Dequeue();
            if (visited[currentTile] > range)
            {
                break;
            }
            else
            {
                var neighbors = NeighborsForTileAtPosition(currentTile, ignoringObstacles: ignoringObstacles);
                foreach (var neighbor in neighbors)
                {
                    if (visited.ContainsKey(neighbor)) { continue; }
                    visited[neighbor] = visited[currentTile] + 1;
                    queue.Enqueue(neighbor);
                }
            }
        }

        HashSet<Vector2Int> results = new();
        foreach (var (position, distance) in visited)
        {
            if (distance <= range)
            {
                results.Add((Vector2Int)position);
            }
        }

        return results;
    }

    public List<Vector2Int> OrderedBFS(Vector3Int startPosition, int range, bool ignoringObstacles = false)
    {
        if (range < 0)
        {
            Debug.LogError("Bruh.");
            return new List<Vector2Int>();
        }

        if (range == 0)
        {
            var quickPath = new List<Vector2Int>();
            quickPath.Add((Vector2Int)startPosition);
            return quickPath;
        }

        List<Vector2Int> results = new();
        Dictionary<Vector3Int, int> visited = new Dictionary<Vector3Int, int>();
        Queue<Vector3Int> queue = new Queue<Vector3Int>();

        foreach (var neighbor in NeighborsForTileAtPosition(startPosition, ignoringObstacles: ignoringObstacles))
        {
            visited[neighbor] = 1;
            queue.Enqueue(neighbor);
        }

        while (queue.Count > 0)
        {
            var currentTile = queue.Dequeue();
            if (visited[currentTile] > range)
            {
                break;
            }
            else
            {
                results.Add((Vector2Int)currentTile);
                var neighbors = NeighborsForTileAtPosition(currentTile, ignoringObstacles: ignoringObstacles);
                foreach (var neighbor in neighbors)
                {
                    if (visited.ContainsKey(neighbor)) { continue; }
                    visited[neighbor] = visited[currentTile] + 1;
                    queue.Enqueue(neighbor);
                }
            }
        }

        return results;
    }

    List<Vector2Int> BuildPath(Vector3Int startPosition, Vector3Int endPosition, Dictionary<Vector3Int, Vector3Int> visited)
    {
        List<Vector2Int> path = new List<Vector2Int>();

        var currentTile = endPosition;

        while (currentTile != startPosition) {
            path.Add((Vector2Int)currentTile);
            currentTile = visited[currentTile];
        }
        path.Add((Vector2Int)startPosition);

        path.Reverse();
        return path;
    }

    bool TileIsWalkable(Vector3Int tilePosition, Vector3Int? target, bool alwaysIncludeTarget, bool ignoringObstacles)
    {
        var tileExists = Walkable.HasTile(WorldTilePositionToGridTilePosition(tilePosition));

        var obstaclesInTheWay = false;
        if (!ignoringObstacles) {
            // Detect if there are any obstacles
        }

        var overrideObstacleCheck = false;
        if (target != null && alwaysIncludeTarget)
        {
            overrideObstacleCheck = (target.Value == tilePosition);
        }

        var tileConfig = GetTileConfig(((Vector2Int)tilePosition));

        var tileCanBeWalkedOn = false;
        if (tileConfig != null)
        {
            tileCanBeWalkedOn = tileConfig.walkable;
        }

        return tileExists && (!obstaclesInTheWay || overrideObstacleCheck) && tileCanBeWalkedOn;
    }

    public List<Vector3Int> NeighborsForTileAtPosition(Vector3Int tilePosition, Vector3Int? target = null, bool includeDiagonal = false, bool alwaysIncludeTarget = false, bool ignoringObstacles = false)
    {
        var neighborPositions = new List<Vector3Int>();

        var northPos = tilePosition + new Vector3Int(0, -1, 0);
        var eastPos = tilePosition + new Vector3Int(1, 0, 0);
        var southPos = tilePosition + new Vector3Int(0, 1, 0);
        var westPos = tilePosition + new Vector3Int(-1, 0, 0);
        if (TileIsWalkable(northPos, target, alwaysIncludeTarget, ignoringObstacles)) { neighborPositions.Add(northPos); }
        if (TileIsWalkable(eastPos, target, alwaysIncludeTarget, ignoringObstacles)) { neighborPositions.Add(eastPos); }
        if (TileIsWalkable(southPos, target, alwaysIncludeTarget, ignoringObstacles)) { neighborPositions.Add(southPos); }
        if (TileIsWalkable(westPos, target, alwaysIncludeTarget, ignoringObstacles)) { neighborPositions.Add(westPos); }

        if (includeDiagonal)
        {
            var northEastPos = tilePosition + new Vector3Int(1, -1, 0);
            var southEastPos = tilePosition + new Vector3Int(1, 1, 0);
            var northWestPos = tilePosition + new Vector3Int(-1, -1, 0);
            var southWestPos = tilePosition + new Vector3Int(-1, 1, 0);
            if (TileIsWalkable(northEastPos, target, alwaysIncludeTarget, ignoringObstacles)) { neighborPositions.Add(northEastPos); }
            if (TileIsWalkable(southEastPos, target, alwaysIncludeTarget, ignoringObstacles)) { neighborPositions.Add(southEastPos); }
            if (TileIsWalkable(northWestPos, target, alwaysIncludeTarget, ignoringObstacles)) { neighborPositions.Add(northWestPos); }
            if (TileIsWalkable(southWestPos, target, alwaysIncludeTarget, ignoringObstacles)) { neighborPositions.Add(southWestPos); }
        }

        return neighborPositions;
    }

    public Vector3 TileCoordinateToWorldPosition(Vector2Int position)
    {
        return new Vector3(position.x + _tileCenterOffset, position.y + _tileCenterOffset, 0.0f);
    }

    public Vector2Int ToWorldPositionTileCoordinate(Vector3 position)
    {
        return new Vector2Int(Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.y));
    }
}
