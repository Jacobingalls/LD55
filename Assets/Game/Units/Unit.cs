using info.jacobingalls.jamkit;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(PubSubSender))]
public class Unit : MonoBehaviour, IDamageable
{
    public UnitDefinition Definition;
    public World World;
    public Material DeathShader;

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

    public bool Alive
    {
        get
        {
            return _alive;
        }
    }
    private bool _alive = true;

    public int MaxHealth { get { return Definition.BaseMaxHealth; } }

    private PubSubSender _pubSubSender;

    // Start is called before the first frame update
    void Awake()
    {
        _pubSubSender = GetComponent<PubSubSender>();
        if (World == null)
        {
            // PANIK, try to find one
            World = GameObject.FindObjectOfType<World>();
        }

        Health = MaxHealth;

        World.RegisterUnit(this);
    }

    private void OnDestroy()
    {
        World.UnregisterUnit(this);
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

        Vector3 startPosition = World.GridManager.TileCoordinateToWorldPosition(startNode);

        startNode = path.First();
        path.RemoveAt(0);
        Vector3 nextPosition = World.GridManager.TileCoordinateToWorldPosition(startNode);

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
                    nextPosition = World.GridManager.TileCoordinateToWorldPosition(path.First());
                    path.RemoveAt(0);
                }
            }

            yield return null;
        }

        moving = false;
        transform.position = nextPosition;
    }

    public void Damage(IDamageSource damageSource)
    {
        Health -= damageSource.GetDamageAmount();

        if (Health <= 0)
        {
            Kill();
        }
    }

    void PlayDeathAudio()
    {
        //AudioManager.Instance.Play("SFX/GenericDeath",
        //    pitchMin: 0.9f, pitchMax: 1.1f,
        //    volumeMin: 1.0f, volumeMax: 1.0f,
        //    position: transform.position,
        //    minDistance: 10, maxDistance: 20);
    }

    void PlayHurtAudio()
    {
        //AudioManager.Instance.Play("SFX/GenericHurt",
        //    pitchMin: 0.9f, pitchMax: 1.1f,
        //    volumeMin: 0.7f, volumeMax: 0.7f,
        //    position: transform.position,
        //    minDistance: 10, maxDistance: 20);
    }

    public void Kill()
    {
        if (!_alive)
        {
            return;
        }

        _alive = false;
        Health = 0;

        GetComponent<PubSubSender>().Publish("unit.slain");

        StartCoroutine(DeathCoroutine());
    }

    [Range(0.0f, 5.0f)]
    public float DeathAnimationTime = 0.5f;

    public bool PlayDeathAnimation = true;

    private IEnumerator DeathCoroutine()
    {
        PlayDeathAudio();

        if (PlayDeathAnimation == false)
        {
            _pubSubSender.Publish("entity.died", this);
            Destroy(gameObject);
            yield break;
        }

        var spriteRenderers = GetComponentsInChildren<SpriteRenderer>();

        foreach (var sr in spriteRenderers)
        {
            var dissolveEffect = sr.gameObject.AddComponent<DissolveEffect>();
            dissolveEffect.DissolveMaterial = DeathShader;
            dissolveEffect.DissolveAmount = 0;
            dissolveEffect.Duration = DeathAnimationTime;
            dissolveEffect.IsDissolving = true;
        }

        yield return new WaitForSeconds(DeathAnimationTime);

        _pubSubSender.Publish("unit.died", this);

        Destroy(gameObject);
    }
}
