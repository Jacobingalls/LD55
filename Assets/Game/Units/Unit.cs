using info.jacobingalls.jamkit;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(PubSubSender))]
public class Unit : MonoBehaviour
{
    public UnitDefinition Definition;
    public GridManager GridManager;

    public float MoveSpeed
    {
        get
        {
            return Definition.MoveSpeed;
        }
    }

    public string Name
    {
        get
        {
            return Definition.Name;
        }
    }

    public string ClassName
    {
        get
        {
            return Definition.ClassName;
        }
    }

    public int Health
    {
        get { return _health; }
        private set
        {
            if (_health == value) { return; }
            _health = value;
            _pubSubSender.Publish("unit.health.changed", _health);
            _pubSubSender.Publish("unit.resources.changed", _health);
        }
    }
    [SerializeField]
    private int _health;

    public int MaxHealth { get { return Definition.BaseMaxHealth; } }

    private PubSubSender _pubSubSender;

    // Start is called before the first frame update
    void Awake()
    {
        _pubSubSender = GetComponent<PubSubSender>();
        if (GridManager == null)
        {
            // PANIK, try to find one
            GridManager = GameObject.FindObjectOfType<GridManager>();
        }

        Health = MaxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Move(List<Vector2Int> path)
    {
        if (moving) { return; }
        if (path == null) { return; }
        if (path.Count == 0) { return; }

        moving = true;
        StartCoroutine(MoveAlongPath(new List<Vector2Int>(path)));
    }

    bool moving = false;

    private bool positionIsCloseEnoughToTarget(Vector3 destination)
    {
        return Vector3.Distance(transform.position, destination) < 0.005f;
    }

    IEnumerator MoveAlongPath(List<Vector2Int> path)
    {
        Vector2Int startNode = path.First();
        path.RemoveAt(0);

        Vector3 startPosition = GridManager.TileCoordinateToWorldPosition(startNode);

        startNode = path.First();
        path.RemoveAt(0);
        Vector3 nextPosition = GridManager.TileCoordinateToWorldPosition(startNode);

        var t = 0.0f;
        var speed = MoveSpeed; // meters per second
        var timeToWalkAcrossTile = 1 / speed;

        while (!positionIsCloseEnoughToTarget(nextPosition) || path.Count > 0)
        {
            t += Time.deltaTime;
            float progress = t / timeToWalkAcrossTile;

            var direction = new Vector3(nextPosition.x - startPosition.x, nextPosition.y - startPosition.y, 0.0f).normalized;
            transform.position = new Vector3(startPosition.x, startPosition.y, transform.position.z) + (direction * progress); // ew, becky. ew.

            if (t > timeToWalkAcrossTile)
            {
                if (path.Count > 0)
                {
                    t = 0.0f;
                    startPosition = nextPosition;
                    nextPosition = GridManager.TileCoordinateToWorldPosition(path.First());
                    path.RemoveAt(0);
                }
            }

            yield return null;
        }

        moving = false;
        transform.position = nextPosition;
    }
}
